using System.ComponentModel.DataAnnotations;

namespace DiscussionForum.Shared.DTO.Messages;

public sealed record EditMessageRequest
{
    public required long MessageId { get; init; }
    [MaxLength(ValidationConstants.MessageContentMaxLength)]
    public required string Message { get; set; }
}
