namespace DiscussionForum.Shared.Models.MessageLikes;
public sealed record GetMessageLikesCount : IRequest<GetMessageLikesCountResult>
{
    public required long MessageId { get; init; }
}
public sealed record GetMessageLikesCountResult
{
    public required int Count { get; init; }
}
