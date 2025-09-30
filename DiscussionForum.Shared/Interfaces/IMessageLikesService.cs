namespace DiscussionForum.Shared.Interfaces;

public interface IMessageLikesService
{
    Task AddMessageLike(long messageId, CancellationToken cancellationToken = default);
    Task DeleteMessageLike(long messageId, CancellationToken cancellationToken = default);
    Task<int> GetMessageLikesCount(long messageId, CancellationToken cancellationToken = default);
}