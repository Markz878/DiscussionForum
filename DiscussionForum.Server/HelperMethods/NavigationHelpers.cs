namespace DiscussionForum.Server.HelperMethods;

public static class NavigationHelpers
{
    public static void NavigateToSecure(this NavigationManager navigationManager, string path)
    {
        string url = navigationManager.BaseUri;
        url = url.Replace("http://", "https://");
        string finalUrl = url + path[1..];
        navigationManager.NavigateTo(finalUrl);
    }
}
