using DiscussionForum.Core.HelperMethods;

namespace DiscussionForum.Tests.IntegrationTests.Infrastructure;
public abstract class AuthorizedBaseTest : BaseTest
{
    protected AuthorizedBaseTest(WebApplicationFactoryFixture factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
    }

    protected override HttpClient GetHttpClient()
    {
        HttpClient client = base.GetHttpClient();
        client.DefaultRequestHeaders.Add(EasyAuthAuthenticationHandler.EasyAuthPrincipalIDP, "aad");
        client.DefaultRequestHeaders.Add(EasyAuthAuthenticationHandler.EasyAuthPrincipalID, Fakers.User.Id.ToString());
        client.DefaultRequestHeaders.Add(EasyAuthAuthenticationHandler.EasyAuthPrincipalName, Fakers.User.Email);
        return client;
    }
}
