namespace DiscussionForum.Server.Installers;

public class CacheInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddMemoryCache();
        builder.Services.AddOutputCache(options =>
            options.AddBasePolicy(builder =>
                builder.Cache().Expire(TimeSpan.FromSeconds(10))));
    }
}
