namespace DiscussionForum.Shared.DTO.Messages;

public sealed record AttachedFileResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}


