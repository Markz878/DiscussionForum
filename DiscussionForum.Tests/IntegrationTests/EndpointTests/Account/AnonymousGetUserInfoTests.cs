namespace DiscussionForum.Tests.IntegrationTests.EndpointTests.Account;

public sealed class AccountEndpoints : BaseTest
{
    private const string uri = "api/account/user";

    public AccountEndpoints(WebApplicationFactoryFixture factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task NotLoggedIn_ReturnUnauthorized()
    {
        HttpResponseMessage response = await client.GetAsync(uri);
        string body = await response.Content.ReadAsStringAsync();
        UserAuthInfo? user = JsonSerializer.Deserialize<UserAuthInfo>(body, jsonOptions);
        ArgumentNullException.ThrowIfNull(user);
        user.IsAuthenticated.Should().BeFalse();
        Assert.Empty(user.Claims);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
