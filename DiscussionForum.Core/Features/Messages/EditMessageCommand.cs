﻿using DiscussionForum.Shared.DTO.Messages;
using DiscussionForum.Shared.DTO.Users;

namespace DiscussionForum.Core.Features.Messages;

public sealed record EditMessageCommand : IRequest<EditMessageResult>
{
    public long MessageId { get; set; }
    public required string Message { get; set; }
    public Guid UserId { get; set; }
    public Role UserRole { get; set; }
}

internal sealed class EditMessageHandler : IRequestHandler<EditMessageCommand, EditMessageResult>
{
    private readonly AppDbContext _db;

    public EditMessageHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EditMessageResult> Handle(EditMessageCommand request, CancellationToken cancellationToken = default)
    {
        Guid messageUserId = await _db.Messages
            .Where(x => x.Id == request.MessageId)
            .Select(x => x.UserId)
            .SingleOrDefaultAsync(cancellationToken);

        if (messageUserId == Guid.Empty)
        {
            throw NotFoundException.SetMessageFromType<Message>();
        }
        if (CommonExtensions.IsUserAdminOrOwner(request.UserRole, messageUserId, request.UserId) is false)
        {
            throw new ForbiddenException();
        }
        DateTimeOffset timeStamp = DateTimeOffset.UtcNow;
        await _db.Messages
            .Where(x => x.Id == request.MessageId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.Content, request.Message)
                .SetProperty(x => x.EditedAt, timeStamp), cancellationToken);
        return new EditMessageResult() { EditedAt = timeStamp };
    }
}