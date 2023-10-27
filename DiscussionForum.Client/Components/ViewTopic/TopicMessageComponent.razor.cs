namespace DiscussionForum.Client.Components.ViewTopic;

public partial class TopicMessageComponent
{
    [Parameter][EditorRequired] public required TopicMessage Message { get; init; }
    [Parameter][EditorRequired] public required UserInfo CurrentUserInfo { get; init; }
    [Parameter][EditorRequired] public required EventCallback<EditMessageRequest> EditMessageHandler { get; init; }
    [Parameter][EditorRequired] public required EventCallback<long> DeleteMessageHandler { get; init; }
    [Parameter][EditorRequired] public required EventCallback<long> AddMessageLikeHandler { get; init; }
    [Parameter][EditorRequired] public required EventCallback<long> DeleteMessageLikeHandler { get; init; }

    private bool _isEditing;
    private EditMessageModel _editingMessage = new();

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
        await EditMessageHandler.InvokeAsync(new EditMessageRequest() { MessageId = Message.Id, Message = _editingMessage.Message });
        _isEditing = false;
    }

    private async Task ClickDelete()
    {
        await DeleteMessageHandler.InvokeAsync(Message.Id);
    }

    private async Task ClickUpvote()
    {
        if (Message.HasUserUpvoted)
        {
            await DeleteMessageLikeHandler.InvokeAsync(Message.Id);
        }
        else
        {
            await AddMessageLikeHandler.InvokeAsync(Message.Id);
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