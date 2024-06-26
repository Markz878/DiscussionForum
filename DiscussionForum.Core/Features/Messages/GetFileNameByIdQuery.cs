﻿namespace DiscussionForum.Core.Features.Messages;

public sealed record GetFileNameByIdQuery : IRequest<GetFileNameByIdResult>
{
    public required Guid Id { get; init; }
}

public sealed record GetFileNameByIdResult
{
    public required string FileName { get; init; }
}

internal sealed class GetFileNameByIdQueryHandler(AppDbContext db) : IRequestHandler<GetFileNameByIdQuery, GetFileNameByIdResult?>
{
    public async Task<GetFileNameByIdResult?> Handle(GetFileNameByIdQuery message, CancellationToken cancellationToken = default)
    {
        string? name = await db.MessageAttachedFiles
            .Where(x => x.Id == message.Id)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);
        return name == null ? null : new GetFileNameByIdResult() { FileName = name };
    }
}
