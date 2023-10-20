namespace DiscussionForum.Core.Features.Common;
internal sealed class CommonExtensions
{
    internal static bool IsUserAdminOrOwner(Role userRole, Guid entityUserGuid, Guid actualUserGuid)
    {
        return userRole == Role.Admin || entityUserGuid == actualUserGuid;
    }
}
