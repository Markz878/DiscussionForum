namespace DiscussionForum.Shared.Models.Users;
public sealed record UpsertUser : IRequest
{
    public Guid UserId { get; set; }
    public required string Email { get; init; }
    public required string UserName { get; init; }
}

public sealed class UpsertUserValidator : AbstractValidator<UpsertUser>
{
    public UpsertUserValidator()
    {
        RuleFor(x => x.UserName).MinimumLength(1).MaximumLength(ValidationConstants.UserNameMaxLength);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(ValidationConstants.UserEmailMaxLength);
    }
}

