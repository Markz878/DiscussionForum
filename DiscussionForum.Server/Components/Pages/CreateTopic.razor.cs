using DiscussionForum.Shared.Models.Topics;
using System.ComponentModel.DataAnnotations;

namespace DiscussionForum.Server.Components.Pages;

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
            Navigation.NavigateTo("/setusername");
        }
    }

    private async Task SubmitAddTopic()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_userInfo);
            ArgumentNullException.ThrowIfNull(AddTopicModel);
            AddTopicResult response = await Mediator.Send(new AddTopic()
            {
                Title = AddTopicModel.Title,
                FirstMessage = AddTopicModel.FirstMessage,
                UserId = _userInfo.GetUserId(),
                AttachedFiles = AddTopicModel?.Files?.Select(x =>
                    new AddAttachedFile() { Name = x.FileName, FileStream = x.OpenReadStream() }).ToArray()
            });
            Navigation.NavigateTo($"/topic/{response.Id}");
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
    public string FirstMessage { get; set; } = "";
    public IFormFileCollection? Files { get; set; }
}