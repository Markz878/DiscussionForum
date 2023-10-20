namespace DiscussionForum.TestE2E.Infrastructure;

internal static class PlaywrightHelpers
{
    internal static async Task<IBrowserContext> GetNewBrowserContext(this WebApplicationFactoryFixture factory, User? fakeAuth = null)
    {
        ArgumentNullException.ThrowIfNull(factory.BrowserInstance);
        IBrowserContext browserContext = await factory.BrowserInstance.NewContextAsync(new BrowserNewContextOptions()
        {
            IgnoreHTTPSErrors = true,
        });
        if (fakeAuth != null)
        {
            await browserContext.SetExtraHTTPHeadersAsync(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(EasyAuthAuthenticationHandler.EasyAuthPrincipalIDP, "aad"),
                new KeyValuePair<string, string>(EasyAuthAuthenticationHandler.EasyAuthPrincipalID, fakeAuth.Id.ToString()),
                new KeyValuePair<string, string>(EasyAuthAuthenticationHandler.EasyAuthPrincipalName, fakeAuth.Email)
            });
        }
        return browserContext;
    }

    internal static async Task<IPage> GotoPage(this IBrowserContext browserContext, string url, bool checkIfAuthenticated = false)
    {
        IPage page = await browserContext.NewPageAsync();
        await page.GotoAsync(url);
        await page.WaitForSelectorAsync("a:has-text('Discussion Forum')");
        if (checkIfAuthenticated)
        {
            await page.GetByText("TE", new() { Exact = true }).WaitForAsync();
        }
        else
        {
            await page.WaitForSelectorAsync("a:has-text('Log In')");
        }
        await Task.Delay(500);
        return page;
    }
}
