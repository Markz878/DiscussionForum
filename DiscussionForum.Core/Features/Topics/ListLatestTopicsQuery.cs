using DiscussionForum.Shared.DTO;
using DiscussionForum.Shared.DTO.Topics;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DiscussionForum.Core.Features.Topics;

public sealed record ListLatestTopicsQuery : IRequest<ListLatestTopicsResult>
{
    public int PageNumber { get; init; }
    public string? SearchText { get; init; }
    public int TopicsCount { get; init; } = 10;
}

internal sealed partial class ListLatestTopicsQueryHandler(AppDbContext db, IDistributedCache cache) : IRequestHandler<ListLatestTopicsQuery, ListLatestTopicsResult>
{
    public async Task<ListLatestTopicsResult> Handle(ListLatestTopicsQuery request, CancellationToken cancellationToken = default)
    {
        string? search = WebUtility.HtmlEncode(request.SearchText);
        if (request.TopicsCount <= 0)
        {
            throw new ValidationException($"{nameof(request.TopicsCount)} must be larger than 0.");
        }
        if (!string.IsNullOrEmpty(search) && !Filter().IsMatch(search))
        {
            return new ListLatestTopicsResult() { Topics = [] };
        }
        string cacheKey = $"topics-{request.PageNumber}-{request.TopicsCount}-{search}";
        byte[]? cachedResultBytes = await cache.GetAsync(cacheKey, cancellationToken);
        if (cachedResultBytes is not null)
        {
            ListLatestTopicsResult? cachedResult = JsonSerializer.Deserialize(cachedResultBytes, JsonContext.Default.ListLatestTopicsResult);
            if (cachedResult is not null)
            {
                return cachedResult;
            }
        }
        (List<TopicResult> topics, int count) = string.IsNullOrEmpty(search) ?
            await GetTopics(db, request.PageNumber, request.TopicsCount, cancellationToken) :
            await GetFreeTextSearchTopics(db, request.PageNumber, request.TopicsCount, search, cancellationToken);
        ListLatestTopicsResult result = new()
        {
            Topics = topics,
            PageCount = Math.Max((count - 1) / request.TopicsCount, 0)
        };
        await cache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(result, JsonContext.Default.ListLatestTopicsResult), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
        }, cancellationToken);
        return result;
    }

    private static async Task<(List<TopicResult>, int count)> GetTopics(AppDbContext db, int pageNumber, int topicsCount, CancellationToken cancellationToken)
    {
        IQueryable<TopicResult> query = db.Topics
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
            .Take(topicsCount);
        List<TopicResult> result = await query.ToListAsync(cancellationToken);
        int count = await db.Topics.CountAsync(cancellationToken);
        return (result, count);
    }

    private static async Task<(List<TopicResult>, int count)> GetFreeTextSearchTopics(AppDbContext db, int pageNumber, int topicsCount, string searchText, CancellationToken cancellationToken)
    {
        List<TopicResult> result = await db.Topics.Where(x => EF.Functions.FreeText(x.Title, searchText))
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

        int count = await db.Topics.Where(x => EF.Functions.FreeText(x.Title, searchText)).CountAsync(cancellationToken);

        return (result, count);
    }

    [GeneratedRegex("^[a-zA-Z0-9?!$€%= ]*$")]
    private static partial Regex Filter();
}

