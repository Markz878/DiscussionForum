namespace DiscussionForum.Shared.DTO.Topics;
public class EditTopicTitleRequest
{
    public required long TopicId { get; init; }
    public required string NewTitle { get; init; }
}
