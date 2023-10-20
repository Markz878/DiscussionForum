namespace DiscussionForum.Server.Installers;

public class AuthenticationInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication().AddAzureContainerAppsEasyAuth();
        builder.Services.AddAuthorization(x => x.AddPolicy("HasUserName", p => p.RequireAuthenticatedUser().RequireClaim(ClaimConstants.UserNameClaimName)));
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
    }
}
