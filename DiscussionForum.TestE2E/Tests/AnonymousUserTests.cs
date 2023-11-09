using DiscussionForum.TestE2E.Infrastructure;

namespace DiscussionForum.TestE2E.Tests;
public class AnonymousUserTests : BaseTest
{
    public AnonymousUserTests(WebApplicationFactoryFixture server) : base(server)
    {
    }

    [Fact]
    public async Task WhenNavigatingBetweenPages_UrlChanges()
    {
        await page.GetByRole(AriaRole.Link, new() { Name = "Next page" }).ClickAsync();
        page.Url.Should().EndWith("/1");
        await page.GetByRole(AriaRole.Link, new() { Name = "Next page" }).ClickAsync();
        page.Url.Should().EndWith("/2");
        await page.GetByRole(AriaRole.Link, new() { Name = "Last page" }).ClickAsync();
        page.Url.Should().EndWith("/7");
        await page.GetByRole(AriaRole.Link, new() { Name = "Previous page" }).ClickAsync();
        page.Url.Should().EndWith("/6");
        await page.GetByRole(AriaRole.Link, new() { Name = "Previous page" }).ClickAsync();
        page.Url.Should().EndWith("/5");
        await page.GetByRole(AriaRole.Link, new() { Name = "First page" }).ClickAsync();
        page.Url.Should().EndWith("/0");
    }
}
