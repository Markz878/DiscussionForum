using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using System.Diagnostics;

namespace DiscussionForum.Server.HelperMethods;

public class PersistingServerAuthenticationStateProvider : ServerAuthenticationStateProvider, IDisposable
{
    private readonly PersistentComponentState _state;

    private readonly PersistingComponentStateSubscription _subscription;

    private Task<AuthenticationState>? _authenticationStateTask;

    public PersistingServerAuthenticationStateProvider(PersistentComponentState state)
    {
        _state = state;
        AuthenticationStateChanged += OnAuthenticationStateChanged;
        _subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> authenticationStateTask)
    {
        _authenticationStateTask = authenticationStateTask;
    }

    private async Task OnPersistingAsync()
    {
        if (_authenticationStateTask is null)
        {
            throw new UnreachableException($"Authentication state not set in {nameof(RevalidatingServerAuthenticationStateProvider)}.{nameof(OnPersistingAsync)}().");
        }

        AuthenticationState authenticationState = await _authenticationStateTask;
        ClaimsPrincipal principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
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
    }

    public void Dispose()
    {
        _subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
