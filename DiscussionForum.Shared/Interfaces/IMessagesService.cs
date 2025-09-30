namespace DiscussionForum.Shared.Interfaces;

public interface IMessagesService
{
    Task<AddMessageResponse> AddMessage(long topicId, string message, AttachedFileInfo[]? attachedFiles = null, CancellationToken cancellationToken = default);
    Task DeleteMessage(long messageId, CancellationToken cancellationToken = default);
    Task<EditMessageResult> EditMessage(long messageId, string content, CancellationToken cancellationToken = default);
    Task<string?> GetFileNameById(Guid id, CancellationToken cancellationToken = default);
    Task<long> GetMessageTopicId(long messageId, CancellationToken cancellationToken = default);
}