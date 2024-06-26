﻿namespace DiscussionForum.Shared.DTO.Messages;

public sealed record EditMessageRequest
{
    public required long MessageId { get; init; }
    public required string Message { get; set; }
}
