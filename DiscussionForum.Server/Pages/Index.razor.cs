using DiscussionForum.Core.Features.Topics;
using DiscussionForum.Shared.DTO.Topics;
using DiscussionForum.Shared.DTO.Users;
using Microsoft.AspNetCore.OutputCaching;

namespace DiscussionForum.Server.Pages;

[OutputCache]
public partial class Index
{
    [Parameter] public int? PageNumber { get; set; }
    [Parameter][SupplyParameterFromQuery] public string? Search { get; set; }
    [CascadingParameter] public required Task<AuthenticationState> AuthenticationStateTask { get; init; }
    [Inject] public required NavigationManager Navigation { get; init; }
    [Inject] public required IMediator Mediator { get; init; }
    [SupplyParameterFromForm] public SearchText? SearchText { get; set; }

    private int _pageNumber;
    private ListLatestTopicsResult? _topicsResult;
    private UserInfo? _userInfo;

    protected override async Task OnInitializedAsync()
    {
        _pageNumber = Math.Max(PageNumber.GetValueOrDefault(0), 0);
        SearchText ??= new SearchText() { Text = Search, IsManuallySet = true };
        _userInfo = await AuthenticationStateTask.GetUserInfo();
        if (_userInfo.IsAuthenticated && string.IsNullOrEmpty(_userInfo.GetUserName()))
        {
            Navigation.NavigateToSecure("/setusername");
        }
        if (SearchText.IsManuallySet)
        {
            _topicsResult = await Mediator.Send(new ListLatestTopicsQuery() { PageNumber = _pageNumber, SearchText = Search });
        }
    }

    private void SearchTopics()
    {
        ArgumentNullException.ThrowIfNull(SearchText);
        string url = string.IsNullOrWhiteSpace(SearchText.Text) ? string.Empty : $"/0?search={SearchText.Text}";
        Navigation.NavigateToSecure(url);
    }

    private string GetUrl(int page)
    {
        return string.IsNullOrWhiteSpace(Search) ? $"/{page}#title" : $"/{page}?search={Search}#title";
    }

    private bool CanShowPreviousPageLink()
    {
        return _pageNumber > 0;
    }

    private bool CanShowNextPageLink()
    {
        return _pageNumber < _topicsResult?.PageCount;
    }
}

public class SearchText
{
    public string? Text { get; set; }
    public bool IsManuallySet { get; set; }
}