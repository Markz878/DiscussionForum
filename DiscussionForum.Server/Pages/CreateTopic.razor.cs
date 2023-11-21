using DiscussionForum.Core.Features.Topics;
using DiscussionForum.Shared.DTO.Messages;
using DiscussionForum.Shared.DTO.Users;
using System.ComponentModel.DataAnnotations;

namespace DiscussionForum.Server.Pages;

[Authorize(Policy = "HasUserName")]
public sealed partial class CreateTopic
{
    [Inject] public required IMediator Mediator { get; init; }
    [Inject] public required NavigationManager Navigation { get; init; }
    [CascadingParameter] public required Task<AuthenticationState> AuthenticationStateTask { get; init; }
    [SupplyParameterFromForm] public AddTopicModel? AddTopicModel { get; set; }

    private UserInfo? _userInfo;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        AddTopicModel ??= new();
        _userInfo = await AuthenticationStateTask.GetUserInfo();
        if (string.IsNullOrWhiteSpace(_userInfo?.GetUserName()))
        {
            Navigation.NavigateToSecure("/setusername");
        }
    }

    private async Task SubmitAddTopic()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_userInfo);
            ArgumentNullException.ThrowIfNull(AddTopicModel);
            AddTopicResult response = await Mediator.Send(new AddTopicCommand()
            {
                Title = AddTopicModel.Title,
                FirstMessage = AddTopicModel.FirstMessage,
                UserId = _userInfo.GetUserId(),
                AttachedFiles = AddTopicModel?.Files?.Select(x =>
                    new AttachedFileInfo() { Name = x.FileName, FileStream = x.OpenReadStream() }).ToArray()
            });
            Navigation.NavigateToSecure($"/topic/{response.Id}");
        }
        catch (Exception ex) when (ex is not NavigationException)
        {
            _errorMessage = "Error creating topic, please try again later.";
        }
    }
}

public class AddTopicModel
{
    [Required]
    [MinLength(ValidationConstants.TopicTitleMinLength)]
    [MaxLength(ValidationConstants.TopicTitleMaxLength)]
    public string Title { get; set; } = "";
    [Required]
    [MinLength(1)]
    [MaxLength(ValidationConstants.MessageContentMaxLength)]
    public string FirstMessage { get; set; } = "";
    [MaxLength(ValidationConstants.MessageMaxFiles, ErrorMessage = "Maximum upload of 4 files per message")]
    public IFormFileCollection? Files { get; set; }
}