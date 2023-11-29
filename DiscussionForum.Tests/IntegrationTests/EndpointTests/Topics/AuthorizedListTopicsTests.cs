using DiscussionForum.Shared.DTO.Topics;
using System.Net.Http.Json;

namespace DiscussionForum.Tests.IntegrationTests.EndpointTests.Topics;

public class AuthorizedListTopicsTests : AuthorizedBaseTest
{
    private const string uri = "api/topics";
    public AuthorizedListTopicsTests(WebApplicationFactoryFixture factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task DeleteTopic()
    {
        HttpResponseMessage response = await client.DeleteAsync(uri + "/" + 50);
        AppDbContext db = GetDbContext();
        Topic? topic = await db.Topics.FirstOrDefaultAsync(x => x.Id == 50);
        topic.Should().BeNull();
    }

    [Fact]
    public async Task EditTopicTitle()
    {
        EditTopicTitleRequest editRequest = new() { NewTitle = "Edited title", TopicId = 60 };
        HttpResponseMessage response = await client.PatchAsJsonAsync(uri, editRequest);
        response.EnsureSuccessStatusCode();
        AppDbContext db = GetDbContext();
        Topic? topic = await db.Topics.FirstOrDefaultAsync(x => x.Id == 60);
        ArgumentNullException.ThrowIfNull(topic);
        topic.Title.Should().Be("Edited title");
    }
}
