using DiscussionForum.Shared.DTO.Users;
using DiscussionForum.Shared.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace DiscussionForum.Core.Services;

internal sealed class UsersService(AppDbContext db, ILogger<UsersService> logger, IDistributedCache cache) : IUsersService
{
    public async Task<UserInfo?> GetUserInfo(Guid userId, CancellationToken cancellationToken = default)
    {
        User? user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user == null)
        {
            return null;
        }
        return new UserInfo() { Id = user.Id, UserName = user.UserName, Email = user.Email, JoinedAt = user.JoinedAt, Role = user.Role };
    }

    public async Task UpsertUser(Guid id, string email, string userName, CancellationToken cancellationToken = default)
    {
        User? user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user == null)
        {
            logger.LogInformation("Creating new user with {email} and {userName}", email, userName);
            await CreateUser(id, email, userName, cancellationToken);
        }
        else
        {
            await UpdateUserName(user, userName, cancellationToken);
            await cache.RemoveAsync($"users/{user.Id}", cancellationToken);
        }
    }

    private async Task UpdateUserName(User user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateUser(Guid userId, string email, string userName, CancellationToken cancellationToken)
    {
        User user = new()
        {
            Id = userId,
            Email = email,
            UserName = userName,
            JoinedAt = DateTime.UtcNow,
            Role = Role.User
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }
}