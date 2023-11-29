namespace DiscussionForum.Shared.DTO.Messages;

public sealed record EditMessageResult
{
    public required DateTimeOffset EditedAt { get; init; }
}