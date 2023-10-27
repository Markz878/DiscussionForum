using DiscussionForum.Server.Filters;
using DiscussionForum.Server.Installers;

namespace DiscussionForum.Server.Endpoints;

public static class APIEndpointMapper
{
    public static void MapAPIEndpoints(this WebApplication app)
    {
        RouteGroupBuilder apiGroup = app.MapGroup("api")
            .RequireAuthorization()
            .AddEndpointFilter<ExceptionFilter>()
            .AddFluentValidation()
            .RequireRateLimiting(RateLimitInstaller.PolicyName)
            .CacheOutput()
            ;

        apiGroup.MapMessageLikesEndpoints();
        apiGroup.MapTopicEndpoints();
        apiGroup.MapMessageEndpoints();
        apiGroup.MapFileRetrievalEndpoint();

        apiGroup.MapGet("headers", (HttpRequest request) =>
        {
            string headers = string.Join("                ", request.Headers.Select(x => x.Key + ": " + x.Value));
            return TypedResults.Ok(headers);
        }).AllowAnonymous();
        apiGroup.MapGet("claims", (ClaimsPrincipal user) =>
        {
            string claims = string.Join("                ", user.Claims.Select(x => x.Type + ": " + x.Value));
            return TypedResults.Ok(claims);
        });
    }
}
