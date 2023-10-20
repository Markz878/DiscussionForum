using DiscussionForum.Shared.Models.MessageLikes;

namespace DiscussionForum.Client.Handlers.MessageLikes;

public class AddMessageLikeClientHandler : IRequestHandler<AddMessageLike>
{
    private const string _path = "api/messagelikes/";
    private readonly IHttpClientFactory httpClientFactory;

    public AddMessageLikeClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task Handle(AddMessageLike message, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(_path + message.MessageId, message, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
