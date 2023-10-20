namespace DiscussionForum.Client.Handlers.Topics;

public class ListLatestTopicsClientHandler : IRequestHandler<ListLatestTopics, ListLatestTopicsResult>
{
    private readonly IHttpClientFactory httpClientFactory;
    public ListLatestTopicsClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<ListLatestTopicsResult> Handle(ListLatestTopics request, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        ListLatestTopicsResult result = await httpClient.GetFromJsonAsync<ListLatestTopicsResult>($"api/topics/latest/{request.PageNumber}?search={request.SearchText}", cancellationToken)
            ?? new ListLatestTopicsResult() { Topics = new List<TopicResult>() };
        return result;
    }
}
