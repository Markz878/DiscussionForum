using DiscussionForum.Shared.DTO.Users;

namespace DiscussionForum.Core.Features.Topics;

public sealed record EditTopicTitleCommand : IRequest
{
    public required long TopicId { get; init; }
    public required string NewTitle { get; init; }
    public Guid UserId { get; set; }
    public Role UserRole { get; set; }
}

public sealed class EditTopicTitleCommandHandlerValidator : AbstractValidator<EditTopicTitleCommand>
{
    public EditTopicTitleCommandHandlerValidator()
    {
        RuleFor(x => x.NewTitle)
            .NotEmpty()
            .MinimumLength(ValidationConstants.TopicTitleMinLength)
            .MaximumLength(ValidationConstants.TopicTitleMaxLength);
    }
}

internal sealed class EditTopicTitleCommandHandler(AppDbContext db) : IRequestHandler<EditTopicTitleCommand>
{
    public async Task Handle(EditTopicTitleCommand request, CancellationToken cancellationToken = default)
    {
        Guid topicUserId = await db.GetTopicUserId(request.TopicId, cancellationToken);

        if (topicUserId == Guid.Empty)
        {
            throw NotFoundException.SetMessageFromType<User>();
        }
        if (CommonExtensions.IsUserAdminOrOwner(request.UserRole, topicUserId, request.UserId) is false)
        {
            throw new ForbiddenException();
        }
        int rows = await db.Topics
            .Where(x => x.Id == request.TopicId)
            .ExecuteUpdateAsync(x => x.SetProperty(x => x.Title, request.NewTitle), cancellationToken);
        if (rows == 0)
        {
            throw NotFoundException.SetMessageFromType<Topic>();
        }
    }
}
