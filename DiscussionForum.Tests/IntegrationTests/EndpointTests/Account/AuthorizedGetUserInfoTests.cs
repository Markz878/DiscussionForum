namespace DiscussionForum.Tests.IntegrationTests.EndpointTests.Account;

public class AuthorizedGetUserInfoTests : AuthorizedBaseTest
{
    private const string uri = "api/account/user";

    public AuthorizedGetUserInfoTests(WebApplicationFactoryFixture factory, ITestOutputHelper testOutputHelper)
               : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task LoggedIn_ReturnUserInfo()
    {
        HttpResponseMessage response = await client.GetAsync(uri);
        string body = await response.Content.ReadAsStringAsync();
        UserInfo? user = JsonSerializer.Deserialize<UserInfo>(body, jsonOptions);
        ArgumentNullException.ThrowIfNull(user);
        user.IsAuthenticated.Should().BeTrue();
        user.Claims.Should().Contain(x => x.Type == ClaimConstants.IdClaimName);
        user.Claims.Should().Contain(x => x.Type == ClaimConstants.EmailNameClaimName);
        user.Claims.Should().Contain(x => x.Type == ClaimConstants.UserNameClaimName);
        user.Claims.Should().Contain(x => x.Type == ClaimConstants.RoleClaimName);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}