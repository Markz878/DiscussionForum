using DiscussionForum.Shared.DTO;

namespace DiscussionForum.Client.Handlers.Messages;

internal class EditMessageClientCommand : IRequest<EditMessageResult>
{
    public long MessageId { get; set; }
    public string Message { get; set; } = string.Empty;
}

internal class EditMessageClientHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<EditMessageClientCommand, EditMessageResult>
{
    public async Task<EditMessageResult> Handle(EditMessageClientCommand message, CancellationToken cancellationToken)
    {
        EditMessageRequest editMessageRequest = new()
        {
            MessageId = message.MessageId,
            Message = message.Message
        };
        HttpResponseMessage response = await httpClientFactory.CreateClient("Client")
            .PatchAsJsonAsync("api/messages", editMessageRequest, JsonContext.Default.EditMessageRequest, cancellationToken);
        EditMessageResult? result = await response.Content
            .ReadFromJsonAsync(JsonContext.Default.EditMessageResult, cancellationToken);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }
}
