namespace DiscussionForum.Client.Handlers.Users;

public class UpsertUserClientHandler : IRequestHandler<UpsertUser>
{
    private readonly IHttpClientFactory httpClientFactory;

    public UpsertUserClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task Handle(UpsertUser request, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/account/upsertuser", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
