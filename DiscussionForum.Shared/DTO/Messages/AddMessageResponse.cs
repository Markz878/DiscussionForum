namespace DiscussionForum.Shared.DTO.Messages;

public sealed record AddMessageResponse
{
    public required long Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public AttachedFileResponse[]? AttachedFiles { get; init; }
}

