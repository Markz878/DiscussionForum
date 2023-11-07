using DiscussionForum.Shared.DTO.Users;

namespace DiscussionForum.Core.Features.Topics;

public sealed record DeleteTopicCommand : IRequest
{
    public long TopicId { get; init; }
    public Guid UserId { get; init; }
    public Role UserRole { get; init; }
}

internal sealed class DeleteTopicHandler : IRequestHandler<DeleteTopicCommand>
{
    private readonly AppDbContext _db;
    private readonly IFileService fileService;

    public DeleteTopicHandler(AppDbContext db, IFileService fileService)
    {
        _db = db;
        this.fileService = fileService;
    }

    /// <summary>
    /// Deletes a topic and all related messages
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">Thrown when the Topic is not found</exception>
    /// <exception cref="ForbiddenException"></exception>
    public async Task Handle(DeleteTopicCommand request, CancellationToken cancellationToken = default)
    {
        Guid topicUserId = await _db.GetTopicUserId(request.TopicId, cancellationToken);

        if (topicUserId == Guid.Empty)
        {
            throw NotFoundException.SetMessageFromType<Topic>();
        }
        if (CommonExtensions.IsUserAdminOrOwner(request.UserRole, topicUserId, request.UserId) is false)
        {
            throw new ForbiddenException();
        }
        await DeleteTopicMessageFiles(request, cancellationToken);
        await DeleteTopic(request, cancellationToken);
    }

    private async Task DeleteTopicMessageFiles(DeleteTopicCommand request, CancellationToken cancellationToken)
    {
        string[] files = await _db.MessageAttachedFiles
            .Where(x => x.Message!.TopicId == request.TopicId)
            .Select(x => x.Id.ToString().ToLowerInvariant() + x.Name)
            .ToArrayAsync(cancellationToken);
        await Task.WhenAll(files.Select(x => fileService.Delete(x, cancellationToken)));
    }

    private async Task DeleteTopic(DeleteTopicCommand request, CancellationToken cancellationToken)
    {
        int rows = await _db.Topics
                    .Where(x => x.Id == request.TopicId)
                    .ExecuteDeleteAsync(cancellationToken);
    }
}
