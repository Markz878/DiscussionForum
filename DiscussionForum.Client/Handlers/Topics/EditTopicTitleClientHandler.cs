namespace DiscussionForum.Client.Handlers.Topics;

public class EditTopicTitleClientHandler : IRequestHandler<EditTopicTitle>
{
    private const string path = "api/topics";
    private readonly IHttpClientFactory httpClientFactory;

    public EditTopicTitleClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task Handle(EditTopicTitle request, CancellationToken cancellationToken)
    {
        await httpClientFactory.CreateClient("Client").PatchAsJsonAsync(path, request, cancellationToken);
    }
}
