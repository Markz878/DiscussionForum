using System.ComponentModel.DataAnnotations;

namespace DiscussionForum.Shared.DTO.Topics;
public class EditTopicTitleRequest
{
    public required long TopicId { get; init; }
    [MinLength(ValidationConstants.TopicTitleMinLength)]
    [MaxLength(ValidationConstants.TopicTitleMaxLength)]
    public required string NewTitle { get; init; }
}
