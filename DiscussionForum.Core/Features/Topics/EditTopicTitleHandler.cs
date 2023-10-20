namespace DiscussionForum.Core.Features.Topics;

internal sealed class EditTopicTitleHandler : IRequestHandler<EditTopicTitle>
{
    private readonly AppDbContext _db;

    public EditTopicTitleHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(EditTopicTitle request, CancellationToken cancellationToken = default)
    {
        Guid topicUserId = await _db.GetTopicUserId(request.TopicId, cancellationToken);

        if (topicUserId == Guid.Empty)
        {
            throw NotFoundException.SetMessageFromType<User>();
        }
        if (CommonExtensions.IsUserAdminOrOwner(request.UserRole, topicUserId, request.UserId) is false)
        {
            throw new ForbiddenException();
        }
        int rows = await _db.Topics
            .Where(x => x.Id == request.TopicId)
            .ExecuteUpdateAsync(x => x.SetProperty(x => x.Title, request.NewTitle), cancellationToken);
        if (rows == 0)
        {
            throw NotFoundException.SetMessageFromType<Topic>();
        }
    }
}
