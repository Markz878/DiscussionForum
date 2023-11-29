using DiscussionForum.Shared.DTO;

namespace DiscussionForum.Client.Handlers.Topics;

internal sealed record EditTopicTitleClientCommand : IRequest
{
    public required long TopicId { get; init; }
    public required string NewTitle { get; init; }
}

internal class EditTopicTitleClientCommandHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<EditTopicTitleClientCommand>
{
    public async Task Handle(EditTopicTitleClientCommand request, CancellationToken cancellationToken)
    {
        EditTopicTitleRequest editTopicTitleRequest = new()
        {
            TopicId = request.TopicId,
            NewTitle = request.NewTitle
        };
        await httpClientFactory.CreateClient("Client")
            .PatchAsJsonAsync("api/topics", editTopicTitleRequest, JsonContext.Default.EditTopicTitleRequest, cancellationToken);
    }
}
