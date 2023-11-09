namespace DiscussionForum.Shared.Interfaces;

public interface ITopicHubClientActions
{
    Task JoinTopic(long topicId);
    Task LeaveTopic(long topicId);
}