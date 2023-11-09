using MessagePack;
using Microsoft.AspNetCore.ResponseCompression;

namespace DiscussionForum.Server.Installers;

public class SignalRInstaller : IInstaller
{
    public void Install(WebApplicationBuilder builder)
    {
        //if (builder.Environment.IsDevelopment())
        //{
            builder.Services.AddSignalR(o => o.EnableDetailedErrors = true)
                .AddMessagePackProtocol();
        //}
        //else
        //{
        //    builder.Services.AddSignalR().AddMessagePackProtocol().AddAzureSignalR();
        //}
    }
}
