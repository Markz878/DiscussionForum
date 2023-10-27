namespace DiscussionForum.Shared.DTO.Topics;

public sealed record TopicResult
{
    public long Id { get; init; }
    public required string Title { get; init; }
    public int MessageCount { get; init; }
    public required string UserName { get; init; }
    public required DateTimeOffset Created { get; init; }
    public DateTimeOffset LastMessageTimeStamp { get; init; }
}
