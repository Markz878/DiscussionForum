namespace DiscussionForum.Shared.DTO.Users;

public class UserInfo
{
    public static UserInfo Anonymous { get; } = new() { Claims = Enumerable.Empty<ClaimValue>() };
    public bool IsAuthenticated { get; set; }
    public required IEnumerable<ClaimValue> Claims { get; init; }
}
