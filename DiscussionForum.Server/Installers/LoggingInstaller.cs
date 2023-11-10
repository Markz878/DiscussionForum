using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace DiscussionForum.Server.Installers;

public class LoggingInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        if (builder.Configuration.GetValue<bool>("AddLogging"))
        {
            builder.Logging.AddConsole();
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Logging.AddApplicationInsights();
            builder.Services.AddApplicationInsightsTelemetryProcessor<IgnoreRequestPathsTelemetryProcessor>();
        }
    }
}

public class IgnoreRequestPathsTelemetryProcessor : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;

    public IgnoreRequestPathsTelemetryProcessor(ITelemetryProcessor next)
    {
        _next = next;
    }

    public void Process(ITelemetry telemetry)
    {
        if (telemetry is RequestTelemetry requestTelemetry)
        {
            // Check if the request path is "/health"
            if (SkipTelemetry(requestTelemetry.Url.AbsolutePath))
            {
                return;
            }
        }
        _next.Process(telemetry);
    }

    private static readonly string[] _ignorePaths = ["/health", "/favicon.ico", "/topichub/negotiate"];
    private static readonly string[] _fileEndings = [".br", ".js", ".svg", ".png", ".css"];
    private static bool SkipTelemetry(string path)
    {
        if (_ignorePaths.Contains(path))
        {
            return true;
        }
        foreach (string fileEnding in _fileEndings)
        {
            if (path.EndsWith(fileEnding))
            {
                return true;
            }
        }
        return false;
    }
}
