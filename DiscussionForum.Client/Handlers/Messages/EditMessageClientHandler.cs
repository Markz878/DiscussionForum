namespace DiscussionForum.Client.Handlers.Messages;

public class EditMessageClientHandler : IRequestHandler<EditMessage, EditMessageResult>
{
    private const string path = "api/messages";
    private readonly IHttpClientFactory httpClientFactory;

    public EditMessageClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<EditMessageResult> Handle(EditMessage message, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        HttpResponseMessage response = await httpClient.PatchAsJsonAsync(path, message, cancellationToken);
        EditMessageResult? result = await response.Content.ReadFromJsonAsync<EditMessageResult>(cancellationToken: cancellationToken);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }
}
