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
        try
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
        catch
        {
            return new() { Topics = [] };
        }
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
        List<TopicResult> result = await db
            .Database.SqlQuery<TopicResult>($"""
            SELECT [t0].[Id], [t0].[Title], (
                SELECT COUNT(*)
                FROM [Messages] AS [m]
                WHERE [t0].[Id] = [m].[TopicId]) AS [MessageCount], [u].[UserName], [t0].[CreatedAt] AS [Created], [t0].[LastMessageTimeStamp]
            FROM (
                SELECT [t].[Id], [t].[CreatedAt], [t].[LastMessageTimeStamp], [t].[Title], [t].[UserId]
                FROM Topics AS [t]
                INNER JOIN FREETEXTTABLE(dbo.Topics, Title, {searchText}) AS KEY_TBL ON [t].[Id] = KEY_TBL.[KEY]
                WHERE KEY_TBL.RANK = (select Max(KEY_TBL.RANK) from [Topics] as t
                    INNER JOIN FREETEXTTABLE(dbo.Topics, Title, {searchText}) AS KEY_TBL ON [t].[Id] = KEY_TBL.[KEY])
                ORDER BY [t].[LastMessageTimeStamp] DESC
                OFFSET {pageNumber * topicsCount} ROWS FETCH NEXT {topicsCount} ROWS ONLY
            ) AS [t0]
            INNER JOIN [Users] AS [u] ON [t0].[UserId] = [u].[Id]
            ORDER BY [t0].[LastMessageTimeStamp] DESC
            """).ToListAsync(cancellationToken);

        Count count = await db.Database.SqlQuery<Count>($"""
            SELECT COUNT(*) as Value
            FROM Topics AS [t]
            INNER JOIN FREETEXTTABLE(dbo.Topics, Title, {searchText}) AS KEY_TBL ON [t].[Id] = KEY_TBL.[KEY]
            WHERE KEY_TBL.RANK = (select Max(KEY_TBL.RANK) from [Topics] as t
            INNER JOIN FREETEXTTABLE(dbo.Topics, Title, {searchText}) AS KEY_TBL ON [t].[Id] = KEY_TBL.[KEY])
            """).FirstAsync(cancellationToken);
        return (result, count.Value);
    }

    [GeneratedRegex("^[a-zA-Z0-9?!$€%= ]*$")]
    private static partial Regex Filter();
}

file class Count
{
    public int Value { get; set; }
}

