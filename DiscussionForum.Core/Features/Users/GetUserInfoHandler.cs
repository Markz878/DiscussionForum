namespace DiscussionForum.Core.Features.Users;

internal sealed class GetUserInfoHandler : IRequestHandler<GetUserInfo, GetUserInfoResult?>
{
    private readonly AppDbContext _db;

    public GetUserInfoHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GetUserInfoResult?> Handle(GetUserInfo message, CancellationToken cancellationToken = default)
    {
        User? user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == message.Id, cancellationToken);
        if (user == null)
        {
            return null;
        }
        return new GetUserInfoResult(user.Id, user.UserName, user.Email, user.JoinedAt, user.Role);
    }
}
