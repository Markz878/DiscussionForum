using DiscussionForum.Shared.DTO.Users;
using DiscussionForum.Shared.Interfaces;

namespace DiscussionForum.Core.Services;

internal sealed class MessageLikesService(AppDbContext db, IUserInfoService userInfoService) : IMessageLikesService
{
    public async Task AddMessageLike(long messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            UserInfo userInfo = userInfoService.GetCurrentUserInfo() ?? throw new ForbiddenException();
            MessageLike messageLike = new()
            {
                MessageId = messageId,
                UserId = userInfo.Id,
            };
            db.MessageLikes.Add(messageLike);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException)
        {
            throw new ConflictException("User has already liked the message");
        }
        catch (ReferenceConstraintException)
        {
            throw NotFoundException.SetMessageFromType<Message>();
        }
    }

    public async Task DeleteMessageLike(long messageId, CancellationToken cancellationToken = default)
    {
        UserInfo userInfo = userInfoService.GetCurrentUserInfo() ?? throw new ForbiddenException();
        int rows = await db.MessageLikes
            .Where(x => x.UserId == userInfo.Id && x.MessageId == messageId)
            .ExecuteDeleteAsync(cancellationToken);
        if (rows == 0)
        {
            throw NotFoundException.SetMessageFromType<MessageLike>();
        }
    }

    public async Task<int> GetMessageLikesCount(long messageId, CancellationToken cancellationToken = default)
    {
        int count = await db.MessageLikes
            .Where(x => x.MessageId == messageId)
            .CountAsync(cancellationToken);
        return count;
    }
}