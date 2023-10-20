using DiscussionForum.Shared.Models.MessageLikes;

namespace DiscussionForum.Client.Handlers.MessageLikes;

public class DeleteMessageLikeClientHandler : IRequestHandler<DeleteMessageLike>
{
    private const string _path = "api/messagelikes/";
    private readonly IHttpClientFactory httpClientFactory;

    public DeleteMessageLikeClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task Handle(DeleteMessageLike message, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        HttpResponseMessage response = await httpClient.DeleteAsync(_path + message.MessageId, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
