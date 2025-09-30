namespace DiscussionForum.Client.Components.ViewTopic;

public partial class AddTopicMessageComponent
{
    [Inject] public required IMessagesService MessagesService { get; init; }
    [Parameter][EditorRequired] public required long TopicId { get; init; }
    [Parameter] public string? AdditionalClasses { get; init; }

    private bool isBusy;
    private string? errorMessage;
    private AddMessageModel newMessage = new();

    private void SelectFile(InputFileChangeEventArgs e)
    {
        errorMessage = "";
        newMessage.Files = e.GetMultipleFiles().ToList();
        foreach (IBrowserFile file in newMessage.Files)
        {
            if (file.Size > ValidationConstants.FileMaxSize)
            {
                errorMessage = $"File {file.Name} had size of {file.Size}, maximum size is {ValidationConstants.FileMaxSize}.";
                newMessage.Files.Clear();
                return;
            }
        }
    }

    private async Task SubmitMessage()
    {
        try
        {
            isBusy = true;
            AddMessageResponse response = await MessagesService.AddMessage(TopicId, newMessage.Message,
                newMessage?.Files?.Select(x => new AttachedFileInfo() { Name = x.Name, FileStream = x.OpenReadStream(ValidationConstants.FileMaxSize) }).ToArray());
            newMessage = new();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isBusy = false;
        }
    }
}

public sealed class AddMessageModel
{
    [MinLength(1)]
    [MaxLength(ValidationConstants.MessageContentMaxLength)]
    public string Message { get; set; } = string.Empty;
    [MaxLength(ValidationConstants.MessageMaxFiles)]
    public IList<IBrowserFile>? Files { get; set; }
}