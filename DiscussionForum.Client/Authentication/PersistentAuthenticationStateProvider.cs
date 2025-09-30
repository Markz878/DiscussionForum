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
            new Claim(ClaimConstants.IdClaimName, userInfo.Id.ToString()),
            new Claim(ClaimConstants.EmailNameClaimName, userInfo.Email),
            new Claim(ClaimConstants.RoleClaimName, userInfo.Role.ToString()),
            new Claim(ClaimConstants.UserNameClaimName, userInfo.UserName)];

        return Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "EasyAuth"))));
    }
}

