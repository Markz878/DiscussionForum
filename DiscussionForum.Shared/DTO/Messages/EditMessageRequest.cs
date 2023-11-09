namespace DiscussionForum.Shared.DTO.Messages;

public sealed record EditMessageRequest
{
    public required long MessageId { get; init; }
    public required string Message { get; set; }
}

public sealed class EditMessageValidator : AbstractValidator<EditMessageRequest>
{
    public EditMessageValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MinimumLength(1).MaximumLength(ValidationConstants.MessageContentMaxLength);
    }
}

public sealed record EditMessageResult
{
    public required DateTimeOffset EditedAt { get; init; }
}