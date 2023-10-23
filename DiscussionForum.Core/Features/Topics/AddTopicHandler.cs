namespace DiscussionForum.Core.Features.Topics;

internal sealed class AddTopicHandler : IRequestHandler<AddTopic, AddTopicResult>
{
    private readonly AppDbContext _db;
    private readonly IFileService _fileService;

    public AddTopicHandler(AppDbContext db, IFileService fileService)
    {
        _db = db;
        _fileService = fileService;
    }

    public async Task<AddTopicResult> Handle(AddTopic request, CancellationToken cancellationToken = default)
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
        _db.Topics.Add(newTopic);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            if (request.AttachedFiles?.Length > 0)
            {
                foreach (AddAttachedFile file in request.AttachedFiles)
                {
                    Guid id = newTopic.Messages.First().AttachedFiles.First(x => x.Name == file.Name).Id;
                    string? url = await _fileService.Upload(file.FileStream, id + file.Name, cancellationToken);
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
