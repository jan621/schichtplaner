using System.Collections.Concurrent;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Identity;
using Microsoft.AspNetCore.Identity;

namespace planer.Identity;

public class LoginMiddleware
{
    public static IDictionary<Guid, SignInRequest> Logins { get; private set; }
        = new ConcurrentDictionary<Guid, SignInRequest>();


    private readonly RequestDelegate _next;

    public LoginMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, SignInManager<User> signInMgr)
    {
        if (context.Request.Path == "/login" && context.Request.Query.ContainsKey("key"))
        {
            var key = Guid.Parse(context.Request.Query["key"]);
            var info = Logins[key];
            var result = await signInMgr.PasswordSignInAsync(info.Email, info.Password, info.RememberMe, lockoutOnFailure: true);
            info.Password = null;
            if (result.Succeeded)
            {
                Logins.Remove(key);
                context.Response.Redirect("/");
                return;
            }
            else
            {
                context.Response.Redirect("/loginfailed");
                return;
            }
        }
        else if (context.Request.Path == "/logout")
        {
            await signInMgr.SignOutAsync();
            context.Response.Redirect("/");
        }
        else
        {
            await _next.Invoke(context);
        }
    }
}