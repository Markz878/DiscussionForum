namespace DiscussionForum.Shared.DTO.Topics;

public sealed record ListLatestTopicsRequest
{
    public int PageNumber { get; init; }
    public string? SearchText { get; init; }
    public int TopicsCount { get; init; } = 10;
}