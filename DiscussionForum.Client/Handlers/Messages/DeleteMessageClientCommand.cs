namespace DiscussionForum.Client.Handlers.Messages;

internal class DeleteMessageClientCommand : IRequest
{
    public long MessageId { get; set; }
}

internal class DeleteMessageClientRequestHandler : IRequestHandler<DeleteMessageClientCommand>
{
    private const string path = "api/messages/";
    private readonly IHttpClientFactory httpClientFactory;

    public DeleteMessageClientRequestHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task Handle(DeleteMessageClientCommand message, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        HttpResponseMessage response = await httpClient.DeleteAsync(path + message.MessageId, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
