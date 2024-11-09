using Microsoft.Extensions.Primitives;

namespace DiscussionForum.Server.Installers;

public sealed class SecurityHeadersMiddlewareInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
        builder.Services.AddSingleton<SecurityHeadersMiddleware>();
    }
}

public sealed class SecurityHeadersMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Response.Headers.Append("X-Content-Type-Options", new StringValues("nosniff"));
        context.Response.Headers.Append("X-Frame-Options", new StringValues("DENY"));
        context.Response.Headers.Append("Referrer-Policy", new StringValues("strict-origin-when-cross-origin"));
        context.Response.Headers.Append("X-XSS-Protection", new StringValues("1; mode=block"));
        context.Response.Headers.Append("Cross-Origin-Opener-Policy", new StringValues("same-origin"));
        return next(context);
    }
}
