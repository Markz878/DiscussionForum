namespace DiscussionForum.Server.Endpoints;

public static class MessageLikesEndpointsMapper
{
    public static void MapMessageLikesEndpoints(this RouteGroupBuilder builder)
    {
        RouteGroupBuilder accountGroup = builder.MapGroup("messagelikes")
            .WithTags("Message likes");

        accountGroup.MapPost("{messageid:long}", AddMessageLike);
        accountGroup.MapDelete("{messageid:long}", DeleteMessageLike);
    }

    public static async Task<NoContent> AddMessageLike(long messageId, IMessagesService messagesService, IMessageLikesService messageLikesService, ClaimsPrincipal claimsPrincipal, IHubContext<TopicHub> hub)
    {
        await messageLikesService.AddMessageLike(messageId);
        await NotifyLikesCountUpdate(messageId, messagesService, messageLikesService, claimsPrincipal, hub, true);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteMessageLike(long messageId, IMessagesService messagesService, IMessageLikesService messageLikesService, ClaimsPrincipal claimsPrincipal, IHubContext<TopicHub> hub)
    {
        await messageLikesService.DeleteMessageLike(messageId);
        await NotifyLikesCountUpdate(messageId, messagesService, messageLikesService, claimsPrincipal, hub, false);
        return TypedResults.NoContent();
    }

    private static async Task NotifyLikesCountUpdate(long messageId, IMessagesService messagesService, IMessageLikesService messageLikesService, ClaimsPrincipal claimsPrincipal, IHubContext<TopicHub> hub, bool likeAdded)
    {
        (int messageLikesCount, long topicId) = await TaskHelpers.RunParallel(
            messageLikesService.GetMessageLikesCount(messageId),
            messagesService.GetMessageTopicId(messageId));
        if (topicId > 0)
        {
            await hub.Clients.Group(topicId.ToString()).SendAsync(nameof(ITopicHubNotifications.MessageLikesChanged), messageId, messageLikesCount, likeAdded, claimsPrincipal.GetUserId());
        }
    }
}

