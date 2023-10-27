namespace DiscussionForum.Core.Features.MessageLikes;

public sealed record DeleteMessageLikeCommand : IRequest
{
    public required Guid UserId { get; init; }
    public required long MessageId { get; init; }
}

internal sealed class DeleteMessageLikeCommandHandler : IRequestHandler<DeleteMessageLikeCommand>
{
    private readonly AppDbContext _db;

    public DeleteMessageLikeCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteMessageLikeCommand request, CancellationToken cancellationToken = default)
    {
        int rows = await _db.MessageLikes.Where(x => x.UserId == request.UserId && x.MessageId == request.MessageId).ExecuteDeleteAsync(cancellationToken);
        if (rows == 0)
        {
            throw NotFoundException.SetMessageFromType<MessageLike>();
        }
    }
}
