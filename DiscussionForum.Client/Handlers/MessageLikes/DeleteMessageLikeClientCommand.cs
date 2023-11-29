namespace DiscussionForum.Client.Handlers.MessageLikes;

internal class DeleteMessageLikeClientCommand : IRequest
{
    public long MessageId { get; set; }
}

internal class DeleteMessageLikeClientHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<DeleteMessageLikeClientCommand>
{
    public async Task Handle(DeleteMessageLikeClientCommand message, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await httpClientFactory.CreateClient("Client")
            .DeleteAsync("api/messagelikes/" + message.MessageId, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
