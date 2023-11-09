using DiscussionForum.Shared.DTO.Messages;
using System.Net.Http.Json;

namespace DiscussionForum.Tests.IntegrationTests.EndpointTests.Messages;
public class AuthorizedMessageTests : AuthorizedBaseTest
{
    private const string uri = "api/messages";
    private readonly ITestOutputHelper _testOutputHelper;

    public AuthorizedMessageTests(WebApplicationFactoryFixture factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
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
        AddMessageResponse? result = await response.Content.ReadFromJsonAsync<AddMessageResponse>();
        ArgumentNullException.ThrowIfNull(result);
        _testOutputHelper.WriteLine($"ATTACHED FILES IS NULL: {result.AttachedFiles == null}");
        _testOutputHelper.WriteLine($"ATTACHED FILES: {result.AttachedFiles?.Length}");
        //result.AttachedFiles.Should().NotBeEmpty();
        //result.AttachedFiles!.First().Name.Should().Be("file.txt");
        //result.AttachedFiles!.First().Id.Should().NotBeEmpty();
        AppDbContext db = GetDbContext();
        Message? message = await db.Messages.Include(x => x.Topic).FirstOrDefaultAsync(x => x.Id == result.Id);
        ArgumentNullException.ThrowIfNull(message);
        message.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
        message.Content.Should().Be("Test message");
        message.Topic.Should().NotBeNull();
        message.Topic!.Id.Should().Be(2);
        message.Topic!.LastMessageTimeStamp.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task AddMessage_WhenTopicIdNotParseable_ReturnsValidationProblems()
    {
        MultipartFormDataContent formData = new()
        {
            { new StringContent(""), "topicid" },
            { new StringContent("Test message"), "message" }
        };
        HttpResponseMessage response = await client.PostAsync(uri, formData);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Dictionary<string, string[]> errors = await response.ToProblemDetailsDictionary();
        errors.Should().ContainKey("TopicId");
    }


    [Fact]
    public async Task AddMessage_WhenMessageTooLongAndTooManyFiles_ReturnsValidationProblems()
    {
        MultipartFormDataContent formData = new()
        {
            { new StringContent("10"), "topicid" },
            { new StringContent(new string('a', ValidationConstants.MessageContentMaxLength+1)), "message" }
        };
        for (int i = 0; i < ValidationConstants.MessageMaxFiles + 1; i++)
        {
            formData.Add(new StreamContent(new MemoryStream(Convert.FromBase64String("VGVzdCB0ZXh0"))), $"file{i}.txt", $"file{i}.txt");
        }
        HttpResponseMessage response = await client.PostAsync(uri, formData);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Dictionary<string, string[]> errors = await response.ToProblemDetailsDictionary();
        errors.Should().ContainKey("Message");
        errors.Should().ContainKey("AttachedFiles");
    }

    [Fact]
    public async Task DeleteMessage()
    {
        MultipartFormDataContent formData = new()
        {
            { new StringContent("2"), "topicid" },
            { new StringContent("Test message"), "message" },
        };
        HttpResponseMessage response = await client.PostAsync(uri, formData);
        AddMessageResponse? result = await response.Content.ReadFromJsonAsync<AddMessageResponse>();
        ArgumentNullException.ThrowIfNull(result);
        response = await client.DeleteAsync($"{uri}/{result.Id}");
        response.EnsureSuccessStatusCode();
        AppDbContext db = GetDbContext();
        Message? message = await db.Messages.FirstOrDefaultAsync(x => x.Id == result.Id);
        message.Should().BeNull();
        response = await client.DeleteAsync($"{uri}/{result.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateMessage()
    {
        MultipartFormDataContent formData = new()
        {
            { new StringContent("2"), "topicid" },
            { new StringContent("Test message"), "message" },
        };
        HttpResponseMessage response = await client.PostAsync(uri, formData);
        AddMessageResponse? result = await response.Content.ReadFromJsonAsync<AddMessageResponse>();
        ArgumentNullException.ThrowIfNull(result);
        EditMessageRequest editMessage = new() { MessageId = result.Id, Message = "Edited message" };
        response = await client.PatchAsJsonAsync(uri, editMessage);
        response.EnsureSuccessStatusCode();
        AppDbContext db = GetDbContext();
        Message? message = await db.Messages.FirstOrDefaultAsync(x => x.Id == result.Id);
        ArgumentNullException.ThrowIfNull(message);
        message.Content.Should().Be("Edited message");
    }
}
