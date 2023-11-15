using DiscussionForum.Client.Components.Common;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;

namespace DiscussionForum.Client.Components.ViewTopic;

public sealed partial class ViewTopicComponent : IAsyncDisposable
{
    [Inject] public required IMediator Mediator { get; set; }
    [Inject] public required NavigationManager Navigation { get; init; }
    [Inject] public required RenderLocation RenderLocation { get; init; }
    [Parameter][EditorRequired] public required GetTopicByIdResult Topic { get; init; }
    [Parameter][EditorRequired] public required UserInfo UserInfo { get; init; }

    private HubConnection? _hubConnection;
    private Modal? modal;
    private string modalHeader = "";
    private string modalMessage = "";
    private RenderFragment? modalContent;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (RenderLocation is ClientRenderLocation && firstRender)
        {
            _hubConnection = BuildHubConnection(Navigation.ToAbsoluteUri("/topichub"));
            await _hubConnection.StartAsync();
            await _hubConnection.InvokeAsync(nameof(ITopicHubClientActions.JoinTopic), Topic.Id);
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
            ArgumentNullException.ThrowIfNull(Topic);
            Topic.Title = title;
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
            ArgumentNullException.ThrowIfNull(Topic);
            Topic.Messages.Add(message);
            StateHasChanged();
        });

        hub.On<long, string, DateTimeOffset>(nameof(ITopicHubNotifications.MessageEdited), (messageId, content, editedAt) =>
        {
            ArgumentNullException.ThrowIfNull(Topic);
            TopicMessage? messageToEdit = Topic.Messages.FirstOrDefault(x => x.Id == messageId);
            if (messageToEdit is not null)
            {
                messageToEdit.Content = content;
                messageToEdit.EditedAt = editedAt;
                StateHasChanged();
            }
        });

        hub.On<long>(nameof(ITopicHubNotifications.MessageDeleted), (messageId) =>
        {
            ArgumentNullException.ThrowIfNull(Topic);
            Topic.Messages.RemoveAll(x => x.Id == messageId);
            StateHasChanged();
        });

        hub.On<long, int, bool, Guid>(nameof(ITopicHubNotifications.MessageLikesChanged), (messageId, likesCount, likeAdded, userId) =>
        {
            ArgumentNullException.ThrowIfNull(Topic);
            TopicMessage? messageToEdit = Topic.Messages.FirstOrDefault(x => x.Id == messageId);
            if (messageToEdit is not null)
            {
                messageToEdit.LikesCount = likesCount;
                if (UserInfo?.TryGetUserId() == userId)
                {
                    messageToEdit.HasUserUpvoted = likeAdded;
                }
                StateHasChanged();
            }
        });

        return hub;
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
        ArgumentNullException.ThrowIfNull(Topic);
        await Mediator.Send(new DeleteTopicClientCommand() { TopicId = Topic.Id });
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
        ArgumentNullException.ThrowIfNull(Topic);
        Topic.Messages.RemoveAll(x => x.Id == messageIdToDelete);
        await Mediator.Send(new DeleteMessageClientCommand() { MessageId = messageIdToDelete });
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.InvokeAsync(nameof(ITopicHubClientActions.LeaveTopic), Topic.Id);
            await _hubConnection.DisposeAsync();
        }
    }
}

