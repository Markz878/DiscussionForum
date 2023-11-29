using DiscussionForum.Shared.DTO;
using DiscussionForum.Shared.DTO.Topics;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace DiscussionForum.Core.Features.Topics;

public sealed record ListLatestTopicsQuery : IRequest<ListLatestTopicsResult>
{
    public int PageNumber { get; init; }
    public string? SearchText { get; init; }
    public int TopicsCount { get; init; } = 10;
}

internal sealed class ListLatestTopicsQueryHandler : IRequestHandler<ListLatestTopicsQuery, ListLatestTopicsResult>
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache cache;

    public ListLatestTopicsQueryHandler(AppDbContext db, IDistributedCache cache)
    {
        _db = db;
        this.cache = cache;
    }

    public async Task<ListLatestTopicsResult> Handle(ListLatestTopicsQuery request, CancellationToken cancellationToken = default)
    {
        if (request.TopicsCount <= 0)
        {
            throw new ValidationException($"{nameof(request.TopicsCount)} must be larger than 0.");
        }
        string cacheKey = $"topics-{request.PageNumber}-{request.TopicsCount}-{request.SearchText}";
        byte[]? cachedResultBytes = await cache.GetAsync(cacheKey, cancellationToken);
        if (cachedResultBytes is not null)
        {
            ListLatestTopicsResult? cachedResult = JsonSerializer.Deserialize(cachedResultBytes, JsonContext.Default.ListLatestTopicsResult);
            if (cachedResult is not null)
            {
                return cachedResult;
            }
        }
        IQueryable<Topic> query = _db.Topics;
        if (string.IsNullOrWhiteSpace(request.SearchText) is false)
        {
            query = query.Where(x => EF.Functions.Contains(x.Title, request.SearchText));
        }
        List<TopicResult> topics = await GetTopics(query, request.PageNumber, request.TopicsCount, cancellationToken);
        int topicsCount = await query.CountAsync(cancellationToken);
        ListLatestTopicsResult result = new()
        {
            Topics = topics,
            PageCount = Math.Max((topicsCount - 1) / request.TopicsCount, 0)
        };
        await cache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(result, JsonContext.Default.ListLatestTopicsResult), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
        }, cancellationToken);
        return result;
    }

    private static async Task<List<TopicResult>> GetTopics(IQueryable<Topic> query, int pageNumber, int topicsCount, CancellationToken cancellationToken)
    {
        return await query
            .OrderByDescending(x => x.LastMessageTimeStamp)
            .Select(x => new TopicResult
            {
                Id = x.Id,
                Title = x.Title,
                MessageCount = x.Messages.Count,
                UserName = x.User!.UserName,
                Created = x.CreatedAt,
                LastMessageTimeStamp = x.LastMessageTimeStamp
            })
            .Skip(pageNumber * topicsCount)
            .Take(topicsCount)
            .ToListAsync(cancellationToken);
    }
}

