//using DiscussionForum.Shared.DTO.Messages;
//using DiscussionForum.Shared.DTO.Users;

//namespace DiscussionForum.Core.Services.Validation;

//public sealed record AddTopicRequest
//{
//    public required string Title { get; init; }
//    public required string FirstMessage { get; init; }
//    public required Guid UserId { get; init; }
//    public AttachedFileInfo[]? AttachedFiles { get; init; }
//}

//public sealed class AddTopicRequestValidator : AbstractValidator<AddTopicRequest>
//{
//    public AddTopicRequestValidator()
//    {
//        RuleFor(x => x.Title).NotEmpty().MinimumLength(ValidationConstants.TopicTitleMinLength).MaximumLength(ValidationConstants.TopicTitleMaxLength);
//        RuleFor(x => x.FirstMessage).NotEmpty().MaximumLength(ValidationConstants.MessageContentMaxLength);
//        RuleFor(x => x.AttachedFiles).Must(x => x is null or { Length: <= ValidationConstants.MessageMaxFiles }).WithMessage("Topic message can contain maximum of 4 attached files.");
//    }
//}

//public sealed record EditTopicTitleRequest
//{
//    public required long TopicId { get; init; }
//    public required string NewTitle { get; init; }
//    public Guid UserId { get; set; }
//    public Role UserRole { get; set; }
//}

//public sealed class EditTopicTitleRequestValidator : AbstractValidator<EditTopicTitleRequest>
//{
//    public EditTopicTitleRequestValidator()
//    {
//        RuleFor(x => x.NewTitle)
//            .NotEmpty()
//            .MinimumLength(ValidationConstants.TopicTitleMinLength)
//            .MaximumLength(ValidationConstants.TopicTitleMaxLength);
//    }
//}