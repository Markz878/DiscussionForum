namespace DiscussionForum.Client.Handlers.Topics;

public sealed class AddTopicClientHandler : IRequestHandler<AddTopic, AddTopicResult>
{
    private const string path = "api/topics";
    private readonly IHttpClientFactory httpClientFactory;

    public AddTopicClientHandler(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<AddTopicResult> Handle(AddTopic request, CancellationToken cancellationToken)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Client");
        MultipartFormDataContent formData = new()
        {
            { new StringContent(request.Title), "title" },
            { new StringContent(request.FirstMessage), "firstMessage" }
        };
        if (request.AttachedFiles is not null)
        {
            foreach (AddAttachedFile item in request.AttachedFiles)
            {
                formData.Add(new StreamContent(item.FileStream), item.Name, item.Name);
            }
        }
        HttpResponseMessage response = await httpClient.PostAsync(path, formData, cancellationToken);
        AddTopicResult? result = await response.Content.ReadFromJsonAsync<AddTopicResult>(cancellationToken: cancellationToken);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }
}
