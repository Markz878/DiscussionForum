namespace DiscussionForum.Client.Handlers.Topics;

internal sealed record ListLatestTopicsClientQuery : IRequest<ListLatestTopicsResult>
{
    public int PageNumber { get; init; }
    public string? SearchText { get; init; }
    public int TopicsCount { get; init; } = 10;
}

internal class ListLatestTopicsClientQueryHandler : IRequestHandler<ListLatestTopicsClientQuery, ListLatestTopicsResult>
{
    private readonly IHttpClientFactory httpClientFactory;
    public ListLatestTopicsClientQueryHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<ListLatestTopicsResult> Handle(ListLatestTopicsClientQuery request, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        ListLatestTopicsResult result = await httpClient.GetFromJsonAsync<ListLatestTopicsResult>($"api/topics/latest/{request.PageNumber}?search={request.SearchText}", cancellationToken)
            ?? new ListLatestTopicsResult() { Topics = [] };
        return result;
    }
}
