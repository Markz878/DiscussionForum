namespace DiscussionForum.Shared.Models.Topics;

public sealed record ListLatestTopics : IRequest<ListLatestTopicsResult>
{
    public int PageNumber { get; init; }
    public string? SearchText { get; init; }
    public int TopicsCount { get; init; } = 10;
}

public sealed record ListLatestTopicsResult
{
    public required List<TopicResult> Topics { get; set; }
    public int PageCount { get; set; }
}

public sealed record TopicResult
{
    public long Id { get; init; }
    public required string Title { get; init; }
    public int MessageCount { get; init; }
    public required string UserName { get; init; }
    public required DateTimeOffset Created { get; init; }
    public DateTimeOffset LastMessageTimeStamp { get; init; }
}
