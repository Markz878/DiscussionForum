using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DiscussionForum.Server.Installers;

public sealed class HealthChecksInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Services
            .AddHealthChecks()
            .AddSqlServer(builder.Configuration.GetConnectionString("SqlServer") ?? throw new ArgumentNullException("SqlServer connection string"))
            .AddCheck<FileStorageHealthCheck>("FileStorageHealthCheck", HealthStatus.Degraded);
    }
}

public sealed class FileStorageHealthCheck : IHealthCheck
{
    private readonly IFileService _fileService;

    public FileStorageHealthCheck(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            bool healthy = await _fileService.CheckHealth(cancellationToken);
            if (healthy)
            {
                return HealthCheckResult.Healthy();
            }
            return HealthCheckResult.Degraded();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
