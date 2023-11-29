namespace DiscussionForum.Client.Handlers.Messages;

internal class DeleteMessageClientCommand : IRequest
{
    public long MessageId { get; set; }
}

internal class DeleteMessageClientRequestHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<DeleteMessageClientCommand>
{
    public async Task Handle(DeleteMessageClientCommand message, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await httpClientFactory.CreateClient("Client")
            .DeleteAsync("api/messages/" + message.MessageId, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
