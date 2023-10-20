using DiscussionForum.Core.HelperMethods;
using DiscussionForum.TestE2E.Infrastructure;
using System.Text.RegularExpressions;

namespace DiscussionForum.TestE2E.Tests;
public class AuthorizedBaseTests : BaseTest
{
    public AuthorizedBaseTests(WebApplicationFactoryFixture server) : base(server)
    {
    }

    public override async Task InitializeAsync()
    {
        browserContext = await server.GetNewBrowserContext(Fakers.User);
        page = await browserContext.GotoPage(server.BaseUrl, true);
    }

    [Fact]
    public async Task WhenCreateNewTopic_RedirectedToTheTopic_AndCanInteractAndDeleteTopic()
    {
        await page.GetByRole(AriaRole.Link, new() { Name = "Create new topic" }).ClickAsync();

        await page.GetByLabel("Title:").FillAsync("Test");

        await page.GetByLabel("Content:").FillAsync("Content");

        await page.Locator("input[type=\"file\"]").SetInputFilesAsync(new FilePayload() { Buffer = Convert.FromBase64String("VGVzdCB0ZXh0"), MimeType = "text/plain", Name = "Test.txt" });

        await page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

        await Task.Delay(2000); // Wait for WASM

        await page.GetByPlaceholder("New message").FillAsync("More content");

        await page.GetByRole(AriaRole.Button, new() { Name = "Send" }).ClickAsync();

        await page.GetByLabel("upvote comment").Nth(1).ClickAsync();
        await page.Locator("div").Filter(new() { HasTextRegex = new Regex("^1 like$") }).GetByAltText("upvote").WaitForAsync();

        await page.GetByLabel("upvote comment").Nth(1).ClickAsync();
        await page.Locator("div").Filter(new() { HasTextRegex = new Regex("^0 likes$") }).Nth(1).GetByAltText("upvote").WaitForAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "edit" }).Nth(1).ClickAsync();
        await page.Locator("form").Filter(new() { HasText = "Submit Cancel" }).GetByRole(AriaRole.Textbox).FillAsync("Let me edit more content");

        await page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();
        await page.GetByText("Let me edit more content").WaitForAsync();
        int zeroLikesCount = await page.Locator("div").Filter(new() { HasTextRegex = new Regex("^0 likes$") }).CountAsync();
        zeroLikesCount.Should().Be(2);

        await page.GetByRole(AriaRole.Button, new() { Name = "edit" }).Nth(1).ClickAsync();

        await page.Locator("form").Filter(new() { HasText = "Submit Cancel" }).GetByRole(AriaRole.Textbox).FillAsync("Let me edit more content again before cancel");

        await page.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
        await page.GetByText("Let me edit more content").WaitForAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "delete" }).Nth(1).ClickAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Edit title" }).ClickAsync();

        await page.Locator("form").Filter(new() { HasText = "Submit Cancel" }).GetByRole(AriaRole.Textbox).ClickAsync();

        await page.Locator("form").Filter(new() { HasText = "Submit Cancel" }).GetByRole(AriaRole.Textbox).FillAsync("Test topic");

        await page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();
        await page.GetByText("Test topic").WaitForAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Edit title" }).ClickAsync();

        await page.Locator("form").Filter(new() { HasText = "Submit Cancel" }).GetByRole(AriaRole.Textbox).FillAsync("Test topic edited");

        await page.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
        await page.GetByText("Test topic").WaitForAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Delete topic" }).ClickAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

        await page.WaitForURLAsync(server.BaseUrl + "/");
    }
}
