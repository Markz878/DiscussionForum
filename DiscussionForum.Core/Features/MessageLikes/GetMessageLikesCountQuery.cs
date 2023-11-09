namespace DiscussionForum.Core.Features.MessageLikes;

public sealed record GetMessageLikesCountQuery : IRequest<GetMessageLikesCountResult>
{
    public required long MessageId { get; init; }
}
public sealed record GetMessageLikesCountResult
{
    public required int Count { get; init; }
}

internal class GetMessageLikesCountQueryHandler : IRequestHandler<GetMessageLikesCountQuery, GetMessageLikesCountResult>
{
    private readonly AppDbContext _db;

    public GetMessageLikesCountQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GetMessageLikesCountResult> Handle(GetMessageLikesCountQuery message, CancellationToken cancellationToken = default)
    {
        int count = await _db.MessageLikes.Where(x => x.MessageId == message.MessageId).CountAsync(cancellationToken);
        return new GetMessageLikesCountResult() { Count = count };
    }
}
