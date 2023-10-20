using System.ComponentModel.DataAnnotations;

namespace DiscussionForum.Server.Components.Pages;

[Authorize]
public partial class SetUserName
{
    [Inject] public required IMediator Mediator { get; init; }
    [Inject] public required NavigationManager Navigation { get; init; }
    [CascadingParameter] public required Task<AuthenticationState> AuthenticationStateTask { get; init; }
    [SupplyParameterFromForm] public CreateUserModel? CreateUserModel { get; set; }

    protected string? _errorMessage;
    private UserInfo? _userInfo;

    protected override void OnInitialized()
    {
        CreateUserModel ??= new();
        _userInfo = AuthenticationStateTask.GetUserInfo().Result;
        CreateUserModel.UserName = _userInfo?.GetUserName() ?? "";
    }

    protected async Task SubmitUserName()
    {
        try
        {
            if (_userInfo?.IsAuthenticated == true && CreateUserModel is not null && string.IsNullOrEmpty(CreateUserModel.UserName) is false)
            {
                await Mediator.Send(new UpsertUser() { UserId = _userInfo.GetUserId(), Email = _userInfo.GetUserEmail(), UserName = CreateUserModel.UserName });
                Navigation.NavigateTo("/", true);
            }
        }
        catch (UserNameTakenException)
        {
            _errorMessage = "Username is already taken, try another one.";
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
    }
}

public class CreateUserModel
{
    [Required]
    [MaxLength(ValidationConstants.UserNameMaxLength, ErrorMessage = "Username can't be over 50 characters long")]
    public string UserName { get; set; } = "";
}