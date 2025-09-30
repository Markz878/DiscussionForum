namespace DiscussionForum.Server.Installers;

public class ValidationInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Services.AddValidation();
    }
}
