using DiscussionForum.Shared.Models.Messages;

namespace DiscussionForum.Shared.Models.Topics;

public sealed record AddTopic : IRequest<AddTopicResult>
{
    public required string Title { get; init; }
    public required string FirstMessage { get; init; }
    public required Guid UserId { get; init; }
    public AddAttachedFile[]? AttachedFiles { get; init; }
}

public sealed class AddTopicValidator : AbstractValidator<AddTopic>
{
    public AddTopicValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MinimumLength(ValidationConstants.TopicTitleMinLength).MaximumLength(ValidationConstants.TopicTitleMaxLength);
        RuleFor(x => x.FirstMessage).NotEmpty().MaximumLength(ValidationConstants.MessageContentMaxLength);
        RuleFor(x => x.AttachedFiles).Must(x => x is null or { Length: <= ValidationConstants.MessageMaxFiles }).WithMessage("Topic message can contain maximum of 4 attached files.");
    }
}

public sealed record AddTopicResult
{
    public required long Id { get; init; }
}

