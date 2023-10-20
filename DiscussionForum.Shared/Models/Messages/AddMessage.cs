namespace DiscussionForum.Shared.Models.Messages;
public sealed record AddMessage : IRequest<AddMessageResult>
{
    public required long TopicId { get; init; }
    public required string Message { get; init; }
    public Guid UserId { get; set; }
    public AddAttachedFile[]? AttachedFiles { get; init; }
}

public sealed record AddAttachedFile
{
    public required string Name { get; init; }
    public required Stream FileStream { get; init; }
}

public sealed class AddMessageValidator : AbstractValidator<AddMessage>
{
    public AddMessageValidator()
    {
        RuleFor(x => x.Message).MinimumLength(1).MaximumLength(ValidationConstants.MessageContentMaxLength);
        RuleFor(x => x.AttachedFiles).Must(x => x is null or { Length: <= ValidationConstants.MessageMaxFiles }).WithMessage("Message can contain maximum of 4 attached files.");
    }
}

public sealed record AddMessageResult
{
    public required long Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public AttachedFileInfo[]? AttachedFiles { get; init; }
}

public sealed record AttachedFileInfo
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}
