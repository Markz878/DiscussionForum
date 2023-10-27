namespace DiscussionForum.Client.Components.ViewTopic;

public partial class AddTopicMessageComponent
{
    [Inject] public required IMediator Mediator { get; init; }
    [Parameter][EditorRequired] public required long TopicId { get; init; }
    [Parameter] public string? AdditionalClasses { get; init; }

    private bool isBusy;
    private readonly long fileMaxSize = 5_000_000;
    private string? errorMessage;
    private AddMessageModel newMessage = new();

    private void SelectFile(InputFileChangeEventArgs e)
    {
        errorMessage = "";
        newMessage.Files = e.GetMultipleFiles().ToList();
        foreach (IBrowserFile file in newMessage.Files)
        {
            if (file.Size > fileMaxSize)
            {
                errorMessage = $"File {file.Name} had size of {file.Size}, maximum size is {fileMaxSize}.";
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
            await Mediator.Send(new AddMessageClientCommand()
            {
                Message = newMessage.Message,
                TopicId = TopicId,
                AttachedFiles = newMessage?.Files?.Select(x => new AttachedFileInfo() { Name = x.Name, FileStream = x.OpenReadStream(fileMaxSize) }).ToArray()
            });
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
    public string Message { get; set; } = string.Empty;
    public IList<IBrowserFile>? Files { get; set; }
}