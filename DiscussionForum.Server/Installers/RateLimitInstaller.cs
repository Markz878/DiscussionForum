using System.Threading.RateLimiting;

namespace DiscussionForum.Server.Installers;

public sealed class RateLimitInstaller : IInstaller
{
    public const string PolicyName = "RateLimitPolicy";
    public void Install(WebApplicationBuilder builder)
    {
        RateLimitOptions rateLimitOptions = new();
        builder.Configuration.GetSection(nameof(RateLimitOptions)).Bind(rateLimitOptions);
        builder.Services.AddRateLimiter(opt =>
        {
            opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opt.AddPolicy(PolicyName, httpContext =>
            {
                Guid? userId = httpContext.User.TryGetUserId();
                if (userId == null)
                {
                    return RateLimitPartition.GetTokenBucketLimiter(Guid.Empty, _ =>
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = rateLimitOptions.AnonTokenLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = rateLimitOptions.AnonQueueLimit,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitOptions.AnonReplenishmentPeriod),
                            TokensPerPeriod = rateLimitOptions.AnonTokensPerPeriod,
                            AutoReplenishment = false
                        });
                }
                else
                {
                    return RateLimitPartition.GetTokenBucketLimiter(userId.Value, _ =>
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = rateLimitOptions.UserTokenLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = rateLimitOptions.UserQueueLimit,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitOptions.UserReplenishmentPeriod),
                            TokensPerPeriod = rateLimitOptions.UserTokensPerPeriod,
                            AutoReplenishment = false
                        });
                }
            });
        });
    }
}

internal sealed class RateLimitOptions
{
    public int AnonTokenLimit { get; set; }
    public int AnonQueueLimit { get; set; }
    public int AnonReplenishmentPeriod { get; set; }
    public int AnonTokensPerPeriod { get; set; }
    public int UserTokenLimit { get; set; }
    public int UserQueueLimit { get; set; }
    public int UserReplenishmentPeriod { get; set; }
    public int UserTokensPerPeriod { get; set; }
}
