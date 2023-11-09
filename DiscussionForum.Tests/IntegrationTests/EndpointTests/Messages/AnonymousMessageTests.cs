namespace DiscussionForum.Tests.IntegrationTests.EndpointTests.Messages;
public class AnonymousMessageTests : BaseTest
{
    private const string uri = "api/messages";

    public AnonymousMessageTests(WebApplicationFactoryFixture factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task AddMessage()
    {
        MultipartFormDataContent formData = new()
        {
            { new StringContent("2"), "topicid" },
            { new StringContent("Test message"), "message" },
            { new StreamContent(new MemoryStream(Convert.FromBase64String("VGVzdCB0ZXh0"))), "file.txt", "file.txt" }
        };
        HttpResponseMessage response = await client.PostAsync(uri, formData);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteMessage()
    {
        HttpResponseMessage response = await client.DeleteAsync($"{uri}/{5}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateMessage()
    {
        MultipartFormDataContent formData = new()
        {
            { new StringContent("3"), "topicid" },
            { new StringContent("Test message"), "message" },
        };
        HttpResponseMessage response = await client.PostAsync(uri, formData);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
