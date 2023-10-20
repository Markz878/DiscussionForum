using DiscussionForum.Core.HelperMethods;

namespace DiscussionForum.Tests.IntegrationTests.Infrastructure;
public abstract class AdminBaseTest : BaseTest
{
    protected AdminBaseTest(WebApplicationFactoryFixture factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
    }

    protected override HttpClient GetHttpClient()
    {
        HttpClient client = base.GetHttpClient();
        client.DefaultRequestHeaders.Add(EasyAuthAuthenticationHandler.EasyAuthPrincipalIDP, "aad");
        client.DefaultRequestHeaders.Add(EasyAuthAuthenticationHandler.EasyAuthPrincipalID, Fakers.Admin.Id.ToString());
        client.DefaultRequestHeaders.Add(EasyAuthAuthenticationHandler.EasyAuthPrincipalName, Fakers.Admin.Email);
        return client;
    }
}
