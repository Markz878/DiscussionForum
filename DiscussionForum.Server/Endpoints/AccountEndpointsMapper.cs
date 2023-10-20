namespace DiscussionForum.Server.Endpoints;

public static class AccountEndpointsMapper
{
    public static void MapAccountEndpoints(this RouteGroupBuilder builder)
    {
        RouteGroupBuilder accountGroup = builder.MapGroup("account");

        accountGroup.MapGet("user", GetUserInfo).AllowAnonymous();
        accountGroup.MapPost("upsertuser", UpsertUser);
    }

    private static Ok<UserInfo> GetUserInfo(ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal.Identity?.IsAuthenticated == false)
        {
            return TypedResults.Ok(UserInfo.Anonymous);
        }
        UserInfo userInfo = new()
        {
            IsAuthenticated = true,
            Claims = claimsPrincipal.Claims.Select(x => new ClaimValue(x.Type, x.Value)).ToArray()
        };
        return TypedResults.Ok(userInfo);
    }

    private static async Task<NoContent> UpsertUser(ClaimsPrincipal claimsPrincipal, UpsertUser upsertUser, IMediator mediator)
    {
        upsertUser.UserId = claimsPrincipal.GetUserId();
        await mediator.Send(upsertUser);
        return TypedResults.NoContent();
    }
}
