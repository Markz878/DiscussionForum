using DiscussionForum.Core.HelperMethods;

namespace DiscussionForum.Tests.IntegrationTests.Infrastructure;

public sealed class WebApplicationFactoryFixture : WebApplicationFactory<Server.Program>, IAsyncLifetime
{
    public ITestOutputHelper? TestOutputHelper { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(configureLogging: logging =>
        {
            logging.ClearProviders();
            logging.AddProvider(new XUnitLoggingProvider(TestOutputHelper));
        });

        builder.ConfigureAppConfiguration((context, config) => config.AddInMemoryCollection(new Dictionary<string, string?>() { { "SeedDatabase", "true" } }));

        builder.ConfigureServices((context, services) =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = "Data Source=localhost,5000;Initial Catalog=DiscussionForumIntegrationTests;User ID=sa;Password=yourStrong(!)Password;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;";
                options.UseSqlServer(connectionString, options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                options.EnableThreadSafetyChecks(false);
            });
        });
    }

    public async Task InitializeAsync()
    {
        DataSeeder.SeedData(Services);
        await Task.Delay(5000);
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
