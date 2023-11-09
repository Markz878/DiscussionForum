namespace DiscussionForum.Server.Hubs;

public sealed class TopicHub : Hub<ITopicHubNotifications>, ITopicHubClientActions
{
    public async Task JoinTopic(long topicId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, topicId.ToString());
    }

    public async Task LeaveTopic(long topicId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, topicId.ToString());
    }
}
