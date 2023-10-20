namespace DiscussionForum.Core.Features.Topics;

internal sealed class GetTopicByIdHandler : IRequestHandler<GetTopicById, GetTopicByIdResult?>
{
    private readonly AppDbContext _db;
    public GetTopicByIdHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GetTopicByIdResult?> Handle(GetTopicById request, CancellationToken cancellationToken = default)
    {
        GetTopicByIdResult? result = await _db.Topics
            .AsSplitQuery()
            .Where(x => x.Id == request.Id)
            .Select(x => new GetTopicByIdResult()
            {
                Id = x.Id,
                Title = x.Title,
                CreatedAt = x.CreatedAt,
                UserName = x.User!.UserName,
                Messages = x.Messages
                    .OrderBy(x => x.CreatedAt)
                    .Select(m => new TopicMessage
                    {
                        Id = m.Id,
                        CreatedAt = m.CreatedAt,
                        EditedAt = m.EditedAt,
                        UserName = m.User!.UserName,
                        Content = m.Content,
                        LikesCount = m.MessageLikes.Count,
                        HasUserUpvoted = m.MessageLikes.Any(x => x.UserId == request.UserId),
                        AttachedFiles = m.AttachedFiles.Select(x => new AttachedFileInfo()
                        {
                            Id = x.Id,
                            Name = x.Name
                        }).ToArray()
                    }).ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);
        return result;
    }
}
