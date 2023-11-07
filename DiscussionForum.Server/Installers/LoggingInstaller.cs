namespace DiscussionForum.Server.Installers;

public class LoggingInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        if (builder.Configuration.GetValue<bool>("AddLogging"))
        {
            if (builder.Environment.IsProduction())
            {
                builder.Services.AddApplicationInsightsTelemetry();
                builder.Logging.AddApplicationInsights();
            }
            builder.Logging.AddConsole();
        }
    }
}
