global using DiscussionForum.Core.FileService;
global using DiscussionForum.Core.HelperMethods;
global using DiscussionForum.Server.HelperMethods;
global using DiscussionForum.Server.Hubs;
global using DiscussionForum.Shared;
global using DiscussionForum.Shared.HelperMethods;
global using DiscussionForum.Shared.Interfaces;
global using DiscussionForum.Shared.Models.Errors;
global using MediatR;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Authorization;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.OpenApi.Models;
global using System.Security.Claims;
using DiscussionForum.Server.Endpoints;
using DiscussionForum.Server.Installers;
using DiscussionForum.Server.Pages;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.InstallAssemblyServices();
builder.Services.AddSingleton<RenderLocation, ServerRenderLocation>();
WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseHttpLogging();
app.AddDevelopmentAuthentication();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Discussion Forum API v1"));
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseOutputCache();
app.UseRateLimiter();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.MapAPIEndpoints();
app.MapHub<TopicHub>("/topichub", options => options.AllowStatefulReconnects = true);
app.MapHealthChecks("/health");
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(DiscussionForum.Client._Imports).Assembly);
app.Run();

namespace DiscussionForum.Server
{
    public partial class Program { }
}