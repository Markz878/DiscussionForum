namespace DiscussionForum.Shared.Models.Topics;

public sealed record DeleteTopic : IRequest
{
    public required long TopicId { get; init; }
    public Guid UserId { get; set; }
    public Role UserRole { get; set; }
}