namespace DiscussionForum.Core.Features.Messages;

public sealed record GetMessageTopicId() : IRequest<GetMessageTopicIdResult?>
{
    public required long MessageId { get; init; }
}

public sealed record GetMessageTopicIdResult()
{
    public required long TopicId { get; init; }
}

internal sealed class GetMessageTopicIdHandler : IRequestHandler<GetMessageTopicId, GetMessageTopicIdResult?>
{
    private readonly AppDbContext _db;

    public GetMessageTopicIdHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GetMessageTopicIdResult?> Handle(GetMessageTopicId request, CancellationToken cancellationToken)
    {
        long topicId = await _db.Messages.Where(x => x.Id == request.MessageId).Select(x => x.TopicId).SingleOrDefaultAsync(cancellationToken);
        if (topicId == 0)
        {
            return null;
        }
        return new GetMessageTopicIdResult() { TopicId = topicId };
    }
}
