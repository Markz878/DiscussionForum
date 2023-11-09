namespace DiscussionForum.Client.Handlers.Topics;

internal sealed record DeleteTopicClientCommand : IRequest
{
    public required long TopicId { get; init; }
}

internal class DeleteTopicClientCommandHandler : IRequestHandler<DeleteTopicClientCommand>
{
    private const string path = "api/topics";
    private readonly IHttpClientFactory httpClientFactory;

    public DeleteTopicClientCommandHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task Handle(DeleteTopicClientCommand request, CancellationToken cancellationToken)
    {
        await httpClientFactory.CreateClient("Client").DeleteAsync($"{path}/{request.TopicId}", cancellationToken);
    }
}
