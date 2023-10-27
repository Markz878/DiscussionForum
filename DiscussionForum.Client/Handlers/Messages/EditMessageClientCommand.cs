namespace DiscussionForum.Client.Handlers.Messages;

internal class EditMessageClientCommand : IRequest<EditMessageResult>
{
    public long MessageId { get; set; }
    public string Message { get; set; } = string.Empty;
}

internal class EditMessageClientHandler : IRequestHandler<EditMessageClientCommand, EditMessageResult>
{
    private const string path = "api/messages";
    private readonly IHttpClientFactory httpClientFactory;

    public EditMessageClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<EditMessageResult> Handle(EditMessageClientCommand message, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        HttpResponseMessage response = await httpClient.PatchAsJsonAsync(path, message, cancellationToken);
        EditMessageResult? result = await response.Content.ReadFromJsonAsync<EditMessageResult>(cancellationToken: cancellationToken);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }
}
