namespace DiscussionForum.Client.Handlers.Topics;

public class DeleteTopicClientHandler : IRequestHandler<DeleteTopic>
{
    private const string path = "api/topics";
    private readonly IHttpClientFactory httpClientFactory;

    public DeleteTopicClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task Handle(DeleteTopic request, CancellationToken cancellationToken)
    {
        await httpClientFactory.CreateClient("Client").DeleteAsync($"{path}/{request.TopicId}", cancellationToken);
    }
}
