namespace DiscussionForum.Client.Handlers.Messages;

public class DeleteMessageClientHandler : IRequestHandler<DeleteMessage>
{
    private const string path = "api/messages/";
    private readonly IHttpClientFactory httpClientFactory;

    public DeleteMessageClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task Handle(DeleteMessage message, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        HttpResponseMessage response = await httpClient.DeleteAsync(path + message.MessageId, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
