namespace DiscussionForum.Client.Components.ViewTopic;

public partial class TopicHeader
{
    [Inject] public required IMediator Mediator { get; init; }
    [Parameter][EditorRequired] public required GetTopicByIdResult Topic { get; init; }
    [Parameter][EditorRequired] public required UserInfo UserInfo { get; init; }
    [Parameter][EditorRequired] public required EventCallback InvokeDeleteConfirm { get; init; }

    private EditTitleModel _editTitle = new();
    private bool _isEditingTitle;

    protected override void OnInitialized()
    {
        _editTitle = new() { Title = Topic?.Title ?? "" };
    }

    private bool CanUserEdit()
    {
        return (Topic?.Messages.Count <= 1 && UserInfo?.GetUserName() == Topic.UserName) || UserInfo?.GetUserRole() == Role.Admin;
    }

    private void StartTitleEdit()
    {
        _isEditingTitle = true;
    }

    private void CancelTitleEdit()
    {
        _isEditingTitle = false;
        _editTitle = new() { Title = Topic?.Title ?? "" };
    }

    private async Task SubmitTitleEdit()
    {
        ArgumentNullException.ThrowIfNull(Topic);
        Topic.Title = _editTitle.Title;
        _isEditingTitle = false;
        await Mediator.Send(new EditTopicTitleClientCommand() { NewTitle = _editTitle.Title, TopicId = Topic.Id });
    }

    private async Task ShowDeleteTopicConfirm()
    {
        await InvokeDeleteConfirm.InvokeAsync();
    }
}

public class EditTitleModel
{
    [Required]
    [MinLength(ValidationConstants.TopicTitleMinLength)]
    [MaxLength(ValidationConstants.TopicTitleMaxLength)]
    public string Title { get; set; } = "";
}