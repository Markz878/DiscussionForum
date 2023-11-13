using DiscussionForum.Client.Components.Common;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;

namespace DiscussionForum.Client.Components.ViewTopic;

public sealed partial class ViewTopic : IAsyncDisposable
{
    [Inject] public required IDataFetchQueries DataFetchQueries { get; set; }
    [Inject] public required IMediator Mediator { get; set; }
    [Inject] public required NavigationManager Navigation { get; init; }
    [Parameter][EditorRequired] public required long TopicId { get; set; }
    [CascadingParameter] public required Task<AuthenticationState> AuthenticationStateTask { get; init; }
    [Inject] public required PersistentComponentState PersistentComponentState { get; init; }
    [Inject] public required RenderLocation RenderLocation { get; init; }

    private PersistingComponentStateSubscription stateSubscription;
    private GetTopicByIdResult? _topic;
    private HubConnection? _hubConnection;
    private EditTitleModel _editTitle = new();
    private bool _isEditingTitle;
    private UserInfo? _userInfo;
    private Modal? modal;
    private string modalHeader = "";
    private string modalMessage = "";
    private RenderFragment? modalContent;

    protected override async Task OnInitializedAsync()
    {
        _userInfo = await AuthenticationStateTask.GetUserInfo();
        stateSubscription = PersistentComponentState.RegisterOnPersisting(PersistData);
        _editTitle = new() { Title = _topic?.Title ?? "" };
        if (!PersistentComponentState.TryTakeFromJson(nameof(_topic), out _topic))
        {
            _topic = await DataFetchQueries.GetTopicById(TopicId, _userInfo.TryGetUserId());
        }
    }

    private Task PersistData()
    {
        PersistentComponentState?.PersistAsJson(nameof(_topic), _topic);
        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (RenderLocation is ClientRenderLocation && firstRender)
        {
            _hubConnection = BuildHubConnection(Navigation.ToAbsoluteUri("/topichub"));
            await _hubConnection.StartAsync();
            await _hubConnection.InvokeAsync(nameof(ITopicHubClientActions.JoinTopic), TopicId);
        }
    }

    private HubConnection BuildHubConnection(Uri hubUri, Action<HttpConnectionOptions>? configureHttpConnection = null)
    {
        HubConnection hub = new HubConnectionBuilder()
            .WithUrl(hubUri, configureHttpConnection ?? (_ => { }))
            .AddMessagePackProtocol()
            .WithAutomaticReconnect()
            .Build();

        hub.On<string>(nameof(ITopicHubNotifications.TopicTitleEdited), (title) =>
        {
            ArgumentNullException.ThrowIfNull(_topic);
            _topic.Title = title;
            StateHasChanged();
        });

        hub.On(nameof(ITopicHubNotifications.TopicDeleted), async () =>
        {
            modalHeader = "Topic deleted";
            modalMessage = "The topic you were viewing has been deleted by the author, going back to home page...";
            modalContent = ModalMessage();
            await (modal?.Show() ?? Task.CompletedTask);
            await Task.Delay(3000);
            Navigation.NavigateTo("/");
        });

        hub.On(nameof(ITopicHubNotifications.MessageAdded), (TopicMessage message) =>
        {
            ArgumentNullException.ThrowIfNull(_topic);
            _topic.Messages.Add(message);
            StateHasChanged();
        });

        hub.On<long, string, DateTimeOffset>(nameof(ITopicHubNotifications.MessageEdited), (messageId, content, editedAt) =>
        {
            ArgumentNullException.ThrowIfNull(_topic);
            TopicMessage? messageToEdit = _topic.Messages.FirstOrDefault(x => x.Id == messageId);
            if (messageToEdit is not null)
            {
                messageToEdit.Content = content;
                messageToEdit.EditedAt = editedAt;
                StateHasChanged();
            }
        });

        hub.On<long>(nameof(ITopicHubNotifications.MessageDeleted), (messageId) =>
        {
            ArgumentNullException.ThrowIfNull(_topic);
            _topic.Messages.RemoveAll(x => x.Id == messageId);
            StateHasChanged();
        });

        hub.On<long, int, bool, Guid>(nameof(ITopicHubNotifications.MessageLikesChanged), (messageId, likesCount, likeAdded, userId) =>
        {
            ArgumentNullException.ThrowIfNull(_topic);
            TopicMessage? messageToEdit = _topic.Messages.FirstOrDefault(x => x.Id == messageId);
            if (messageToEdit is not null)
            {
                messageToEdit.LikesCount = likesCount;
                if (_userInfo?.TryGetUserId() == userId)
                {
                    messageToEdit.HasUserUpvoted = likeAdded;
                }
                StateHasChanged();
            }
        });

        return hub;
    }

    private bool CanUserEdit()
    {
        return (_topic?.Messages.Count <= 1 && _userInfo?.GetUserName() == _topic.UserName) || _userInfo?.GetUserRole() == Role.Admin;
    }

    private void StartTitleEdit()
    {
        _isEditingTitle = true;
    }

    private void CancelTitleEdit()
    {
        _isEditingTitle = false;
        _editTitle = new() { Title = _topic?.Title ?? "" };
    }

    private async Task SubmitTitleEdit()
    {
        ArgumentNullException.ThrowIfNull(_topic);
        _topic.Title = _editTitle.Title;
        _isEditingTitle = false;
        await Mediator.Send(new EditTopicTitleClientCommand() { NewTitle = _editTitle.Title, TopicId = _topic.Id });
    }

    private async Task ShowDeleteTopicConfirm()
    {
        modalHeader = "Confirm delete";
        modalMessage = "Are you sure you want to delete this topic?";
        modalContent = ModalConfirm(ConfirmTopicDeletion);
        await (modal?.Show() ?? Task.CompletedTask);
    }

    private async Task ConfirmTopicDeletion()
    {
        ArgumentNullException.ThrowIfNull(_topic);
        await Mediator.Send(new DeleteTopicClientCommand() { TopicId = _topic.Id });
    }

    private async Task ShowDeleteMessageConfirm(long messageId)
    {
        modalHeader = "Confirm delete";
        modalMessage = "Are you sure you want to delete this message?";
        messageIdToDelete = messageId;
        modalContent = ModalConfirm(ConfirmMessageDeletion);
        await (modal?.Show() ?? Task.CompletedTask);
    }
    private long messageIdToDelete;
    private async Task ConfirmMessageDeletion()
    {
        ArgumentNullException.ThrowIfNull(_topic);
        _topic.Messages.RemoveAll(x => x.Id == messageIdToDelete);
        await Mediator.Send(new DeleteMessageClientCommand() { MessageId = messageIdToDelete });
    }

    public async ValueTask DisposeAsync()
    {
        stateSubscription.Dispose();
        if (_hubConnection is not null)
        {
            await _hubConnection.InvokeAsync(nameof(ITopicHubClientActions.LeaveTopic), TopicId);
            await _hubConnection.DisposeAsync();
        }
    }
}

public class EditTitleModel
{
    [Required]
    [MinLength(ValidationConstants.TopicTitleMinLength)]
    [MaxLength(ValidationConstants.TopicTitleMaxLength)]
    public string Title { get; set; } = "";
}