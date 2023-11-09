namespace DiscussionForum.Core.DataAccess.Models;

internal sealed class Message
{
    public long Id { get; set; }
    public required string Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? EditedAt { get; set; }
    public long TopicId { get; set; }
    public Topic? Topic { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public List<MessageLike> MessageLikes { get; set; } = [];
    public List<MessageAttachedFile> AttachedFiles { get; set; } = [];
}

internal sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    void IEntityTypeConfiguration<Message>.Configure(EntityTypeBuilder<Message> builder)
    {
        builder.Property(x => x.Content).HasMaxLength(ValidationConstants.MessageContentMaxLength);
        builder.HasOne(x => x.User).WithMany().OnDelete(DeleteBehavior.NoAction);
    }
}
