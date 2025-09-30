global using DiscussionForum.Client.Authentication;
global using DiscussionForum.Shared;
global using DiscussionForum.Shared.DTO.Messages;
global using DiscussionForum.Shared.DTO.Topics;
global using DiscussionForum.Shared.DTO.Users;
global using DiscussionForum.Shared.HelperMethods;
global using DiscussionForum.Shared.Interfaces;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Authorization;
global using Microsoft.AspNetCore.Components.Forms;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using System.ComponentModel.DataAnnotations;
global using System.Net.Http.Json;
using DiscussionForum.Client.Services;
using System.Net.Http.Headers;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();
builder.Services.AddHttpClient("Client", config =>
    {
        config.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
        config.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
    })
    .AddStandardResilienceHandler();
builder.Services.AddScoped<RenderLocation, ClientRenderLocation>();
builder.Services.AddScoped<ITopicsService, TopicsClientService>();
builder.Services.AddScoped<IMessagesService, MessagesClientService>();
builder.Services.AddScoped<IMessageLikesService, MessageLikesClientService>();
await builder.Build().RunAsync();
