namespace DiscussionForum.Shared.Models.Messages;
public record class GetFileNameById : IRequest<GetFileNameByIdResult>
{
    public required Guid Id { get; init; }
}

public record class GetFileNameByIdResult
{
    public required string FileName { get; init; }
}
