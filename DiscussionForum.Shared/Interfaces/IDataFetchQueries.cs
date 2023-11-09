using DiscussionForum.Shared.DTO.Topics;

namespace DiscussionForum.Shared.Interfaces;

public interface IDataFetchQueries
{
    Task<GetTopicByIdResult?> GetTopicById(long id, Guid? userId, CancellationToken cancellationToken = default);
    Task<ListLatestTopicsResult> ListLatestTopics(ListLatestTopicsRequest request, CancellationToken cancellationToken = default);
}
