namespace DiscussionForum.Client.Handlers.MessageLikes;

internal class AddMessageLikeClientCommand : IRequest
{
    public long MessageId { get; set; }
}

internal class AddMessageLikeClientHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<AddMessageLikeClientCommand>
{
    public async Task Handle(AddMessageLikeClientCommand message, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await httpClientFactory.CreateClient("Client")
            .PostAsJsonAsync("api/messagelikes/" + message.MessageId, message, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
