using DiscussionForum.Shared.DTO;

namespace DiscussionForum.Client.Handlers.Messages;

internal sealed record AddMessageClientCommand : IRequest<AddMessageResponse>
{
    public required long TopicId { get; init; }
    public required string Message { get; init; }
    public AttachedFileInfo[]? AttachedFiles { get; init; }
}

internal class AddMessageClientCommandHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<AddMessageClientCommand, AddMessageResponse>
{
    public async Task<AddMessageResponse> Handle(AddMessageClientCommand message, CancellationToken cancellationToken)
    {
        MultipartFormDataContent formData = new()
        {
            { new StringContent(message.TopicId.ToString()), "topicid" },
            { new StringContent(message.Message), "message" }
        };
        if (message.AttachedFiles is not null)
        {
            foreach (AttachedFileInfo item in message.AttachedFiles)
            {
                formData.Add(new StreamContent(item.FileStream), item.Name, item.Name);
            }
        }
        HttpResponseMessage response = await httpClientFactory.CreateClient("Client")
            .PostAsync("api/messages", formData, cancellationToken);
        AddMessageResponse? result = await response.Content.ReadFromJsonAsync(JsonContext.Default.AddMessageResponse, cancellationToken);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }
}
