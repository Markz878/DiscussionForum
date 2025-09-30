using DiscussionForum.Core.HelperMethods;
using Microsoft.Extensions.Hosting;
using System.Net.Sockets;

namespace DiscussionForum.TestE2E.Infrastructure;

public sealed class WebApplicationFactoryFixture : WebApplicationFactory<Server.Program>, IAsyncLifetime
{
    public ITestOutputHelper? TestOutputHelper { get; set; }
    public IPlaywright? PlaywrightInstance { get; private set; }
    public IBrowser? BrowserInstance { get; private set; }
    public string BaseUrl { get; } = $"https://localhost:7777";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(BaseUrl);

        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureServices((context, services) =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = "Data Source=127.0.0.1,1433;Initial Catalog=DiscussionForumE2ETests;User ID=sa;Password=yourStrong(!)Password;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;";
                options.UseAzureSql(connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
        });
    }

    public async Task InitializeAsync()
    {
        UseKestrel(x => x.ListenLocalhost(7777, o => o.UseHttps()));
        StartServer();
        DataSeeder.SeedData(Services);
        DataSeeder.CreateStorageContainer(Services);
        PlaywrightInstance = await Playwright.CreateAsync();
        BrowserInstance = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            SlowMo = 400,
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (BrowserInstance is not null && PlaywrightInstance is not null)
        {
            await BrowserInstance.DisposeAsync();
            PlaywrightInstance.Dispose();
        }
    }
}
