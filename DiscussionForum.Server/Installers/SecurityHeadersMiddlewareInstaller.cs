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

public sealed class SecurityHeadersMiddleware(IWebHostEnvironment hostingEnvironment) : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Response.Headers.Append("X-Content-Type-Options", new StringValues("nosniff"));
        context.Response.Headers.Append("X-Frame-Options", new StringValues("DENY"));
        context.Response.Headers.Append("Referrer-Policy", new StringValues("strict-origin-when-cross-origin"));
        context.Response.Headers.Append("X-XSS-Protection", new StringValues("1; mode=block"));
        context.Response.Headers.Append("Cross-Origin-Opener-Policy", new StringValues("same-origin"));
        //context.Response.Headers.Append("Permissions-Policy", new StringValues(
        //    "accelerometer=(), " +
        //    "autoplay=(), " +
        //    "camera=(), " +
        //    "display-capture=(), " +
        //    "encrypted-media=(), " +
        //    "geolocation=(), " +
        //    "gyroscope=(), " +
        //    "magnetometer=(), " +
        //    "microphone=(), " +
        //    "midi=(), " +
        //    "payment=(), " +
        //    "picture-in-picture=(), " +
        //    "publickey-credentials-get=(), " +
        //    "sync-xhr=(), " +
        //    "usb=(), " +
        //    "xr-spatial-tracking=()"
        //    ));
        context.Response.Headers.Append("Content-Security-Policy-Report-Only", new StringValues(
            "default-src 'self'; " +
            "base-uri 'self'; " +
            "connect-src *; " +
            "object-src 'none'; " +
            "script-src 'self' 'wasm-unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self'; " +
            "upgrade-insecure-requests; "
            ));
        return next(context);
    }
}
