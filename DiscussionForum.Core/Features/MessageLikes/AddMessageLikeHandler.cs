namespace DiscussionForum.Core.Features.MessageLikes;

internal sealed class AddMessageLikeHandler : IRequestHandler<AddMessageLike>
{
    private readonly AppDbContext _db;

    public AddMessageLikeHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(AddMessageLike request, CancellationToken cancellationToken = default)
    {
        try
        {
            MessageLike messageLike = new()
            {
                MessageId = request.MessageId,
                UserId = request.UserId,
            };
            _db.MessageLikes.Add(messageLike);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException)
        {
            throw new ConflictException("User has already liked the message");
        }
        catch (ReferenceConstraintException)
        {
            throw NotFoundException.SetMessageFromType<Message>();
        }
    }
}
