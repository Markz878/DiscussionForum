using DiscussionForum.Shared.DTO.Users;

namespace DiscussionForum.Core.HelperMethods;

internal sealed class CommonExtensions
{
    internal static bool IsUserAdminOrOwner(UserInfo? userInfo, Guid entityUserGuid)
    {
        return userInfo is not null && (userInfo.Role == Role.Admin || entityUserGuid == userInfo.Id);
    }
}
