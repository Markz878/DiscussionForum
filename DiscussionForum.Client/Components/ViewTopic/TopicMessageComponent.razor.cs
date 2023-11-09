namespace DiscussionForum.Client.Components.ViewTopic;

public partial class TopicMessageComponent
{
    [Inject] public required IMediator Mediator { get; set; }
    [Parameter][EditorRequired] public required TopicMessage Message { get; init; }
    [Parameter][EditorRequired] public required UserInfo CurrentUserInfo { get; init; }
    [Parameter] public bool CanDelete { get; init; } = true;
    [Parameter][EditorRequired] public required EventCallback<long> DeleteMessageHandler { get; init; }

    private bool _isEditing;
    private EditMessageModel _editingMessage = new();
    private string? errorMessage;

    protected override void OnParametersSet()
    {
        _editingMessage = new() { Message = Message.Content };
    }

    private bool CanUserEditMessage(string messageUserName)
    {
        return messageUserName == CurrentUserInfo.GetUserName() || CurrentUserInfo.GetUserRole() == Role.Admin;
    }

    private void StartMessageEdit()
    {
        _isEditing = true;
    }

    private void CancelMessageEdit()
    {
        _isEditing = false;
        _editingMessage.Message = Message.Content;
    }

    private static string GetShortenedFileName(string fileName)
    {
        return fileName.Length < 40 ? fileName : fileName[..15] + "..." + fileName[^15..];
    }

    private async Task SubmitMessageEdit()
    {
        if (string.IsNullOrWhiteSpace(_editingMessage.Message))
        {
            return;
        }
        errorMessage = "";
        try
        {
            Message.Content = _editingMessage.Message;
            await Mediator.Send(new EditMessageClientCommand() { MessageId = Message.Id, Message = _editingMessage.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            errorMessage = "Error while editing message. Please try again later.";
        }
        _isEditing = false;
    }

    private async Task ClickDelete()
    {
        await DeleteMessageHandler.InvokeAsync(Message.Id);
    }

    private async Task ClickUpvote()
    {
        try
        {
            errorMessage = "";
            if (Message.HasUserUpvoted)
            {
                Message.HasUserUpvoted = false;
                Message.LikesCount--;
                await Mediator.Send(new DeleteMessageLikeClientCommand() { MessageId = Message.Id });
            }
            else
            {
                Message.HasUserUpvoted = true;
                Message.LikesCount++;
                await Mediator.Send(new AddMessageLikeClientCommand() { MessageId = Message.Id });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            errorMessage = "Error while upvoting message. Please try again later.";
        }
    }
}

internal class EditMessageModel
{
    [Required]
    [MinLength(1)]
    [MaxLength(ValidationConstants.MessageContentMaxLength)]
    public string Message { get; set; } = "";
}