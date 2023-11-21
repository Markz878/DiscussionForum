using DiscussionForum.Shared.DTO.Users;
using Microsoft.Extensions.Caching.Distributed;

namespace DiscussionForum.Core.Features.Users;

public sealed record UpsertUserCommand : IRequest
{
    public Guid UserId { get; set; }
    public required string Email { get; init; }
    public required string UserName { get; init; }
}

public sealed class UpsertUserValidator : AbstractValidator<UpsertUserCommand>
{
    public UpsertUserValidator()
    {
        RuleFor(x => x.UserName).MinimumLength(1).MaximumLength(ValidationConstants.UserNameMaxLength);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(ValidationConstants.UserEmailMaxLength);
    }
}

internal sealed class UpsertUserCommandHandler : IRequestHandler<UpsertUserCommand>
{
    private readonly AppDbContext db;
    private readonly ILogger<UpsertUserCommand> logger;
    private readonly IDistributedCache cache;

    public UpsertUserCommandHandler(AppDbContext db, ILogger<UpsertUserCommand> logger, IDistributedCache cache)
    {
        this.db = db;
        this.logger = logger;
        this.cache = cache;
    }

    /// <summary>
    /// Creates a new user or updates an existing user UserName property
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Handle(UpsertUserCommand request, CancellationToken cancellationToken = default)
    {
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

    private async Task UpdateUserName(UpsertUserCommand request, User user, CancellationToken cancellationToken)
    {
        user.UserName = request.UserName;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateUser(UpsertUserCommand request, CancellationToken cancellationToken)
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

