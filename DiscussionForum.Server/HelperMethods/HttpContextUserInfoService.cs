using DiscussionForum.Shared.DTO.Users;

namespace DiscussionForum.Server.HelperMethods;

public sealed class HttpContextUserInfoService(IHttpContextAccessor httpContextAccessor) : IUserInfoService
{
    public UserInfo? GetCurrentUserInfo()
    {
        UserInfo? userInfo = httpContextAccessor.HttpContext?.User.GetUserInfo();
        return userInfo;
    }
}
