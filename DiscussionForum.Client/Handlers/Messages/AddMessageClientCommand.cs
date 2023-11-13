namespace DiscussionForum.Client.Handlers.Messages;

internal sealed record AddMessageClientCommand : IRequest<AddMessageResponse>
{
    public required long TopicId { get; init; }
    public required string Message { get; init; }
    public AttachedFileInfo[]? AttachedFiles { get; init; }
}

internal class AddMessageClientCommandHandler : IRequestHandler<AddMessageClientCommand, AddMessageResponse>
{
    private const string path = "api/messages";
    private readonly IHttpClientFactory httpClientFactory;

    public AddMessageClientCommandHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<AddMessageResponse> Handle(AddMessageClientCommand message, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
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
        HttpResponseMessage response = await httpClient.PostAsync(path, formData, cancellationToken);
        AddMessageResponse? result = await response.Content.ReadFromJsonAsync<AddMessageResponse>(cancellationToken: cancellationToken);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }
}
