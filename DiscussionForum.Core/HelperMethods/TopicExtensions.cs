namespace DiscussionForum.Core.HelperMethods;

internal static class TopicExtensions
{
    internal static async Task<Guid> GetTopicUserId(this AppDbContext db, long topicId, CancellationToken cancellationToken = default)
    {
        Guid userId = await db.Topics
            .Where(x => x.Id == topicId)
            .Select(x => x.UserId)
            .SingleOrDefaultAsync(cancellationToken);
        return userId;
    }
}
