namespace DiscussionForum.Server.Installers;

public class LoggingInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        if (builder.Environment.IsProduction())
        {
            builder.Services.AddApplicationInsightsTelemetry();
        }
        if (builder.Configuration.GetValue<bool>("AddLogging"))
        {
            builder.Host.UseSerilog((context, services, configuration)
                => configuration.ReadFrom.Configuration(context.Configuration));
        }
    }
}
