using DiscussionForum.Core.HelperMethods;
using DiscussionForum.Shared.DTO;
using DiscussionForum.Shared.DTO.Messages;
using DiscussionForum.Shared.DTO.Topics;
using DiscussionForum.Shared.DTO.Users;
using DiscussionForum.Shared.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DiscussionForum.Core.Services;

internal sealed partial class TopicsService(AppDbContext db, IFileService fileService, IDistributedCache cache, IUserInfoService userInfoService) : ITopicsService
{
    public async Task<AddTopicResult> AddTopic(string title, string firstMessage, AttachedFileInfo[]? attachedFiles = null, CancellationToken cancellationToken = default)
    {
        DateTimeOffset timeStamp = DateTimeOffset.UtcNow;
        UserInfo userInfo = userInfoService.GetCurrentUserInfo() ?? throw new ForbiddenException();
        Topic newTopic = new()
        {
            Title = title,
            CreatedAt = timeStamp,
            UserId = userInfo.Id,
            LastMessageTimeStamp = timeStamp,
            Messages =
            [
                new()
                {
                    Content = firstMessage,
                    CreatedAt = timeStamp,
                    UserId = userInfo.Id,
                    AttachedFiles = attachedFiles?.Select(x => new MessageAttachedFile()
                    {
                        Name = x.Name
                    }).ToList() ?? [],
                }
            ]
        };
        try
        {
            db.Topics.Add(newTopic);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (ReferenceConstraintException)
        {
            throw NotFoundException.SetMessageFromType<User>();
        }
        if (attachedFiles?.Length > 0)
        {
            List<Task<string>> uploadTasks = new(attachedFiles.Length);
            foreach (AttachedFileInfo file in attachedFiles)
            {
                Guid id = newTopic.Messages[0].AttachedFiles.First(x => x.Name == file.Name).Id;
                uploadTasks.Add(fileService.Upload(file.FileStream, id + file.Name, cancellationToken));
            }
            await Task.WhenAll(uploadTasks);
        }
        return new AddTopicResult { Id = newTopic.Id };
    }

    public async Task DeleteTopic(long topicId, CancellationToken cancellationToken = default)
    {
        Guid topicUserId = await db.GetTopicUserId(topicId, cancellationToken);

        if (topicUserId == Guid.Empty)
        {
            throw NotFoundException.SetMessageFromType<Topic>();
        }
        if (CommonExtensions.IsUserAdminOrOwner(userInfoService.GetCurrentUserInfo(), topicUserId) is false)
        {
            throw new ForbiddenException();
        }
        await DeleteTopicMessageFiles(topicId, cancellationToken);
        int rows = await db.Topics
                    .Where(x => x.Id == topicId)
                    .ExecuteDeleteAsync(cancellationToken);

        async Task DeleteTopicMessageFiles(long topicId, CancellationToken cancellationToken)
        {
            string[] files = await db.MessageAttachedFiles
                .Where(x => x.Message!.TopicId == topicId)
                .Select(x => x.Id.ToString().ToLowerInvariant() + x.Name)
                .ToArrayAsync(cancellationToken);
            await Task.WhenAll(files.Select(x => fileService.Delete(x, cancellationToken)));
        }
    }

    public async Task EditTopicTitle(long topicId, string newTitle, CancellationToken cancellationToken = default)
    {
        Guid topicUserId = await db.GetTopicUserId(topicId, cancellationToken);

        if (topicUserId == Guid.Empty)
        {
            throw NotFoundException.SetMessageFromType<User>();
        }
        if (CommonExtensions.IsUserAdminOrOwner(userInfoService.GetCurrentUserInfo(), topicUserId) is false)
        {
            throw new ForbiddenException();
        }
        int rows = await db.Topics
            .Where(x => x.Id == topicId)
            .ExecuteUpdateAsync(x => x.SetProperty(x => x.Title, newTitle), cancellationToken);
        if (rows == 0)
        {
            throw NotFoundException.SetMessageFromType<Topic>();
        }
    }

    public async Task<GetTopicByIdResult?> GetTopicById(long topicId, CancellationToken cancellationToken = default)
    {
        UserInfo? userInfo = userInfoService.GetCurrentUserInfo();
        string cacheKey = $"topic-{topicId}-{userInfo?.Id}";
        byte[]? cachedResultBytes = await cache.GetAsync(cacheKey, cancellationToken);
        if (cachedResultBytes is not null)
        {
            GetTopicByIdResult? cachedResult = JsonSerializer.Deserialize(cachedResultBytes, JsonContext.Default.GetTopicByIdResult);
            if (cachedResult is not null)
            {
                return cachedResult;
            }
        }
        Guid userId = userInfo?.Id ?? Guid.Empty;
        GetTopicByIdResult? result = await db.Topics
            .AsSplitQuery()
            .Where(x => x.Id == topicId)
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
                        HasUserUpvoted = m.MessageLikes.Any(x => x.UserId == userId),
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

    public async Task<ListLatestTopicsResult> ListLatestTopics(int page, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        string? search = WebUtility.HtmlEncode(searchTerm);
        if (pageSize <= 0)
        {
            throw new ValidationException($"{nameof(pageSize)} must be larger than 0.");
        }
        if (!string.IsNullOrEmpty(search) && !Filter().IsMatch(search))
        {
            return new ListLatestTopicsResult() { Topics = [] };
        }
        string cacheKey = $"topics-{page}-{pageSize}-{search}";
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
            await GetTopics(page, pageSize, cancellationToken) :
            await GetFreeTextSearchTopics(page, pageSize, search, cancellationToken);
        ListLatestTopicsResult result = new()
        {
            Topics = topics,
            PageCount = Math.Max((count - 1) / pageSize, 0)
        };
        await cache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(result, JsonContext.Default.ListLatestTopicsResult), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
        }, cancellationToken);
        return result;
    }

    private async Task<(List<TopicResult>, int count)> GetTopics(int pageNumber, int topicsCount, CancellationToken cancellationToken)
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

    private async Task<(List<TopicResult>, int count)> GetFreeTextSearchTopics(int pageNumber, int topicsCount, string searchText, CancellationToken cancellationToken)
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