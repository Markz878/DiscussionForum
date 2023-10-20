namespace DiscussionForum.Shared.Models.MessageLikes;

public sealed record DeleteMessageLike : IRequest
{
    public required Guid UserId { get; init; }
    public required long MessageId { get; init; }
}