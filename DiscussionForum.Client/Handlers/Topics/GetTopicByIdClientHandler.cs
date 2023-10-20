namespace DiscussionForum.Client.Handlers.Topics;

public sealed class GetTopicByIdClientHandler : IRequestHandler<GetTopicById, GetTopicByIdResult?>
{
    private readonly IHttpClientFactory httpClientFactory;
    public GetTopicByIdClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<GetTopicByIdResult?> Handle(GetTopicById request, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        GetTopicByIdResult? result = await httpClient.GetFromJsonAsync<GetTopicByIdResult>($"api/topics/{request.Id}", cancellationToken);
        return result;
    }
}
