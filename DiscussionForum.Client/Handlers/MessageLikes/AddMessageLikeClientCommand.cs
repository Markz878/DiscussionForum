namespace DiscussionForum.Client.Handlers.MessageLikes;

internal class AddMessageLikeClientCommand : IRequest
{
    public long MessageId { get; set; }
}

internal class AddMessageLikeClientHandler : IRequestHandler<AddMessageLikeClientCommand>
{
    private const string _path = "api/messagelikes/";
    private readonly IHttpClientFactory httpClientFactory;

    public AddMessageLikeClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task Handle(AddMessageLikeClientCommand message, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(_path + message.MessageId, message, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
