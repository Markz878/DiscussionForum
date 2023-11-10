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
            if (requestTelemetry.Url.AbsolutePath.Equals("/health", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }
        _next.Process(telemetry);
    }
}
