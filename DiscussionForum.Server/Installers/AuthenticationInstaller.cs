namespace DiscussionForum.Server.Installers;

public class AuthenticationInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication().AddAzureContainerAppsEasyAuth();
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("HasUserName", p => p.RequireAuthenticatedUser().RequireClaim(ClaimConstants.UserNameClaimName));
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
    }
}
