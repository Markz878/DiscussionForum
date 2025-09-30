using DiscussionForum.Shared.DTO.Messages;
using DiscussionForum.Shared.DTO.Topics;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DiscussionForum.Server.Endpoints;

public static class MessageEndpointsMapper
{
    public static void MapMessageEndpoints(this RouteGroupBuilder builder)
    {
        RouteGroupBuilder accountGroup = builder.MapGroup("messages")
            .WithTags("Messages");

        accountGroup.MapPost("", AddMessage).Accepts<AddMessageBinder>("multipart/form-data");
        accountGroup.MapDelete("{messageId:long}", DeleteMessage);
        accountGroup.MapPatch("", UpdateMessage);
    }

    public static async Task<Results<Ok<AddMessageResponse>, ValidationProblem>> AddMessage(AddMessageBinder message, IMessagesService messagesService, IHubContext<TopicHub> topicHub, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        if (long.TryParse(message.TopicId, out long topicId) is false)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { { "TopicId", new[] { $"Given value {message.TopicId} was not parseable to an integer" } } });
        }

        AddMessageResponse result = await messagesService.AddMessage(
            topicId,
            message.Message,
            message.AttachedFiles?.Select(x => new AttachedFileInfo() { Name = x.FileName, FileStream = x.OpenReadStream() }).ToArray(),
            cancellationToken);

        TopicMessage topicMessage = new()
        {
            Id = result.Id,
            Content = message.Message,
            CreatedAt = result.CreatedAt,
            UserName = claimsPrincipal.GetUserName() ?? "",
            AttachedFiles = result.AttachedFiles
        };
        await topicHub.Clients.Group(message.TopicId.ToString()).SendAsync(nameof(ITopicHubNotifications.MessageAdded), topicMessage, cancellationToken);
        return TypedResults.Ok(result);

    }

    public static async Task<Results<NoContent, NotFound>> DeleteMessage(long messageId, IMessagesService messagesService, IHubContext<TopicHub> topicHub, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        try
        {
            long topicId = await messagesService.GetMessageTopicId(messageId, cancellationToken);
            await messagesService.DeleteMessage(messageId, cancellationToken);
            await topicHub.Clients.Group(topicId.ToString()).SendAsync(nameof(ITopicHubNotifications.MessageDeleted), messageId, cancellationToken);
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    public static async Task<Results<Ok<EditMessageResult>, NotFound>> UpdateMessage(EditMessageRequest editMessage, IMessagesService messagesService, IHubContext<TopicHub> topicHub, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        try
        {
            EditMessageResult editMessageResult = await messagesService.EditMessage(editMessage.MessageId, editMessage.Message, cancellationToken);
            await topicHub.Clients.Group(editMessageResult.TopicId.ToString()).SendAsync(nameof(ITopicHubNotifications.MessageEdited), editMessage.MessageId, editMessage.Message, editMessageResult.EditedAt, cancellationToken);
            return TypedResults.Ok(editMessageResult);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}

public class AddMessageBinder
{
    [Required(AllowEmptyStrings = false)]
    public required string TopicId { get; init; }
    [MaxLength(ValidationConstants.MessageContentMaxLength)]
    public required string Message { get; init; }
    [MaxLength(ValidationConstants.MessageMaxFiles)]
    public IFormFileCollection? AttachedFiles { get; init; }

    public static async ValueTask<AddMessageBinder> BindAsync(HttpContext httpContext, ParameterInfo _)
    {
        IFormCollection form = await httpContext.Request.ReadFormAsync();
        AddMessageBinder binder = new()
        {
            TopicId = form["topicid"][0] ?? "0",
            Message = form["message"][0] ?? "",
            AttachedFiles = form.Files
        };
        return binder;
    }
}
