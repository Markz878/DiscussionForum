using DiscussionForum.Core.Features.Topics;
using DiscussionForum.Shared.DTO.Topics;
using DiscussionForum.Shared.Interfaces;

namespace DiscussionForum.Core.Services;

internal class DataFetchServices(IMediator mediator) : IDataFetchQueries
{
    public async Task<GetTopicByIdResult?> GetTopicById(long id, Guid? userId, CancellationToken cancellationToken = default)
    {
        GetTopicByIdResult? result = await mediator.Send(new GetTopicByIdQuery() { TopicId = id, UserId = userId }, cancellationToken);
        return result;
    }

    public async Task<ListLatestTopicsResult> ListLatestTopics(ListLatestTopicsRequest request, CancellationToken cancellationToken = default)
    {
        ListLatestTopicsResult result = await mediator.Send(new ListLatestTopicsQuery() { PageNumber = request.PageNumber, SearchText = request.SearchText, TopicsCount = request.TopicsCount }, cancellationToken);
        return result;
    }
}
