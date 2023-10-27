using DiscussionForum.Shared.DTO.Topics;

namespace DiscussionForum.Shared.Interfaces;

public interface ITopicHubNotifications
{
    Task TopicTitleEdited(string newTitle);
    Task TopicDeleted();
    Task MessageAdded(TopicMessage message);
    Task MessageEdited(long id, string Content, DateTimeOffset editedAt);
    Task MessageDeleted(long id);
    Task MessageLikesChanged(long id, int likesCount, bool likeAdded, Guid userId);
}
