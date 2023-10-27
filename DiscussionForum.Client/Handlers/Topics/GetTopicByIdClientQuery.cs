namespace DiscussionForum.Client.Handlers.Topics;

internal sealed record GetTopicByIdClientQuery : IRequest<GetTopicByIdResult?>
{
    public required long Id { get; init; }
}

internal sealed class GetTopicByIdClientQueryHandler : IRequestHandler<GetTopicByIdClientQuery, GetTopicByIdResult?>
{
    private readonly IHttpClientFactory httpClientFactory;
    public GetTopicByIdClientQueryHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<GetTopicByIdResult?> Handle(GetTopicByIdClientQuery request, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        GetTopicByIdResult? result = await httpClient.GetFromJsonAsync<GetTopicByIdResult>($"api/topics/{request.Id}", cancellationToken);
        return result;
    }
}
