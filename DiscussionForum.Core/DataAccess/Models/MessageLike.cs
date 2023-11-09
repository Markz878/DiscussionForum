namespace DiscussionForum.Core.DataAccess.Models;

internal sealed class MessageLike
{
    public Guid UserId { get; set; }
    public long MessageId { get; set; }
}

internal sealed class MessageLikesConfiguration : IEntityTypeConfiguration<MessageLike>
{
    void IEntityTypeConfiguration<MessageLike>.Configure(EntityTypeBuilder<MessageLike> builder)
    {
        builder.HasKey(x => new { x.MessageId, x.UserId });
    }
}
