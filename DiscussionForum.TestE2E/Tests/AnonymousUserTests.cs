using DiscussionForum.TestE2E.Infrastructure;

namespace DiscussionForum.TestE2E.Tests;
public class AnonymousUserTests(WebApplicationFactoryFixture server) : BaseTest(server)
{
    [Fact]
    public async Task WhenNavigatingBetweenPages_UrlChanges()
    {
        await page.GetByRole(AriaRole.Link, new() { Name = "Next page" }).ClickAsync();
        page.Url.Should().EndWith("/1#title");
        await page.GetByRole(AriaRole.Link, new() { Name = "Next page" }).ClickAsync();
        page.Url.Should().EndWith("/2#title");
        await page.GetByRole(AriaRole.Link, new() { Name = "Last page" }).ClickAsync();
        page.Url.Should().EndWith("/7#title");
        await page.GetByRole(AriaRole.Link, new() { Name = "Previous page" }).ClickAsync();
        page.Url.Should().EndWith("/6#title");
        await page.GetByRole(AriaRole.Link, new() { Name = "Previous page" }).ClickAsync();
        page.Url.Should().EndWith("/5#title");
        await page.GetByRole(AriaRole.Link, new() { Name = "First page" }).ClickAsync();
        page.Url.Should().EndWith("/0#title");
    }
}
