using DiscussionForum.Shared.DTO.Users;

namespace DiscussionForum.Core.Features.Messages;

public sealed record DeleteMessageCommand : IRequest
{
    public required long TopicId { get; init; }
    public required long MessageId { get; init; }
    public Guid UserId { get; set; }
    public Role UserRole { get; set; }
}

internal class DeleteMessageHandler : IRequestHandler<DeleteMessageCommand>
{
    private readonly AppDbContext _db;
    private readonly IFileService fileService;

    public DeleteMessageHandler(AppDbContext db, IFileService fileService)
    {
        _db = db;
        this.fileService = fileService;
    }

    public async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken = default)
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
        if (await CheckIfFirstMessage(request.TopicId, request.MessageId))
        {
            throw new BusinessException("Can't delete topic's first message.");
        }
        await DeleteMessageFiles(request, cancellationToken);
        await DeleteMessage(request, cancellationToken);
    }

    private async Task<bool> CheckIfFirstMessage(long topicId, long messageId)
    {
        long x = await _db.Topics
            .Where(x => x.Id == topicId)
            .Select(x => x.Messages.OrderBy(x => x.CreatedAt).First().Id)
            .SingleAsync();
        return x == messageId;
    }

    private async Task DeleteMessageFiles(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        string[] files = await _db.MessageAttachedFiles
            .Where(x => x.MessageId == request.MessageId)
            .Select(x => x.Id.ToString().ToLowerInvariant() + x.Name) // Blob storage uses lower case guids
            .ToArrayAsync(cancellationToken);
        await Task.WhenAll(files.Select(x => fileService.Delete(x, cancellationToken)));
    }

    private async Task DeleteMessage(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        await _db.Messages.Where(x => x.Id == request.MessageId).ExecuteDeleteAsync(cancellationToken);
    }
}
