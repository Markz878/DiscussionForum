namespace DiscussionForum.Core.Features.Messages;

internal sealed class EditMessageHandler : IRequestHandler<EditMessage, EditMessageResult>
{
    private readonly AppDbContext _db;
    private readonly IValidator<EditMessage> _validator;

    public EditMessageHandler(AppDbContext db, IValidator<EditMessage> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<EditMessageResult> Handle(EditMessage request, CancellationToken cancellationToken = default)
    {
        _validator.ValidateAndThrow(request);
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
