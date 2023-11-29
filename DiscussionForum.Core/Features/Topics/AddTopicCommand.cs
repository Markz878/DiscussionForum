using DiscussionForum.Shared.DTO.Messages;

namespace DiscussionForum.Core.Features.Topics;

public sealed record AddTopicCommand : IRequest<AddTopicResult>
{
    public required string Title { get; init; }
    public required string FirstMessage { get; init; }
    public required Guid UserId { get; init; }
    public AttachedFileInfo[]? AttachedFiles { get; init; }
}

public sealed class AddTopicCommandValidator : AbstractValidator<AddTopicCommand>
{
    public AddTopicCommandValidator()
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


internal sealed class AddTopicCommandHandler(AppDbContext db, IFileService fileService) : IRequestHandler<AddTopicCommand, AddTopicResult>
{
    public async Task<AddTopicResult> Handle(AddTopicCommand request, CancellationToken cancellationToken = default)
    {
        DateTimeOffset timeStamp = DateTimeOffset.UtcNow;
        Topic newTopic = new()
        {
            Title = request.Title,
            CreatedAt = timeStamp,
            UserId = request.UserId,
            LastMessageTimeStamp = timeStamp,
            Messages =
            [
                new()
                {
                    Content = request.FirstMessage,
                    CreatedAt = timeStamp,
                    UserId = request.UserId,
                    AttachedFiles = request.AttachedFiles?.Select(x => new MessageAttachedFile()
                    {
                        Name = x.Name
                    }).ToList() ?? [],
                }
            ]
        };
        db.Topics.Add(newTopic);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            if (request.AttachedFiles?.Length > 0)
            {
                foreach (AttachedFileInfo file in request.AttachedFiles)
                {
                    Guid id = newTopic.Messages.First().AttachedFiles.First(x => x.Name == file.Name).Id;
                    string? url = await fileService.Upload(file.FileStream, id + file.Name, cancellationToken);
                }
            }
            return new AddTopicResult { Id = newTopic.Id };
        }
        catch (ReferenceConstraintException)
        {
            throw NotFoundException.SetMessageFromType<User>();
        }
    }
}
