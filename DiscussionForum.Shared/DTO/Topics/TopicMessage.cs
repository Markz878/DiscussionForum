namespace DiscussionForum.Shared.DTO.Topics;

public sealed record TopicMessage
{
    public required long Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? EditedAt { get; set; }
    public required string UserName { get; init; }
    public required string Content { get; set; }
    public int LikesCount { get; set; }
    public bool HasUserUpvoted { get; set; }
    public AttachedFileResponse[]? AttachedFiles { get; set; }
}

