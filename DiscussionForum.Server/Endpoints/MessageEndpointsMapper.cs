﻿using DiscussionForum.Core.Features.Messages;
using DiscussionForum.Shared.DTO.Messages;
using DiscussionForum.Shared.DTO.Topics;
using FluentValidation;
using FluentValidation.Results;
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

    public static async Task<Results<Ok<AddMessageResponse>, ValidationProblem>> AddMessage(AddMessageBinder message, IValidator<AddMessageCommand> validator, IMediator mediator, IHubContext<TopicHub> topicHub, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        if (long.TryParse(message.TopicId, out long topicId) is false)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { { "TopicId", new[] { $"Given value {message.TopicId} was not parseable to an integer" } } });
        }
        AddMessageCommand addMessage = new()
        {
            TopicId = topicId,
            Message = message.Message,
            UserId = claimsPrincipal.GetUserId(),
            AttachedFiles = message.AttachedFiles?.Select(x => new AttachedFileInfo() { Name = x.FileName, FileStream = x.OpenReadStream() }).ToArray()
        };
        ValidationResult validationResult = validator.Validate(addMessage);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }
        AddMessageResponse result = await mediator.Send(addMessage, cancellationToken);
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

    public static async Task<Results<NoContent, NotFound>> DeleteMessage(long messageId, IMediator mediator, IHubContext<TopicHub> topicHub, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        GetMessageTopicIdResult? topicIdResult = await mediator.Send(new GetMessageTopicIdQuery() { MessageId = messageId }, cancellationToken);
        if (topicIdResult is null)
        {
            return TypedResults.NotFound();
        }
        DeleteMessageCommand deleteMessage = new()
        {
            TopicId = topicIdResult.TopicId,
            MessageId = messageId,
            UserId = claimsPrincipal.GetUserId(),
            UserRole = claimsPrincipal.GetUserRole(),
        };
        await mediator.Send(deleteMessage, cancellationToken);
        await topicHub.Clients.Group(topicIdResult.TopicId.ToString()).SendAsync(nameof(ITopicHubNotifications.MessageDeleted), messageId, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<Results<Ok<EditMessageResult>, NotFound>> UpdateMessage(EditMessageRequest editMessage, IMediator mediator, IHubContext<TopicHub> topicHub, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        EditMessageCommand command = new()
        {
            MessageId = editMessage.MessageId,
            Message = editMessage.Message,
            UserId = claimsPrincipal.GetUserId(),
            UserRole = claimsPrincipal.GetUserRole()
        };

        command.UserId = claimsPrincipal.GetUserId();
        command.UserRole = claimsPrincipal.GetUserRole();
        (EditMessageResult editMessageResult, GetMessageTopicIdResult? topicIdResult) = await TaskHelpers.RunParallel(
            mediator.Send(command, cancellationToken),
            mediator.Send(new GetMessageTopicIdQuery() { MessageId = editMessage.MessageId }, cancellationToken));
        if (topicIdResult is null)
        {
            return TypedResults.NotFound();
        }
        await topicHub.Clients.Group(topicIdResult.TopicId.ToString()).SendAsync(nameof(ITopicHubNotifications.MessageEdited), editMessage.MessageId, editMessage.Message, editMessageResult.EditedAt, cancellationToken);
        return TypedResults.Ok(editMessageResult);
    }
}

public class AddMessageBinder
{
    public required string TopicId { get; init; }
    public required string Message { get; init; }
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
