namespace DiscussionForum.Shared.DTO.Users;

public class UserInfo
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public Role Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
}
