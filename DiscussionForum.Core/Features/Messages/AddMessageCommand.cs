using DiscussionForum.Shared.DTO.Messages;

namespace DiscussionForum.Core.Features.Messages;

public sealed record AddMessageCommand : IRequest<AddMessageResponse>
{
    public required long TopicId { get; init; }
    public required string Message { get; init; }
    public Guid UserId { get; set; }
    public AttachedFileInfo[]? AttachedFiles { get; init; }
}

public sealed class AddMessageValidator : AbstractValidator<AddMessageCommand>
{
    public AddMessageValidator()
    {
        RuleFor(x => x.Message).MinimumLength(1).MaximumLength(ValidationConstants.MessageContentMaxLength);
        RuleFor(x => x.AttachedFiles).Must(x => x is null or { Length: <= ValidationConstants.MessageMaxFiles }).WithMessage("Message can contain maximum of 4 attached files.");
    }
}

internal sealed class AddMessageHandler : IRequestHandler<AddMessageCommand, AddMessageResponse>
{
    private readonly AppDbContext db;
    private readonly IFileService fileService;
    private readonly IValidator<AddMessageCommand> validator;

    public AddMessageHandler(AppDbContext db, IFileService fileService, IValidator<AddMessageCommand> validator)
    {
        this.db = db;
        this.fileService = fileService;
        this.validator = validator;
    }
    public async Task<AddMessageResponse> Handle(AddMessageCommand request, CancellationToken cancellationToken = default)
    {
        validator.ValidateAndThrow(request);
        Topic parentTopic = await db.Topics.FirstOrDefaultAsync(x => x.Id == request.TopicId, cancellationToken)
            ?? throw NotFoundException.SetMessageFromType<Topic>();
        DateTimeOffset timeStamp = DateTimeOffset.UtcNow;
        parentTopic.LastMessageTimeStamp = timeStamp;
        Message message = new()
        {
            Content = request.Message,
            TopicId = request.TopicId,
            CreatedAt = timeStamp,
            UserId = request.UserId,
            AttachedFiles = request.AttachedFiles?.Select(x => new MessageAttachedFile() { Name = x.Name }).ToList() ?? []
        };
        parentTopic.Messages.Add(message);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            List<AttachedFileResponse>? fileInfos = null;
            if (request.AttachedFiles?.Length > 0)
            {
                fileInfos = new(request.AttachedFiles.Length);
                foreach (AttachedFileInfo file in request.AttachedFiles)
                {
                    Guid id = message.AttachedFiles.First(x => x.Name == file.Name).Id;
                    string? url = await fileService.Upload(file.FileStream, id + file.Name, cancellationToken);
                    if (url != null)
                    {
                        fileInfos.Add(new AttachedFileResponse() { Name = file.Name, Id = id });
                    }
                }
            }
            return new AddMessageResponse()
            {
                Id = message.Id,
                CreatedAt = timeStamp,
                AttachedFiles = fileInfos?.ToArray()
            };
        }
        catch (ReferenceConstraintException)
        {
            // Only user reference can be invalid since we checked the topic reference before
            throw NotFoundException.SetMessageFromType<User>();
        }
    }
}
