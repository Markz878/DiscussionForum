namespace DiscussionForum.TestE2E.Infrastructure;

[Collection(nameof(WebApplicationFactoryCollection))]
public abstract class BaseTest : IAsyncLifetime
{
    protected readonly WebApplicationFactoryFixture server;
    protected IBrowserContext browserContext = default!;
    protected IPage page = default!;

    public BaseTest(WebApplicationFactoryFixture server)
    {
        server.CreateDefaultClient();
        this.server = server;
    }

    public virtual async Task InitializeAsync()
    {
        browserContext = await server.GetNewBrowserContext();
        page = await browserContext.GotoPage(server.BaseUrl);
        await Task.Delay(1000);
    }

    public async Task DisposeAsync()
    {
        await browserContext.DisposeAsync();
    }
}
