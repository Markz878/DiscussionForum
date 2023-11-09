namespace DiscussionForum.Shared.DTO.Topics;

public sealed record GetTopicByIdResult
{
    public required long Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required string UserName { get; init; }
    public required string Title { get; set; }
    public required List<TopicMessage> Messages { get; init; }
}
