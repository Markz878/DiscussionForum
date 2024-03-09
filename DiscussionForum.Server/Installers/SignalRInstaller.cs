using Microsoft.AspNetCore.ResponseCompression;

namespace DiscussionForum.Server.Installers;

public class SignalRInstaller : IInstaller
{
    private const string octetStream = "application/octet-stream";
    public void Install(WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSignalR(o => o.EnableDetailedErrors = true).AddMessagePackProtocol();
        }
        else
        {
            builder.Services.AddSignalR().AddMessagePackProtocol().AddAzureSignalR();
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Append(octetStream);
            });
        }
    }
}
