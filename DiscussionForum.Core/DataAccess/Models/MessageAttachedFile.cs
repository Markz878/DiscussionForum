namespace DiscussionForum.Core.DataAccess.Models;
internal sealed class MessageAttachedFile
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public long MessageId { get; set; }
    public Message? Message { get; set; }
}

internal sealed class MessageAttachedFileConfiguration : IEntityTypeConfiguration<MessageAttachedFile>
{
    void IEntityTypeConfiguration<MessageAttachedFile>.Configure(EntityTypeBuilder<MessageAttachedFile> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(ValidationConstants.AttachmentNameMaxLength);
        builder.HasIndex(x => new { x.Id, x.Name }).IsUnique().IsCreatedOnline();
    }
}
