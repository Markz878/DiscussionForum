namespace DiscussionForum.Server.Installers;

public class CacheInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddMemoryCache();
    }
}
