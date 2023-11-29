namespace DiscussionForum.Core.Features.Messages;

public sealed record GetMessageTopicIdQuery() : IRequest<GetMessageTopicIdResult?>
{
    public required long MessageId { get; init; }
}

public sealed record GetMessageTopicIdResult()
{
    public required long TopicId { get; init; }
}

internal sealed class GetMessageTopicIdQueryHandler(AppDbContext db) : IRequestHandler<GetMessageTopicIdQuery, GetMessageTopicIdResult?>
{
    public async Task<GetMessageTopicIdResult?> Handle(GetMessageTopicIdQuery request, CancellationToken cancellationToken)
    {
        long topicId = await db.Messages
            .Where(x => x.Id == request.MessageId)
            .Select(x => x.TopicId)
            .SingleOrDefaultAsync(cancellationToken);
        if (topicId == 0)
        {
            return null;
        }
        return new GetMessageTopicIdResult() { TopicId = topicId };
    }
}
