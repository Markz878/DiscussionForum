namespace DiscussionForum.Core.Features.Messages;
internal class DeleteMessageHandler : IRequestHandler<DeleteMessage>
{
    private readonly AppDbContext _db;
    private readonly IFileService fileService;

    public DeleteMessageHandler(AppDbContext db, IFileService fileService)
    {
        _db = db;
        this.fileService = fileService;
    }

    public async Task Handle(DeleteMessage request, CancellationToken cancellationToken = default)
    {
        Guid messageUsedId = await _db.Messages.Where(x => x.Id == request.MessageId).Select(x => x.UserId).SingleOrDefaultAsync(cancellationToken);
        if (messageUsedId == Guid.Empty)
        {
            throw NotFoundException.SetMessageFromType<Message>();
        }
        if (CommonExtensions.IsUserAdminOrOwner(request.UserRole, messageUsedId, request.UserId) is false)
        {
            throw new ForbiddenException();
        }
        await DeleteMessageFiles(request, cancellationToken);
        await DeleteMessage(request, cancellationToken);
    }

    private async Task DeleteMessageFiles(DeleteMessage request, CancellationToken cancellationToken)
    {
        string[] files = await _db.MessageAttachedFiles
            .Where(x => x.MessageId == request.MessageId)
            .Select(x => x.Id.ToString().ToLowerInvariant() + x.Name) // Blob storage uses lower case guids
            .ToArrayAsync(cancellationToken);
        await Task.WhenAll(files.Select(x => fileService.Delete(x, cancellationToken)));
    }

    private async Task DeleteMessage(DeleteMessage request, CancellationToken cancellationToken)
    {
        await _db.Messages.Where(x => x.Id == request.MessageId && x.UserId == request.UserId).ExecuteDeleteAsync(cancellationToken);
    }
}
