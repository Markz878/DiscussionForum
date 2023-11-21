using DiscussionForum.Core.Features.Topics;
using DiscussionForum.Shared.DTO.Topics;
using DiscussionForum.Shared.DTO.Users;

namespace DiscussionForum.Server.Pages;

public sealed partial class ViewTopic
{
    [Inject] public required IMediator Mediator { get; set; }
    [Parameter][EditorRequired] public required long TopicId { get; set; }
    [CascadingParameter] public required Task<AuthenticationState> AuthenticationStateTask { get; init; }

    private UserInfo? _userInfo;
    private GetTopicByIdResult? _topic;

    protected override async Task OnInitializedAsync()
    {
        _userInfo = await AuthenticationStateTask.GetUserInfo();
        _topic = await Mediator.Send(new GetTopicByIdQuery() { TopicId = TopicId, UserId = _userInfo.TryGetUserId() });
    }
}
