namespace DiscussionForum.Core.Features.Messages;
internal class GetFileNameHandler : IRequestHandler<GetFileNameById, GetFileNameByIdResult?>
{
    private readonly AppDbContext db;

    public GetFileNameHandler(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<GetFileNameByIdResult?> Handle(GetFileNameById message, CancellationToken cancellationToken = default)
    {
        string? name = await db.MessageAttachedFiles.Where(x => x.Id == message.Id).Select(x => x.Name).FirstOrDefaultAsync(cancellationToken);
        return name == null ? null : new GetFileNameByIdResult() { FileName = name };
    }
}
