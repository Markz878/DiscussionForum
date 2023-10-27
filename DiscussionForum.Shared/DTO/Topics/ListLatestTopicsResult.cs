namespace DiscussionForum.Shared.DTO.Topics;

public sealed record ListLatestTopicsResult
{
    public required List<TopicResult> Topics { get; set; }
    public int PageCount { get; set; }
}
