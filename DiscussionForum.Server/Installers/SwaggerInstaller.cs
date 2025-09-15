using DiscussionForum.Server.Filters;

namespace DiscussionForum.Server.Installers;

public sealed class SwaggerInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
    }
}
