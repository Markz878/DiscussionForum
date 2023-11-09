using DiscussionForum.Core.Features.Users;
using DiscussionForum.Shared.DTO.Users;
using EntityFramework.Exceptions.Common;
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
        _userInfo = AuthenticationStateTask.GetUserInfo().Result;
        CreateUserModel ??= new() { UserName = _userInfo.GetUserName() ?? "" };
    }

    protected async Task SubmitUserName()
    {
        try
        {
            if (_userInfo?.IsAuthenticated == true && string.IsNullOrEmpty(CreateUserModel?.UserName) is false)
            {
                await Mediator.Send(new UpsertUserCommand() { UserId = _userInfo.GetUserId(), Email = _userInfo.GetUserEmail(), UserName = CreateUserModel.UserName });
                Navigation.NavigateTo("/");
            }
        }
        catch (UniqueConstraintException)
        {
            _errorMessage = "Username is already taken, try another one.";
        }
        catch (Exception ex) when (ex is not NavigationException)
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