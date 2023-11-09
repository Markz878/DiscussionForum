using DiscussionForum.Core.HelperMethods;

namespace DiscussionForum.Tests.IntegrationTests.Infrastructure;

[Collection(nameof(WebApplicationFactoryFixture))]
public abstract class BaseTest : IDisposable
{
    protected readonly WebApplicationFactoryFixture factory;
    protected readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);
    protected readonly HttpClient client;
    protected Guid UserId = Fakers.User.Id;
    protected Guid AdminId = Fakers.Admin.Id;
    protected IServiceScope scope;
    private readonly WebApplicationFactoryClientOptions NoRedirectClientOptions = new()
    {
        AllowAutoRedirect = false
    };
    public BaseTest(WebApplicationFactoryFixture factory, ITestOutputHelper testOutputHelper)
    {
        this.factory = factory;
        factory.TestOutputHelper ??= testOutputHelper;
        client = GetHttpClient();
        scope = factory.Services.CreateScope();
    }

    protected virtual HttpClient GetHttpClient()
    {
        return factory.CreateClient(NoRedirectClientOptions);
    }

    internal AppDbContext GetDbContext()
    {
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public void Dispose()
    {
        scope.Dispose();
        GC.SuppressFinalize(this);
    }
}