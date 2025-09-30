namespace DiscussionForum.Shared.DTO.Messages;

public sealed record EditMessageResult
{
    public required DateTimeOffset EditedAt { get; init; }
    public required long TopicId { get; init; }
}