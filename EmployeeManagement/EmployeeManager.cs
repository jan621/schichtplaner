using System.Net;
using System.Text;
using AutoMapper;
using CrossCutting.DataObjects;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Enums;
using CrossCutting.Identity;
using DatabaseManagement.Contract;
using Datastoring.EfCore;
using EmployeeManagement.Contract;
using MailManagement.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrganizationManagement.Contract;
using UserManagement.Contract;

namespace EmployeeManagement;

public class EmployeeManager(
    IDatabaseManager<User, PlanerIdentityContext> databaseManager,
    IMapper mapper,
    IUserManager userManager,
    UserManager<User> identityUserManager,
    IMailManager mailManager,
    IOrganizationManager organizationManager)
    : IEmployeeManager
{
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var currentUser = await userManager.GetCurrentUserAsync();
        var leader = await organizationManager.GetOrganizationLeaderAsync(currentUser.Organization);
        var dbEmployees = (List<User>)await databaseManager.GetAllAsync(true);
        var result = dbEmployees.Where(e => e.Id != currentUser.Id && e.Id != leader?.Id
                                                                   && e.Organization?.Id == currentUser.Organization.Id);
        return result;
    }

    public async Task<bool> AddAsync(User employees)
    {
        var created = await databaseManager.AddAsync(employees);

        return created;
    }

    public async Task<string> InviteEmployeeAsync(SignUpEmployeeRequest request)
    {
        var user = mapper.Map<User>(request);

        var dbUser = await identityUserManager.Users
            .SingleOrDefaultAsync(u => u.Email == request.Email);

        if (dbUser != null)
            throw new Exception(HttpStatusCode.Conflict.ToString());

        var generatedPassword = GeneratePassword();

        var identityResult = await identityUserManager.CreateAsync(user, generatedPassword);

        if (identityResult != IdentityResult.Success)
            throw new Exception("Registration failed");

        //Add role
        var createdUser = await identityUserManager.Users.SingleOrDefaultAsync(u => u.Email == user.Email);
        identityResult = await identityUserManager.AddToRoleAsync(createdUser!, request.Role.ToString());

        if (identityResult != IdentityResult.Success)
            throw new Exception("Role assertion failed");

        //---

        var mailTemplate = mailManager.GetMailTemplateByType(MailTemplateType.EmployeeInvitation);

        var mailMessage = new MailMessage()
        {
            To = request.Email,
            Subject = mailTemplate?.Subject,
            Body = mailTemplate?.Body
        };

        var convertedMail = user.Email.Replace("@", "(at)");
        convertedMail = convertedMail.Replace(".", "(point)");
        mailMessage.Body = mailMessage.Body?
            .Replace("{0}", user.FirstName)
            .Replace("{1}", user.Organization.Name)
            .Replace("{2}", convertedMail)
            .Replace("{3}", generatedPassword);

        await mailManager.SendAsync(mailMessage);

        return generatedPassword;
    }

    public async Task ReleaseEmployeeAsync(User employee)
    {
        employee.EmailConfirmed = true;
        employee.Activated = true;
        await identityUserManager.UpdateAsync(employee);

        var mailTemplate = mailManager.GetMailTemplateByType(MailTemplateType.AccountActivatedByHost);

        var mailMessage = new MailMessage
        {
            To = employee.Email,
            Subject = mailTemplate?.Subject,
            Body = mailTemplate?.Body?.Replace("{0}", employee.FirstName)
        };

        await mailManager.SendAsync(mailMessage);
    }

    public async Task<bool> EditAsync(User user)
    {
        var edited = await databaseManager.UpdateAsync(user);

        return edited;
    }

    public async Task<bool> DeleteAsync(User employees)
    {
        var deleted = await databaseManager.DeleteAsync(employees);

        return deleted;
    }

    public async Task ConfirmEmployee(string employeeMail)
    {
        var employees = await databaseManager.GetAllAsync(true);
        var employee = employees.Single(e => e.Email == employeeMail);

        employee.EmailConfirmed = true;
        employee.Activated = true;
        await identityUserManager.UpdateAsync(employee);
    }

    private string GeneratePassword()
    {
        const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+";
        var random = new Random();
        var password = new StringBuilder();

        var randomUpperCase = (char)random.Next('A', 'Z' + 1);
        var randomLowerCase = (char)random.Next('a', 'z' + 1);
        var randomDigit = (char)random.Next('0', '9' + 1);
        var randomSpecialChar = allowedChars[random.Next(allowedChars.Length)];

        password.Append(randomUpperCase);
        password.Append(randomLowerCase);
        password.Append(randomDigit);
        password.Append(randomSpecialChar);

        for (var i = 4; i < 12; i++)
        {
            password.Append(allowedChars[random.Next(allowedChars.Length)]);
        }

        var shuffledPassword = new string(password.ToString().ToCharArray().OrderBy(c => Guid.NewGuid()).ToArray());
        return shuffledPassword;
    }
}