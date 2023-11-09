namespace DiscussionForum.Server.HelperMethods;

public static class DevelopmentAuthenticationHelpers
{
    private static bool _isAuthorized;

    public static void AddDevelopmentAuthentication(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/.auth/login/aad", (string post_login_redirect_uri) =>
            {
                _isAuthorized = true;
                return TypedResults.Redirect(post_login_redirect_uri);
            });
            app.MapGet("/.auth/logout", (string post_logout_redirect_uri) =>
            {
                _isAuthorized = false;
                return TypedResults.Redirect(post_logout_redirect_uri);
            });
            app.Use((context, next) =>
            {
                if (_isAuthorized)
                {
                    context.Request.Headers.TryAdd(EasyAuthAuthenticationHandler.EasyAuthPrincipalIDP, "aad");
                    context.Request.Headers.TryAdd(EasyAuthAuthenticationHandler.EasyAuthPrincipalName, "test.user@email.com");
                    context.Request.Headers.TryAdd(EasyAuthAuthenticationHandler.EasyAuthPrincipalID, "BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB");
                }
                return next();
            });
        }
    }
}
