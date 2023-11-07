namespace DiscussionForum.Shared.Models.Errors;

public class BusinessException(string message) : Exception(message)
{
}
