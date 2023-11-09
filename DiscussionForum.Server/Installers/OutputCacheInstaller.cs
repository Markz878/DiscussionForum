namespace DiscussionForum.Server.Installers;

public class OutputCacheInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Services.AddOutputCache(options =>
            options.AddBasePolicy(builder =>
                builder.Expire(TimeSpan.FromSeconds(10))));
    }
}
