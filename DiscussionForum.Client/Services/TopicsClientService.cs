using DiscussionForum.Shared.DTO;

namespace DiscussionForum.Client.Services;

public sealed class TopicsClientService(IHttpClientFactory httpClientFactory) : ITopicsService
{
    public Task<AddTopicResult> AddTopic(string title, string firstMessage, AttachedFileInfo[]? attachedFiles = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteTopic(long topicId, CancellationToken cancellationToken = default)
    {
        await httpClientFactory.CreateClient("Client").DeleteAsync($"api/topics/{topicId}", cancellationToken);
    }

    public async Task EditTopicTitle(long topicId, string newTitle, CancellationToken cancellationToken = default)
    {
        EditTopicTitleRequest editTopicTitleRequest = new()
        {
            TopicId = topicId,
            NewTitle = newTitle
        };
        await httpClientFactory.CreateClient("Client")
            .PatchAsJsonAsync("api/topics", editTopicTitleRequest, JsonContext.Default.EditTopicTitleRequest, cancellationToken);
    }

    public async Task<GetTopicByIdResult?> GetTopicById(long topicId, CancellationToken cancellationToken = default)
    {
        return await httpClientFactory.CreateClient("Client")
            .GetFromJsonAsync("api/topics/" + topicId, JsonContext.Default.GetTopicByIdResult, cancellationToken);
    }

    public Task<ListLatestTopicsResult> ListLatestTopics(int page, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
