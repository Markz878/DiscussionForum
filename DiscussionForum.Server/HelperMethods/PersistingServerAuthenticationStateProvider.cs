using Microsoft.AspNetCore.Components.Web;

namespace DiscussionForum.Server.HelperMethods;

public sealed class PersistingServerAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly PersistentComponentState _state;
    private readonly PersistingComponentStateSubscription _subscription;

    public PersistingServerAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor, PersistentComponentState state)
    {
        this.httpContextAccessor = httpContextAccessor;
        _state = state;
        _subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }


    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(httpContextAccessor.HttpContext?.User ?? new()));
    }

    private Task OnPersistingAsync()
    {
        ClaimsPrincipal? principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated == true)
        {
            string? userId = principal.FindFirst(ClaimConstants.IdClaimName)?.Value;
            string? email = principal.FindFirst(ClaimConstants.EmailNameClaimName)?.Value;

            if (userId != null && email != null)
            {
                _state.PersistAsJson(nameof(UserInfo), new UserInfo
                {
                    Claims = principal.Claims.Select(x => new ClaimValue(x.Type, x.Value)).ToArray(),
                    IsAuthenticated = true,
                });
            }
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}
