namespace DiscussionForum.Shared.Models.Messages;

public sealed record EditMessage : IRequest<EditMessageResult>
{
    public required long MessageId { get; init; }
    public Guid UserId { get; set; }
    public Role UserRole { get; set; }
    public string? Message { get; set; }
}

public sealed class EditMessageValidator : AbstractValidator<EditMessage>
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