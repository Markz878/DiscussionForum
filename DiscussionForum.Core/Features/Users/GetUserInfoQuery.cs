using DiscussionForum.Shared.DTO.Users;

namespace DiscussionForum.Core.Features.Users;

public sealed record GetUserInfoQuery : IRequest<GetUserInfoResult?>
{
    public required Guid Id { get; init; }
}
public sealed record GetUserInfoResult(Guid Id, string UserName, string Email, DateTimeOffset JoinedAt, Role Role);


internal sealed class GetUserInfoQueryHandler(AppDbContext db) : IRequestHandler<GetUserInfoQuery, GetUserInfoResult?>
{
    public async Task<GetUserInfoResult?> Handle(GetUserInfoQuery message, CancellationToken cancellationToken = default)
    {
        User? user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == message.Id, cancellationToken);
        if (user == null)
        {
            return null;
        }
        return new GetUserInfoResult(user.Id, user.UserName, user.Email, user.JoinedAt, user.Role);
    }
}
