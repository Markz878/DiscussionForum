
using DiscussionForum.Shared.DTO;
using System.Net.Http;

namespace DiscussionForum.Client.Services;

public sealed class MessagesClientService(IHttpClientFactory httpClientFactory) : IMessagesService
{
    public async Task<AddMessageResponse> AddMessage(long topicId, string message, AttachedFileInfo[]? attachedFiles = null, CancellationToken cancellationToken = default)
    {
        MultipartFormDataContent formData = new()
        {
            { new StringContent(topicId.ToString()), "topicid" },
            { new StringContent(message), "message" }
        };
        if (attachedFiles is not null)
        {
            foreach (AttachedFileInfo item in attachedFiles)
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

    public async Task DeleteMessage(long messageId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClientFactory.CreateClient("Client")
            .DeleteAsync("api/messages/" + messageId, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<EditMessageResult> EditMessage(long messageId, string content, CancellationToken cancellationToken = default)
    {
        EditMessageRequest editMessageRequest = new()
        {
            MessageId = messageId,
            Message = content
        };
        HttpResponseMessage response = await httpClientFactory.CreateClient("Client")
            .PatchAsJsonAsync("api/messages", editMessageRequest, JsonContext.Default.EditMessageRequest, cancellationToken);
        EditMessageResult? result = await response.Content
            .ReadFromJsonAsync(JsonContext.Default.EditMessageResult, cancellationToken);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }

    public Task<string?> GetFileNameById(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<long> GetMessageTopicId(long messageId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
