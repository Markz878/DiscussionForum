using DiscussionForum.Shared.DTO.Users;

namespace DiscussionForum.Shared.Interfaces;

public interface IUsersService
{
    Task<UserInfo?> GetUserInfo(Guid userId, CancellationToken cancellationToken = default);
    Task UpsertUser(Guid id, string email, string userName, CancellationToken cancellationToken = default);
}