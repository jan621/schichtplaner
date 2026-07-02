using System.Net;
using AutoMapper;
using Blazored.LocalStorage;
using CrossCutting.DataObjects;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Enums;
using CrossCutting.Identity;
using DatabaseManagement.Contract;
using Datastoring.EfCore;
using MailManagement.Contract;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Contract;

namespace UserManagement;

public class UserManager(
    IMapper mapper,
    IMailManager mailManager,
    UserManager<User> identityUserManager,
    RoleManager<IdentityRole> identityRoleManager,
    SignInManager<User> signInManager,
    ILocalStorageService localStorageService,
    AuthenticationStateProvider getAuthenticationStateAsync,
    IDatabaseManager<User, PlanerIdentityContext> databaseManager)
    : IUserManager
{
    public async Task CreateUser(SignUpRequest request)
    {
        var user = mapper.Map<SignUpRequest, User>(request);
        user.Organization = new Organization
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.CompanyName,
            Users = new List<User>(),
            TimeTrackingGapHours = 8
        };

        var dbUser = await identityUserManager.Users
            .SingleOrDefaultAsync(u => u.Email == request.Email);

        if (dbUser != null)
            throw new Exception(HttpStatusCode.Conflict.ToString());

        var identityResult = await identityUserManager.CreateAsync(user, request.Password);

        if (identityResult != IdentityResult.Success)
            throw new Exception("Registration failed");

        
        //Add role
        var createdUser = await identityUserManager.Users.SingleOrDefaultAsync(u => u.Email == user.Email);
        identityResult = await identityUserManager.AddToRoleAsync(createdUser!, request.Role.ToString());
        
        if (identityResult != IdentityResult.Success)
            throw new Exception("Role assertion failed");
        
        //---

        var mailTemplate = mailManager.GetMailTemplateByType(MailTemplateType.ConfirmRegistration);

        var mailMessage = new MailMessage()
        {
            To = request.Email,
            Subject = mailTemplate?.Subject,
            Body = mailTemplate?.Body
        };

        var convertedMail = user.Email.Replace("@", "(at)");
        convertedMail = convertedMail.Replace(".", "(point)");
        mailMessage.Body = mailMessage.Body?.Replace("{0}", user.FirstName)
            .Replace("{1}", convertedMail);

        await mailManager.SendAsync(mailMessage);
    }

    public async Task<SignInResult> LoginUser(SignInRequest request)
    {
        var dbUser = await identityUserManager.Users
            .Include(u => u.Organization)
            .SingleOrDefaultAsync(u => u.Email == request.Email);

        if (dbUser == null)
            return SignInResult.Failed;

        if (!dbUser.Activated || !dbUser.EmailConfirmed)
            throw new Exception(HttpStatusCode.Forbidden.ToString());

        if (dbUser.Deleted)
            throw new Exception(HttpStatusCode.NotFound.ToString());

        if (await signInManager.CanSignInAsync(dbUser))
        {
            var result = await signInManager.CheckPasswordSignInAsync(dbUser, request.Password, true);

            if (result != SignInResult.Success) return result;
            
            var userSession = mapper.Map<UserSession>(dbUser);
            await SetUserSessionAsync(userSession);

            return result;
        }

        throw new Exception("Error occurred during signIn");
    }

    public async Task ConfirmUserEmail(string email)
    {
        var dbUser = await identityUserManager.Users.SingleOrDefaultAsync(u => u.Email == email);
        
        if (dbUser == null)
            throw new Exception(HttpStatusCode.NotFound.ToString());
        
        if(dbUser.EmailConfirmed)
            throw new Exception(HttpStatusCode.Conflict.ToString());
        
        dbUser.EmailConfirmed = true;
        
        await identityUserManager.UpdateAsync(dbUser);
    }

    public async Task SetPasswordReset(string email)
    {
        var dbUser = await identityUserManager.Users.SingleOrDefaultAsync(u => u.Email == email);
        
        if (dbUser == null)
            throw new Exception(HttpStatusCode.NotFound.ToString());
        
        if(!dbUser.EmailConfirmed)
            throw new Exception(HttpStatusCode.Conflict.ToString());
        
        var token = await identityUserManager.GeneratePasswordResetTokenAsync(dbUser);
        
        var mailTemplate = mailManager.GetMailTemplateByType(MailTemplateType.ResetPassword);
        
        var mailMessage = new MailMessage()
        {
            To = dbUser.Email,
            Subject = mailTemplate?.Subject,
            Body = mailTemplate?.Body
        };
        
        var convertedMail = email.Replace("@", "(at)");
        convertedMail = convertedMail.Replace(".", "(point)");
        mailMessage.Body = mailMessage.Body?.Replace("{0}", dbUser.FirstName)
            .Replace("{1}", convertedMail)
            .Replace("{2}", token);
        
        await mailManager.SendAsync(mailMessage);
    }

    public async Task ResetPassword(PasswordResetRequest request)
    {
        var dbUser = await identityUserManager.Users.SingleOrDefaultAsync(u => u.UserName == request.UserName);
        
        if (dbUser == null)
            throw new Exception(HttpStatusCode.NotFound.ToString());

        var result = await identityUserManager.ResetPasswordAsync(dbUser, request.Token, request.Password);

        if (!result.Succeeded)
            throw new Exception("Error at resetting password");
    }

    public Task ConfirmUserByAdmin(string email)
    {
        throw new NotImplementedException();
    }

    public async Task CreateBasicRolesAsync()
    {
        if (identityRoleManager.Roles.Any())
            return;

        await identityRoleManager.CreateAsync(new IdentityRole
        {
            Name = Role.Employee.ToString()
        });

        await identityRoleManager.CreateAsync(new IdentityRole
        {
            Name = Role.Company.ToString()
        });

        await identityRoleManager.CreateAsync(new IdentityRole
        {
            Name = Role.Administrator.ToString()
        });

        await identityRoleManager.CreateAsync(new IdentityRole
        {
            Name = Role.Developer.ToString(),
        });

        await identityRoleManager.CreateAsync(new IdentityRole
        {
            Name = Role.Management.ToString(),
        });
    }

    public async Task ReleaseUsers(IEnumerable<User> hosts)
    {
        foreach (var host in hosts)
        {
            if (host.Activated || host.Deleted)
                continue;

            host.Activated = true;

            await identityUserManager.UpdateAsync(host);
        }
    }

    public async Task SetUserSessionAsync(UserSession userSession)
    {
        await localStorageService.SetItemAsync("userSession", userSession);
    }
    
    public async Task<UserSession> GetUserSessionAsync()
    {
        var localStorageResult = await localStorageService.GetItemAsync<UserSession>("userSession");
        return localStorageResult;
    }

    public async Task<string> GetUserIdAsync()
    {
        var currentAuthState = await getAuthenticationStateAsync.GetAuthenticationStateAsync();
        var currentUserId = currentAuthState.User.FindFirst(c => c.Type.Contains("nameidentifier"))?.Value;
        return currentUserId ?? string.Empty;
    }

    public async Task<User> GetCurrentUserAsync()
    {
        var dbEmployees = (List<User>)await databaseManager.GetAllAsync(true);
        var currentUserId = await GetUserIdAsync();
        var user = dbEmployees.Single(e => e.Id == currentUserId);
        return user;
    }
}