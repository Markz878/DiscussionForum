namespace DiscussionForum.Client.Handlers.Messages;

public class AddMessageClientHandler : IRequestHandler<AddMessage, AddMessageResult>
{
    private const string path = "api/messages";
    private readonly IHttpClientFactory httpClientFactory;

    public AddMessageClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<AddMessageResult> Handle(AddMessage message, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        MultipartFormDataContent formData = new()
        {
            { new StringContent(message.TopicId.ToString()), "topicid" },
            { new StringContent(message.Message), "message" }
        };
        if (message.AttachedFiles is not null)
        {
            foreach (AddAttachedFile item in message.AttachedFiles)
            {
                formData.Add(new StreamContent(item.FileStream), item.Name, item.Name);
            }
        }
        HttpResponseMessage response = await httpClient.PostAsync(path, formData, cancellationToken);
        AddMessageResult? result = await response.Content.ReadFromJsonAsync<AddMessageResult>(cancellationToken: cancellationToken);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }
}
