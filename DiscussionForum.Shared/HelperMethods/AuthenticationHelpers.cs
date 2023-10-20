using DiscussionForum.Shared.Models.Errors;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace DiscussionForum.Shared.HelperMethods;

public static class AuthenticationHelpers
{
    public static Guid GetUserId(this ClaimsPrincipal? user)
    {
        string? userId = user?.FindFirst(ClaimConstants.IdClaimName)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new ForbiddenException();
        }
        return Guid.Parse(userId);
    }

    public static Guid? TryGetUserId(this ClaimsPrincipal? user)
    {
        string? userId = user?.FindFirst(ClaimConstants.IdClaimName)?.Value;
        return string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);
    }

    public static Role GetUserRole(this ClaimsPrincipal? user)
    {
        string? userRole = user?.FindFirst(ClaimConstants.RoleClaimName)?.Value;
        if (string.IsNullOrEmpty(userRole))
        {
            throw new ForbiddenException();
        }
        return Enum.Parse<Role>(userRole);
    }

    public static string GetUserName(this ClaimsPrincipal user)
    {
        string? userName = user?.FindFirst(ClaimConstants.UserNameClaimName)?.Value;
        if (string.IsNullOrEmpty(userName))
        {
            throw new ForbiddenException();
        }
        return userName;
    }


    public static async Task<UserInfo> GetUserInfo(this Task<AuthenticationState> authenticationStateTask)
    {
        AuthenticationState authenticationState = await authenticationStateTask;
        return new UserInfo()
        {
            IsAuthenticated = authenticationState.User.Identity?.IsAuthenticated == true,
            Claims = authenticationState.User.Claims.Select(x => new ClaimValue(x.Type, x.Value)).ToList(),
        };
    }

    public static Guid GetUserId(this UserInfo userInfo)
    {
        string? userId = userInfo?.Claims.FirstOrDefault(x => x.Type == ClaimConstants.IdClaimName)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new ForbiddenException();
        }
        return Guid.Parse(userId);
    }

    public static Guid? TryGetUserId(this UserInfo userInfo)
    {
        string? userId = userInfo?.Claims.FirstOrDefault(x => x.Type == ClaimConstants.IdClaimName)?.Value;
        return string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);
    }

    public static string GetUserEmail(this UserInfo userInfo)
    {
        string? email = userInfo?.Claims.FirstOrDefault(x => x.Type == ClaimConstants.EmailNameClaimName)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            throw new ForbiddenException();
        }
        return email;
    }

    public static Role? GetUserRole(this UserInfo userInfo)
    {
        string? role = userInfo?.Claims.FirstOrDefault(x => x.Type == ClaimConstants.RoleClaimName)?.Value;
        return string.IsNullOrEmpty(role) ? null : Enum.Parse<Role>(role);
    }

    public static string? GetUserName(this UserInfo userInfo)
    {
        string? userName = userInfo?.Claims.FirstOrDefault(x => x.Type == ClaimConstants.UserNameClaimName)?.Value;
        return userName;
    }
}
