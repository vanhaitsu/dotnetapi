using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Repositories.Entities;
using Repositories.Utils;

namespace API.Middlewares;

public class AccountStatusMiddleware : IMiddleware
{
    private readonly UserManager<Account> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountStatusMiddleware(UserManager<Account> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var identity = _httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;
        var currentUserId = AuthenticationTools.GetCurrentUserId(identity);

        if (currentUserId != null)
        {
            var user = await _userManager.FindByIdAsync(currentUserId.ToString()!);

            if (user != null && user.IsDeleted)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);

                var response = new
                {
                    isBlocking = true,
                    message = "Account has been deleted"
                };

                var jsonResponse = JsonConvert.SerializeObject(response);
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(jsonResponse);

                return;
            }
        }

        await next(context);
    }
}