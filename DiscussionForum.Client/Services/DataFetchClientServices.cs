namespace DiscussionForum.Client.Services;

internal class DataFetchClientServices : IDataFetchQueries
{
    private readonly IMediator mediator;

    public DataFetchClientServices(IMediator mediator)
    {
        this.mediator = mediator;
    }

    public async Task<GetTopicByIdResult?> GetTopicById(long id, Guid? userId, CancellationToken cancellationToken = default)
    {
        GetTopicByIdResult? result = await mediator.Send(new GetTopicByIdClientQuery() { Id = id }, cancellationToken);
        return result;
    }

    public async Task<ListLatestTopicsResult> ListLatestTopics(ListLatestTopicsRequest request, CancellationToken cancellationToken = default)
    {
        ListLatestTopicsResult? result = await mediator.Send(new ListLatestTopicsClientQuery() { PageNumber = request.PageNumber, SearchText = request.SearchText }, cancellationToken);
        return result;
    }
}
