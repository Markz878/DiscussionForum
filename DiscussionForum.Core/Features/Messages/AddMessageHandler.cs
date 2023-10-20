namespace DiscussionForum.Core.Features.Messages;

internal sealed class AddMessageHandler : IRequestHandler<AddMessage, AddMessageResult>
{
    private readonly AppDbContext db;
    private readonly IFileService fileService;
    private readonly IValidator<AddMessage> validator;

    public AddMessageHandler(AppDbContext db, IFileService fileService, IValidator<AddMessage> validator)
    {
        this.db = db;
        this.fileService = fileService;
        this.validator = validator;
    }
    public async Task<AddMessageResult> Handle(AddMessage request, CancellationToken cancellationToken = default)
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
            AttachedFiles = request.AttachedFiles?.Select(x => new MessageAttachedFile() { Name = x.Name }).ToList() ?? new()
        };
        parentTopic.Messages.Add(message);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            List<AttachedFileInfo>? fileInfos = null;
            if (request.AttachedFiles?.Length > 0)
            {
                fileInfos = new(request.AttachedFiles.Length);
                foreach (AddAttachedFile file in request.AttachedFiles)
                {
                    Guid id = message.AttachedFiles.First(x => x.Name == file.Name).Id;
                    string? url = await fileService.Upload(file.FileStream, id + file.Name, cancellationToken);
                    if (url != null)
                    {
                        fileInfos.Add(new AttachedFileInfo() { Name = file.Name, Id = id });
                    }
                }
            }
            return new AddMessageResult()
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
