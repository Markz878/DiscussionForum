using DiscussionForum.Shared.DTO.Topics;

namespace DiscussionForum.Shared.Interfaces;

public interface ITopicsService
{
    Task<AddTopicResult> AddTopic(string title, string firstMessage, AttachedFileInfo[]? attachedFiles = null, CancellationToken cancellationToken = default);
    Task DeleteTopic(long topicId, CancellationToken cancellationToken = default);
    Task EditTopicTitle(long topicId, string newTitle, CancellationToken cancellationToken = default);
    Task<GetTopicByIdResult?> GetTopicById(long topicId, CancellationToken cancellationToken = default);
    Task<ListLatestTopicsResult> ListLatestTopics(int page, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
}