namespace DiscussionForum.Shared.Models.Topics;

public sealed record EditTopicTitle : IRequest
{
    public required long TopicId { get; init; }
    public required string NewTitle { get; init; }
    public Guid UserId { get; set; }
    public Role UserRole { get; set; }
}

public sealed class EditTopicTitleValidator : AbstractValidator<EditTopicTitle>
{
    public EditTopicTitleValidator()
    {
        RuleFor(x => x.NewTitle).NotEmpty().MinimumLength(ValidationConstants.TopicTitleMinLength).MaximumLength(ValidationConstants.TopicTitleMaxLength);
    }
}