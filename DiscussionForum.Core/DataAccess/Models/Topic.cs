namespace DiscussionForum.Core.DataAccess.Models;
internal sealed class Topic
{
    public long Id { get; set; }
    public required string Title { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastMessageTimeStamp { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public List<Message> Messages { get; set; } = new();
}

internal sealed class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    void IEntityTypeConfiguration<Topic>.Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(ValidationConstants.TopicTitleMaxLength);
        builder.HasIndex(x => x.LastMessageTimeStamp).IsCreatedOnline();
    }
}
