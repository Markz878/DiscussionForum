using DiscussionForum.Core.Features.Messages;
using DiscussionForum.Shared.Models.MessageLikes;

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
        await mediator.Send(new AddMessageLike() { MessageId = messageId, UserId = claimsPrincipal.GetUserId() });
        await NotifyLikesCountUpdate(messageId, mediator, claimsPrincipal, hub, true);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteMessageLike(long messageId, IMediator mediator, ClaimsPrincipal claimsPrincipal, IHubContext<TopicHub> hub)
    {
        await mediator.Send(new DeleteMessageLike() { MessageId = messageId, UserId = claimsPrincipal.GetUserId() });
        await NotifyLikesCountUpdate(messageId, mediator, claimsPrincipal, hub, false);
        return TypedResults.NoContent();
    }

    private static async Task NotifyLikesCountUpdate(long messageId, IMediator mediator, ClaimsPrincipal claimsPrincipal, IHubContext<TopicHub> hub, bool likeAdded)
    {
        (GetMessageLikesCountResult messageLikesCount, GetMessageTopicIdResult? topicId) = await TaskHelpers.RunParallel(
            mediator.Send(new GetMessageLikesCount() { MessageId = messageId }),
            mediator.Send(new GetMessageTopicId() { MessageId = messageId }));
        if (topicId is not null)
        {
            await hub.Clients.Group(topicId.TopicId.ToString()).SendAsync(nameof(ITopicHubNotifications.MessageLikesChanged), messageId, messageLikesCount.Count, likeAdded, claimsPrincipal.GetUserId());
        }
    }
}

