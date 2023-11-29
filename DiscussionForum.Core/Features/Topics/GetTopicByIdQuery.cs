using DiscussionForum.Shared.DTO;
using DiscussionForum.Shared.DTO.Messages;
using DiscussionForum.Shared.DTO.Topics;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace DiscussionForum.Core.Features.Topics;

public sealed record GetTopicByIdQuery : IRequest<GetTopicByIdResult>
{
    public long TopicId { get; set; }
    public Guid? UserId { get; set; }
}

internal sealed class GetTopicByIdQueryHandler(AppDbContext db, IDistributedCache cache) : IRequestHandler<GetTopicByIdQuery, GetTopicByIdResult?>
{
    public async Task<GetTopicByIdResult?> Handle(GetTopicByIdQuery request, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"topic-{request.TopicId}-{request.UserId}";
        byte[]? cachedResultBytes = await cache.GetAsync(cacheKey, cancellationToken);
        if (cachedResultBytes is not null)
        {
            GetTopicByIdResult? cachedResult = JsonSerializer.Deserialize(cachedResultBytes, JsonContext.Default.GetTopicByIdResult);
            if (cachedResult is not null)
            {
                return cachedResult;
            }
        }
        GetTopicByIdResult? result = await db.Topics
            .AsSplitQuery()
            .Where(x => x.Id == request.TopicId)
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
                        AttachedFiles = m.AttachedFiles.Select(x => new AttachedFileResponse()
                        {
                            Id = x.Id,
                            Name = x.Name
                        }).ToArray()
                    }).ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (result is not null)
        {
            await cache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(result, JsonContext.Default.GetTopicByIdResult), new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
            }, cancellationToken);
        }
        return result;
    }
}
