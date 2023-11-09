namespace DiscussionForum.Client.Handlers.MessageLikes;

internal class DeleteMessageLikeClientCommand : IRequest
{
    public long MessageId { get; set; }
}

internal class DeleteMessageLikeClientHandler : IRequestHandler<DeleteMessageLikeClientCommand>
{
    private const string _path = "api/messagelikes/";
    private readonly IHttpClientFactory httpClientFactory;

    public DeleteMessageLikeClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task Handle(DeleteMessageLikeClientCommand message, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        HttpResponseMessage response = await httpClient.DeleteAsync(_path + message.MessageId, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
