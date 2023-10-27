namespace DiscussionForum.Shared.DTO.Messages;
public sealed record AttachedFileInfo
{
    public required string Name { get; init; }
    public required Stream FileStream { get; init; }
}
