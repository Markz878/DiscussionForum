namespace DiscussionForum.Server.Components.Layout;

[Authorize]
public sealed partial class ProfileButton
{
    [CascadingParameter] public required Task<AuthenticationState> AuthenticationStateTask { get; init; }
    [Inject] public required NavigationManager Navigation { get; init; }

    private string? _email;
    private string? _userName;
    private Role? role; 

    protected override async Task OnInitializedAsync()
    {
        UserInfo userInfo = await AuthenticationStateTask.GetUserInfo();
        _email = userInfo.GetUserEmail();
        _userName = userInfo.GetUserName();
        role = userInfo.GetUserRole();
    }

    public string GetUserInitials()
    {
        if (string.IsNullOrWhiteSpace(_userName) is false)
        {
            return _userName[0..2].ToUpperInvariant();
        }
        if (string.IsNullOrWhiteSpace(_email) is false)
        {
            return _email[0..2].ToUpperInvariant();
        }
        throw new ForbiddenException();
    }
}