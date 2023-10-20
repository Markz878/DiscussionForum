using DiscussionForum.Shared.Models.Topics;
using System.Net.Http.Json;

namespace DiscussionForum.Tests.IntegrationTests.EndpointTests.Topics;

public class AuthorizedListTopicsTests : AuthorizedBaseTest
{
    private const string uri = "api/topics";
    public AuthorizedListTopicsTests(WebApplicationFactoryFixture factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task ListLatestTopics()
    {
        ListLatestTopicsResult? response = await client.GetFromJsonAsync<ListLatestTopicsResult>(uri + "/latest/0");
        ArgumentNullException.ThrowIfNull(response);
        response.Topics.Should().NotBeEmpty().And.HaveCount(10);
        response.PageCount.Should().BeCloseTo(8, 2);
    }

    [Fact]
    public async Task ListLatestTopicsWithSearch()
    {
        ListLatestTopicsResult? response = await client.GetFromJsonAsync<ListLatestTopicsResult>(uri + "/latest/0?search=product");
        ArgumentNullException.ThrowIfNull(response);
        response.Topics.Should().AllSatisfy(topic => topic.Title.ToLower().Should().Contain("product"));
        response.PageCount.Should().BeLessThan(7);
    }

    [Fact]
    public async Task GetTopic()
    {
        GetTopicByIdResult? response = await client.GetFromJsonAsync<GetTopicByIdResult>(uri + "/1");
        ArgumentNullException.ThrowIfNull(response);
        response.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddYears(-1));
        response.Title.Should().NotBeNullOrWhiteSpace();
        response.UserName.Should().NotBeNullOrWhiteSpace();
        response.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddYears(-1));
        response.Messages.Should().NotBeEmpty();
        response.Messages.Should().AllSatisfy(message =>
        {
            message.Content.Should().NotBeNullOrWhiteSpace();
            message.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddYears(-1));
            message.UserName.Should().NotBeNullOrWhiteSpace();
            message.LikesCount.Should().BeGreaterOrEqualTo(0);
        });
    }

    [Fact]
    public async Task CreateTopic()
    {
        MultipartFormDataContent formData = new()
        {
            { new StringContent("Test title"), "title" },
            { new StringContent("This is the message content"), "firstMessage" },
            { new StreamContent(new MemoryStream(Convert.FromBase64String("VGVzdCB0ZXh0"))), "file.txt", "file.txt" }
        };
        HttpResponseMessage response = await client.PostAsync(uri, formData);
        AddTopicResult? result = await response.Content.ReadFromJsonAsync<AddTopicResult>();
        ArgumentNullException.ThrowIfNull(result);
        AppDbContext db = GetDbContext();
        Topic? topic = await db.Topics.Include(x => x.Messages).FirstOrDefaultAsync(x => x.Id == result.Id);
        ArgumentNullException.ThrowIfNull(topic);
        topic.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
        topic.LastMessageTimeStamp.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
        topic.Title.Should().Be("Test title");
        topic.Messages.Should().ContainSingle();
        topic.Id.Should().Be(result.Id);
        topic.UserId.Should().Be(UserId);
    }

    [Fact]
    public async Task DeleteTopic()
    {
        MultipartFormDataContent formData = new()
        {
            { new StringContent("Test title"), "title" },
            { new StringContent("This is the message content"), "firstMessage" },
        };
        HttpResponseMessage response = await client.PostAsync(uri, formData);
        AddTopicResult? result = await response.Content.ReadFromJsonAsync<AddTopicResult>();
        ArgumentNullException.ThrowIfNull(result);
        response = await client.DeleteAsync(uri + "/" + result.Id);
        AppDbContext db = GetDbContext();
        Topic? topic = await db.Topics.FirstOrDefaultAsync(x => x.Id == result.Id);
        topic.Should().BeNull();
    }

    [Fact]
    public async Task EditTopicTitle()
    {
        MultipartFormDataContent formData = new()
        {
            { new StringContent("Test title"), "title" },
            { new StringContent("This is the message content"), "firstMessage" },
        };
        HttpResponseMessage response = await client.PostAsync(uri, formData);
        AddTopicResult? result = await response.Content.ReadFromJsonAsync<AddTopicResult>();
        ArgumentNullException.ThrowIfNull(result);
        EditTopicTitle editRequest = new() { NewTitle = "Edited title", TopicId = result.Id };
        response = await client.PatchAsJsonAsync(uri, editRequest);
        response.EnsureSuccessStatusCode();
        AppDbContext db = GetDbContext();
        Topic? topic = await db.Topics.FirstOrDefaultAsync(x => x.Id == result.Id);
        ArgumentNullException.ThrowIfNull(topic);
        topic.Title.Should().Be("Edited title");
    }
}
