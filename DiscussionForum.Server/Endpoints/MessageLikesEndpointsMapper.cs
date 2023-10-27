using DiscussionForum.Core.Features.MessageLikes;
using DiscussionForum.Core.Features.Messages;

namespace DiscussionForum.Server.Endpoints;

public static class MessageLikesEndpointsMapper
{
    public static void MapMessageLikesEndpoints(this RouteGroupBuilder builder)
    {
        RouteGroupBuilder accountGroup = builder.MapGroup("messagelikes");

        accountGroup.MapPost("{messageid:long}", AddMessageLike);
        accountGroup.MapDelete("{messageid:long}", DeleteMessageLike);
    }

    public static async Task<NoContent> AddMessageLike(long messageId, IMediator mediator, ClaimsPrincipal claimsPrincipal, IHubContext<TopicHub> hub)
    {
        await mediator.Send(new AddMessageLikeCommand() { MessageId = messageId, UserId = claimsPrincipal.GetUserId() });
        await NotifyLikesCountUpdate(messageId, mediator, claimsPrincipal, hub, true);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteMessageLike(long messageId, IMediator mediator, ClaimsPrincipal claimsPrincipal, IHubContext<TopicHub> hub)
    {
        await mediator.Send(new DeleteMessageLikeCommand() { MessageId = messageId, UserId = claimsPrincipal.GetUserId() });
        await NotifyLikesCountUpdate(messageId, mediator, claimsPrincipal, hub, false);
        return TypedResults.NoContent();
    }

    private static async Task NotifyLikesCountUpdate(long messageId, IMediator mediator, ClaimsPrincipal claimsPrincipal, IHubContext<TopicHub> hub, bool likeAdded)
    {
        (GetMessageLikesCountResult messageLikesCount, GetMessageTopicIdResult? topicId) = await TaskHelpers.RunParallel(
            mediator.Send(new GetMessageLikesCountQuery() { MessageId = messageId }),
            mediator.Send(new GetMessageTopicIdQuery() { MessageId = messageId }));
        if (topicId is not null)
        {
            await hub.Clients.Group(topicId.TopicId.ToString()).SendAsync(nameof(ITopicHubNotifications.MessageLikesChanged), messageId, messageLikesCount.Count, likeAdded, claimsPrincipal.GetUserId());
        }
    }
}

