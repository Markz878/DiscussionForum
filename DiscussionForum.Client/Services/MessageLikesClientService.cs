namespace DiscussionForum.Client.Services;

public class MessageLikesClientService(IHttpClientFactory httpClientFactory) : IMessageLikesService
{
    public async Task AddMessageLike(long messageId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClientFactory.CreateClient("Client")
            .PostAsync("api/messagelikes/" + messageId, null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteMessageLike(long messageId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClientFactory.CreateClient("Client")
            .DeleteAsync("api/messagelikes/" + messageId, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public Task<int> GetMessageLikesCount(long messageId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
