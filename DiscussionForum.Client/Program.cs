global using DiscussionForum.Client.Authentication;
global using DiscussionForum.Client.Handlers.MessageLikes;
global using DiscussionForum.Client.Handlers.Messages;
global using DiscussionForum.Client.Handlers.Topics;
global using DiscussionForum.Shared;
global using DiscussionForum.Shared.DTO.Messages;
global using DiscussionForum.Shared.DTO.Topics;
global using DiscussionForum.Shared.DTO.Users;
global using DiscussionForum.Shared.HelperMethods;
global using DiscussionForum.Shared.Interfaces;
global using MediatR;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Authorization;
global using Microsoft.AspNetCore.Components.Forms;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using System.ComponentModel.DataAnnotations;
global using System.Net.Http.Json;
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
builder.Services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddScoped<RenderLocation, ClientRenderLocation>();
await builder.Build().RunAsync();
