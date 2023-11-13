namespace DiscussionForum.Client.Handlers.Topics;

internal sealed record EditTopicTitleClientCommand : IRequest
{
    public required long TopicId { get; init; }
    public required string NewTitle { get; init; }
}

internal class EditTopicTitleClientCommandHandler : IRequestHandler<EditTopicTitleClientCommand>
{
    private const string path = "api/topics";
    private readonly IHttpClientFactory httpClientFactory;

    public EditTopicTitleClientCommandHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task Handle(EditTopicTitleClientCommand request, CancellationToken cancellationToken)
    {
        await httpClientFactory.CreateClient("Client").PatchAsJsonAsync(path, request, cancellationToken);
    }
}
