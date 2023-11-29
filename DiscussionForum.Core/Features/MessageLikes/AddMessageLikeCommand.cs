namespace DiscussionForum.Core.Features.MessageLikes;

public sealed record AddMessageLikeCommand : IRequest
{
    public required Guid UserId { get; init; }
    public required long MessageId { get; init; }
}

internal sealed class AddMessageLikeCommandHandler(AppDbContext db) : IRequestHandler<AddMessageLikeCommand>
{
    public async Task Handle(AddMessageLikeCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            MessageLike messageLike = new()
            {
                MessageId = request.MessageId,
                UserId = request.UserId,
            };
            db.MessageLikes.Add(messageLike);
            await db.SaveChangesAsync(cancellationToken);
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
