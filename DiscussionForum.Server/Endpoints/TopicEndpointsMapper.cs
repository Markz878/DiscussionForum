using DiscussionForum.Core.Features.Topics;
using DiscussionForum.Shared.DTO.Topics;

namespace DiscussionForum.Server.Endpoints;

public static class TopicEndpointsMapper
{
    public static void MapTopicEndpoints(this RouteGroupBuilder routeBuilder)
    {
        RouteGroupBuilder topicGroup = routeBuilder.MapGroup("topics")
            .WithTags("Topics");

        topicGroup.MapDelete("{topicId:long}", DeleteTopic);
        topicGroup.MapPatch("", EditTopicTitle);
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