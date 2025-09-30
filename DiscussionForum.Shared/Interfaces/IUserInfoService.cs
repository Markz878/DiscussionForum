using DiscussionForum.Shared.DTO.Users;

namespace DiscussionForum.Shared.Interfaces;

public interface IUserInfoService
{
    UserInfo? GetCurrentUserInfo();
}
