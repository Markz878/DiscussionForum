namespace DiscussionForum.Core.Features.MessageLikes;

internal sealed class DeleteMessageLikeHandler : IRequestHandler<DeleteMessageLike>
{
    private readonly AppDbContext _db;

    public DeleteMessageLikeHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteMessageLike request, CancellationToken cancellationToken = default)
    {
        int rows = await _db.MessageLikes.Where(x => x.UserId == request.UserId && x.MessageId == request.MessageId).ExecuteDeleteAsync(cancellationToken);
        if (rows == 0)
        {
            throw NotFoundException.SetMessageFromType<MessageLike>();
        }
    }
}
