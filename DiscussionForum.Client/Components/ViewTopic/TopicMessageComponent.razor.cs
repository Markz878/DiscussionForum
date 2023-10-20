using DiscussionForum.Shared.Models.MessageLikes;

namespace DiscussionForum.Client.Components.ViewTopic;

public partial class TopicMessageComponent
{
    [Parameter][EditorRequired] public required TopicMessage Message { get; init; }
    [Parameter][EditorRequired] public required UserInfo CurrentUserInfo { get; init; }
    [Parameter][EditorRequired] public required EventCallback<EditMessage> EditMessageHandler { get; init; }
    [Parameter][EditorRequired] public required EventCallback<DeleteMessage> DeleteMessageHandler { get; init; }
    [Parameter][EditorRequired] public required EventCallback<AddMessageLike> AddMessageLikeHandler { get; init; }
    [Parameter][EditorRequired] public required EventCallback<DeleteMessageLike> DeleteMessageLikeHandler { get; init; }

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
        await EditMessageHandler.InvokeAsync(new EditMessage() { MessageId = Message.Id, Message = _editingMessage.Message, UserId = CurrentUserInfo.GetUserId(), UserRole = CurrentUserInfo.GetUserRole().GetValueOrDefault() });
        _isEditing = false;
    }

    private async Task ClickDelete()
    {
        await DeleteMessageHandler.InvokeAsync(new DeleteMessage() { MessageId = Message.Id });
    }

    private async Task ClickUpvote()
    {
        if (Message.HasUserUpvoted)
        {
            await DeleteMessageLikeHandler.InvokeAsync(new DeleteMessageLike() { MessageId = Message.Id, UserId = CurrentUserInfo.GetUserId() });
        }
        else
        {
            await AddMessageLikeHandler.InvokeAsync(new AddMessageLike() { MessageId = Message.Id, UserId = CurrentUserInfo.GetUserId() });
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