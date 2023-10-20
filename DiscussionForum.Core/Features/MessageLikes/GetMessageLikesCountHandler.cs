namespace DiscussionForum.Core.Features.MessageLikes;

internal class GetMessageLikesCountHandler : IRequestHandler<GetMessageLikesCount, GetMessageLikesCountResult>
{
    private readonly AppDbContext _db;

    public GetMessageLikesCountHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GetMessageLikesCountResult> Handle(GetMessageLikesCount message, CancellationToken cancellationToken = default)
    {
        int count = await _db.MessageLikes.Where(x => x.MessageId == message.MessageId).CountAsync(cancellationToken);
        return new GetMessageLikesCountResult() { Count = count };
    }
}
