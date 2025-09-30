using DiscussionForum.Core.HelperMethods;
using DiscussionForum.Shared.DTO.Messages;
using DiscussionForum.Shared.DTO.Users;
using DiscussionForum.Shared.Interfaces;

namespace DiscussionForum.Core.Services;

internal sealed class MessagesService(AppDbContext db, IFileService fileService, IUserInfoService userInfoService) : IMessagesService
{
    public async Task<AddMessageResponse> AddMessage(long topicId, string message, AttachedFileInfo[]? attachedFiles = null, CancellationToken cancellationToken = default)
    {
        UserInfo userInfo = userInfoService.GetCurrentUserInfo() ?? throw new ForbiddenException();
        Topic parentTopic = await db.Topics.FirstOrDefaultAsync(x => x.Id == topicId, cancellationToken)
            ?? throw NotFoundException.SetMessageFromType<Topic>();
        DateTimeOffset timeStamp = DateTimeOffset.UtcNow;
        parentTopic.LastMessageTimeStamp = timeStamp;
        Message messageEntity = new()
        {
            Content = message,
            TopicId = topicId,
            CreatedAt = timeStamp,
            UserId = userInfo.Id,
            AttachedFiles = attachedFiles?.Select(x => new MessageAttachedFile() { Name = x.Name }).ToList() ?? []
        };
        try
        {
            parentTopic.Messages.Add(messageEntity);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (ReferenceConstraintException)
        {
            // Only user reference can be invalid since we checked the topic reference before
            throw NotFoundException.SetMessageFromType<User>();
        }
        List<AttachedFileResponse>? fileInfos = null;
        if (attachedFiles?.Length > 0)
        {
            fileInfos = new(attachedFiles.Length);
            List<Task<string>> uploadTasks = new(attachedFiles.Length);
            foreach (AttachedFileInfo file in attachedFiles)
            {
                Guid id = messageEntity.AttachedFiles.First(x => x.Name == file.Name).Id;
                uploadTasks.Add(fileService.Upload(file.FileStream, id + file.Name, cancellationToken));
                fileInfos.Add(new AttachedFileResponse() { Name = file.Name, Id = id });
            }
            await Task.WhenAll(uploadTasks);
        }
        return new AddMessageResponse()
        {
            Id = messageEntity.Id,
            CreatedAt = timeStamp,
            AttachedFiles = fileInfos?.ToArray()
        };
    }

    public async Task DeleteMessage(long messageId, CancellationToken cancellationToken = default)
    {
        var messageInfo = await db.Messages
            .Where(x => x.Id == messageId)
            .Select(x => new { x.UserId, x.TopicId })
            .SingleOrDefaultAsync(cancellationToken);

        if (messageInfo == null)
        {
            throw NotFoundException.SetMessageFromType<Message>();
        }
        UserInfo? userInfo = userInfoService.GetCurrentUserInfo();
        if (CommonExtensions.IsUserAdminOrOwner(userInfo, messageInfo.UserId) is false)
        {
            throw new ForbiddenException();
        }
        if (await CheckIfFirstMessage(messageInfo.TopicId, messageId))
        {
            throw new BusinessException("Can't delete topic's first message.");
        }
        await DeleteMessageFiles(messageId, cancellationToken);
        await db.Messages.Where(x => x.Id == messageId).ExecuteDeleteAsync(cancellationToken);

        async Task<bool> CheckIfFirstMessage(long topicId, long messageId)
        {
            long x = await db.Topics
                .Where(x => x.Id == topicId)
                .Select(x => x.Messages.OrderBy(x => x.CreatedAt).First().Id)
                .SingleAsync(cancellationToken);
            return x == messageId;
        }

        async Task DeleteMessageFiles(long messageId, CancellationToken cancellationToken)
        {
            string[] files = await db.MessageAttachedFiles
                .Where(x => x.MessageId == messageId)
                .Select(x => x.Id.ToString().ToLowerInvariant() + x.Name) // Blob storage uses lower case guids
                .ToArrayAsync(cancellationToken);
            await Task.WhenAll(files.Select(x => fileService.Delete(x, cancellationToken)));
        }
    }

    public async Task<EditMessageResult> EditMessage(long messageId, string content, CancellationToken cancellationToken = default)
    {
        var message = await db.Messages
            .Where(x => x.Id == messageId)
            .Select(x => new { x.UserId, x.TopicId })
            .SingleOrDefaultAsync(cancellationToken);

        if (message is null)
        {
            throw NotFoundException.SetMessageFromType<Message>();
        }
        UserInfo? userInfo = userInfoService.GetCurrentUserInfo();
        if (CommonExtensions.IsUserAdminOrOwner(userInfo, message.UserId) is false)
        {
            throw new ForbiddenException();
        }
        DateTimeOffset timeStamp = DateTimeOffset.UtcNow;
        await db.Messages
            .Where(x => x.Id == messageId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.Content, content)
                .SetProperty(x => x.EditedAt, timeStamp), cancellationToken);
        return new EditMessageResult() { EditedAt = timeStamp, TopicId = message.TopicId };
    }

    public async Task<string?> GetFileNameById(Guid id, CancellationToken cancellationToken = default)
    {
        string? name = await db.MessageAttachedFiles
            .Where(x => x.Id == id)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);
        return name;
    }

    public async Task<long> GetMessageTopicId(long messageId, CancellationToken cancellationToken = default)
    {
        long topicId = await db.Messages
            .Where(x => x.Id == messageId)
            .Select(x => x.TopicId)
            .SingleOrDefaultAsync(cancellationToken);

        if (topicId == 0)
        {
            throw NotFoundException.SetMessageFromType<Message>();
        }

        return topicId;
    }
}