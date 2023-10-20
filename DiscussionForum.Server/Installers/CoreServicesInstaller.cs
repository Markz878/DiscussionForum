namespace DiscussionForum.Server.Installers;

public class CoreServicesInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Services.RegisterCoreServices(builder.Configuration, builder.Environment.IsDevelopment());
        builder.Services.AddOptions<FileStorageSettings>().BindConfiguration(nameof(FileStorageSettings)).ValidateDataAnnotations().ValidateOnStart();
    }
}
