using DiscussionForum.Shared.Models.Messages;

namespace DiscussionForum.Shared.Models.Topics;

public sealed record GetTopicById : IRequest<GetTopicByIdResult>
{
    public required long Id { get; init; }
    public Guid? UserId { get; init; }
}

public sealed record GetTopicByIdResult
{
    public required long Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required string UserName { get; init; }
    public required string Title { get; set; }
    public required List<TopicMessage> Messages { get; init; }
}

public sealed record TopicMessage
{
    public required long Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? EditedAt { get; set; }
    public required string UserName { get; init; }
    public required string Content { get; set; }
    public int LikesCount { get; set; }
    public bool HasUserUpvoted { get; set; }
    public AttachedFileInfo[]? AttachedFiles { get; set; }
}
