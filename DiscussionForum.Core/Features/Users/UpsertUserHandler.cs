using Microsoft.Extensions.Caching.Distributed;

namespace DiscussionForum.Core.Features.Users;

internal sealed class UpsertUserHandler : IRequestHandler<UpsertUser>
{
    private readonly AppDbContext db;
    private readonly ILogger<UpsertUser> logger;
    private readonly IValidator<UpsertUser> validator;
    private readonly IDistributedCache cache;

    public UpsertUserHandler(AppDbContext db, ILogger<UpsertUser> logger, IValidator<UpsertUser> validator, IDistributedCache cache)
    {
        this.db = db;
        this.logger = logger;
        this.validator = validator;
        this.cache = cache;
    }

    /// <summary>
    /// Creates a new user or updates an existing user UserName property
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Handle(UpsertUser request, CancellationToken cancellationToken = default)
    {
        validator.ValidateAndThrow(request);
        User? user = await db.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            logger.LogInformation("Creating new user with {email} and {userName}", request.Email, request.UserName);
            await CreateUser(request, cancellationToken);
        }
        else
        {
            await UpdateUserName(request, user, cancellationToken);
            await cache.RemoveAsync($"users/{user.Id}", cancellationToken);
        }
    }

    private async Task UpdateUserName(UpsertUser request, User user, CancellationToken cancellationToken)
    {
        user.UserName = request.UserName;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateUser(UpsertUser request, CancellationToken cancellationToken)
    {
        User user = new()
        {
            Id = request.UserId,
            Email = request.Email,
            UserName = request.UserName,
            JoinedAt = DateTime.UtcNow,
            Role = Role.User
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }
}

