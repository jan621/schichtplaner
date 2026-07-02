using System.Collections;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Identity;
using Microsoft.AspNetCore.Identity;

namespace UserManagement.Contract;

public interface IUserManager
{
    Task CreateUser(SignUpRequest request);

    Task<SignInResult> LoginUser(SignInRequest request);

    Task ConfirmUserEmail(string email);

    Task SetPasswordReset(string email);

    Task ConfirmUserByAdmin(string email);

    Task ResetPassword(PasswordResetRequest passwordResetRequest);

    Task CreateBasicRolesAsync();

    Task ReleaseUsers(IEnumerable<User> hosts);

    Task SetUserSessionAsync(UserSession userSession);
    
    Task<UserSession> GetUserSessionAsync();

    Task<string> GetUserIdAsync();

    Task<User> GetCurrentUserAsync();

}