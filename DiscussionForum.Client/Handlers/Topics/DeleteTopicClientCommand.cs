namespace DiscussionForum.Client.Handlers.Topics;

internal sealed record DeleteTopicClientCommand : IRequest
{
    public required long TopicId { get; init; }
}

internal class DeleteTopicClientCommandHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<DeleteTopicClientCommand>
{
    public async Task Handle(DeleteTopicClientCommand request, CancellationToken cancellationToken)
    {
        await httpClientFactory.CreateClient("Client").DeleteAsync($"api/topics/{request.TopicId}", cancellationToken);
    }
}
