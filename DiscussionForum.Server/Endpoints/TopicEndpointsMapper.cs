using DiscussionForum.Core.Features.Topics;
using DiscussionForum.Shared.DTO.Topics;
using System.Reflection;

namespace DiscussionForum.Server.Endpoints;

public static class TopicEndpointsMapper
{
    public static void MapTopicEndpoints(this RouteGroupBuilder routeBuilder)
    {
        RouteGroupBuilder topicGroup = routeBuilder.MapGroup("topics");

        topicGroup.MapGet("latest/{page:int}", ListLatestTopics).AllowAnonymous();
        topicGroup.MapGet("{topicId:long}", GetTopicById).AllowAnonymous();
        topicGroup.MapDelete("{topicId:long}", DeleteTopic);
        topicGroup.MapPatch("", EditTopicTitle);
    }

    public static async Task<Ok<ListLatestTopicsResult>> ListLatestTopics(int page, string? search, ClaimsPrincipal claimsPrincipal, IDataFetchQueries dataFetch, CancellationToken cancellationToken)
    {
        ListLatestTopicsResult result = await dataFetch.ListLatestTopics(new ListLatestTopicsRequest() { PageNumber = page, TopicsCount = 10, SearchText = search }, cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<GetTopicByIdResult>, NotFound>> GetTopicById(long topicId, ClaimsPrincipal claimsPrincipal, IDataFetchQueries dataFetch, CancellationToken cancellationToken)
    {
        GetTopicByIdResult? result = await dataFetch.GetTopicById(topicId, claimsPrincipal.GetUserId(), cancellationToken);
        return result == null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<NoContent> DeleteTopic(long topicId, ClaimsPrincipal claimsPrincipal, IMediator mediator, IHubContext<TopicHub> hub, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteTopicCommand() { TopicId = topicId, UserId = claimsPrincipal.GetUserId(), UserRole = claimsPrincipal.GetUserRole() }, cancellationToken);
        await hub.Clients.Group(topicId.ToString()).SendAsync(nameof(ITopicHubNotifications.TopicDeleted), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> EditTopicTitle(EditTopicTitleRequest request, ClaimsPrincipal claimsPrincipal, IMediator mediator, IHubContext<TopicHub> hub, CancellationToken cancellationToken)
    {
        await mediator.Send(new EditTopicTitleCommand() { TopicId = request.TopicId, NewTitle = request.NewTitle, UserId = claimsPrincipal.GetUserId(), UserRole = claimsPrincipal.GetUserRole() }, cancellationToken);
        await hub.Clients.Group(request.TopicId.ToString()).SendAsync(nameof(ITopicHubNotifications.TopicTitleEdited), request.NewTitle, cancellationToken);
        return TypedResults.NoContent();
    }
}

public class AddTopicBinder
{
    public required string Title { get; init; }
    public required string FirstMessage { get; init; }
    public IFormFileCollection? AttachedFiles { get; set; }

    public static async ValueTask<AddTopicBinder> BindAsync(HttpContext httpContext, ParameterInfo _)
    {
        IFormCollection form = await httpContext.Request.ReadFormAsync();
        AddTopicBinder binder = new()
        {
            Title = form["title"][0] ?? "",
            FirstMessage = form["firstMessage"][0] ?? "",
            AttachedFiles = form.Files
        };
        return binder;
    }
}



