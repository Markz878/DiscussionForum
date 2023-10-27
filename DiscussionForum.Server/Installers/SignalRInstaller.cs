using Microsoft.AspNetCore.ResponseCompression;

namespace DiscussionForum.Server.Installers;

public class SignalRInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        builder.Services.AddResponseCompression(opts =>
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSignalR().AddMessagePackProtocol();
        }
        else
        {
            builder.Services.AddSignalR().AddMessagePackProtocol().AddAzureSignalR();
        }
    }
}
