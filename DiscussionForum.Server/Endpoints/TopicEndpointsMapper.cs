using DiscussionForum.Shared.DTO.Topics;

namespace DiscussionForum.Server.Endpoints;

public static class TopicEndpointsMapper
{
    public static void MapTopicEndpoints(this RouteGroupBuilder routeBuilder)
    {
        RouteGroupBuilder topicGroup = routeBuilder.MapGroup("topics")
            .WithTags("Topics");

        topicGroup.MapGet("{topicId:long}", GetTopic);
        topicGroup.MapDelete("{topicId:long}", DeleteTopic);
        topicGroup.MapPatch("", EditTopicTitle);
    }

    public static async Task<Results<Ok<GetTopicByIdResult>, NotFound>> GetTopic(long topicId, ClaimsPrincipal claimsPrincipal, ITopicsService topicsService, IHubContext<TopicHub> hub, CancellationToken cancellationToken)
    {
        GetTopicByIdResult? topic = await topicsService.GetTopicById(topicId, cancellationToken);
        return topic is not null ? TypedResults.Ok(topic) : TypedResults.NotFound();
    }

    public static async Task<NoContent> DeleteTopic(long topicId, ClaimsPrincipal claimsPrincipal, ITopicsService topicsService, IHubContext<TopicHub> hub, CancellationToken cancellationToken)
    {
        await topicsService.DeleteTopic(topicId, cancellationToken);
        await hub.Clients.Group(topicId.ToString()).SendAsync(nameof(ITopicHubNotifications.TopicDeleted), cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> EditTopicTitle(EditTopicTitleRequest request, ClaimsPrincipal claimsPrincipal, ITopicsService topicsService, IHubContext<TopicHub> hub, CancellationToken cancellationToken)
    {
        await topicsService.EditTopicTitle(request.TopicId, request.NewTitle, cancellationToken);
        await hub.Clients.Group(request.TopicId.ToString()).SendAsync(nameof(ITopicHubNotifications.TopicTitleEdited), request.NewTitle, cancellationToken);
        return TypedResults.NoContent();
    }
}