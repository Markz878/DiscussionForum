namespace DiscussionForum.Shared.Models.Users;
public sealed record GetUserInfo : IRequest<GetUserInfoResult?>
{
    public required Guid Id { get; init; }
}
public sealed record GetUserInfoResult(Guid Id, string UserName, string Email, DateTimeOffset JoinedAt, Role Role);
