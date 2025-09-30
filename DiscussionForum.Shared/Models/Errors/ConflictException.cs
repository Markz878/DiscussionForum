namespace DiscussionForum.Shared.Models.Errors;

public class ConflictException(string message) : Exception(message)
{
}
