using Microsoft.JSInterop;

namespace DiscussionForum.Client.Components.Common;

public sealed partial class Modal
{
    [Parameter][EditorRequired] public required string Header { get; init; }
    [Parameter][EditorRequired] public required RenderFragment ChildContent { get; init; }
    [Parameter] public required EventCallback CloseModal { get; init; }
    [Inject] public required IJSRuntime JS { get; init; }

    private ElementReference modal;
    private IJSObjectReference? module;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Common/Modal.razor.js");
        }
    }

    public async Task Show()
    {
        if (module is not null)
        {
            await module.InvokeVoidAsync("showModal", modal);
        }
    }
}
