using System.Security.Claims;

namespace DiscussionForum.Client.Authentication;
public sealed class PersistentAuthenticationStateProvider(PersistentComponentState persistentState) : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> _unauthenticatedTask =
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!persistentState.TryTakeFromJson(nameof(UserInfo), out UserInfo? userInfo) || userInfo is null)
        {
            return _unauthenticatedTask;
        }

        Claim[] claims = [
            new Claim(ClaimConstants.IdClaimName, userInfo.GetUserId().ToString()),
            new Claim(ClaimConstants.EmailNameClaimName, userInfo.GetUserEmail()),
            new Claim(ClaimConstants.RoleClaimName, userInfo.GetUserRole().ToString() ?? ""),
            new Claim(ClaimConstants.UserNameClaimName, userInfo.GetUserName() ?? "")];

        return Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "EasyAuth"))));
    }
}

