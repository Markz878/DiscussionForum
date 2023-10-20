namespace DiscussionForum.Shared.Models.Messages;

public sealed record DeleteMessage : IRequest
{
    public required long MessageId { get; init; }
    public Guid UserId { get; set; }
    public Role UserRole { get; set; }
}