using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.HttpLogging;

namespace DiscussionForum.Server.Installers;

public class LoggingInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        if (builder.Configuration.GetValue<bool>("AddLogging"))
        {
            builder.Logging.AddSimpleConsole(x =>
            {
                x.UseUtcTimestamp = true;
                x.TimestampFormat = "dd/MM/yy HH:mm:ss ";
            });
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Logging.AddApplicationInsights();
            builder.Services.AddApplicationInsightsTelemetryProcessor<IgnoreRequestPathsTelemetryProcessor>();
            builder.Services.AddHttpLogging(logging =>
            {
                logging.CombineLogs = true;
                logging.LoggingFields = HttpLoggingFields.Duration | HttpLoggingFields.RequestProperties | HttpLoggingFields.ResponsePropertiesAndHeaders;
            });
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
    private static readonly string[] _fileEndings = [".br", ".js", ".svg", ".png", ".css", ".json"];
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
