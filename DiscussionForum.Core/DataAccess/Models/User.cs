namespace DiscussionForum.Core.DataAccess.Models;
internal sealed class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public Role Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
}

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    void IEntityTypeConfiguration<User>.Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(x => x.Email).IsUnique().IsCreatedOnline();
        builder.Property(x => x.Email).HasMaxLength(ValidationConstants.UserEmailMaxLength);
        builder.HasIndex(x => x.UserName).IsUnique().IsCreatedOnline();
        builder.Property(x => x.UserName).HasMaxLength(ValidationConstants.UserNameMaxLength);
        builder.Property(x => x.Role).HasConversion(x => x.ToString(), x => Enum.Parse<Role>(x)).HasMaxLength(20);
    }
}
