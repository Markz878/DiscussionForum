//using DiscussionForum.Shared.DTO.Messages;
//using DiscussionForum.Shared.DTO.Users;

//namespace DiscussionForum.Core.Services.Validation;

//public sealed record AddMessageRequest
//{
//    public required long TopicId { get; init; }
//    public required string Message { get; init; }
//    public Guid UserId { get; set; }
//    public AttachedFileInfo[]? AttachedFiles { get; init; }
//}

//public sealed class AddMessageRequestValidator : AbstractValidator<AddMessageRequest>
//{
//    public AddMessageRequestValidator()
//    {
//        RuleFor(x => x.Message).MinimumLength(1).MaximumLength(ValidationConstants.MessageContentMaxLength);
//        RuleFor(x => x.AttachedFiles).Must(x => x is null or { Length: <= ValidationConstants.MessageMaxFiles }).WithMessage("Message can contain maximum of 4 attached files.");
//    }
//}

//public sealed record EditMessageRequest
//{
//    public long MessageId { get; set; }
//    public required string Message { get; set; }
//    public Guid UserId { get; set; }
//    public Role UserRole { get; set; }
//}

//public sealed class EditMessageRequestValidator : AbstractValidator<EditMessageRequest>
//{
//    public EditMessageRequestValidator()
//    {
//        RuleFor(x => x.Message).MinimumLength(1).MaximumLength(ValidationConstants.MessageContentMaxLength);
//    }
//}