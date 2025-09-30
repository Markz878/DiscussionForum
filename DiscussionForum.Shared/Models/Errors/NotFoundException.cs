namespace DiscussionForum.Shared.Models.Errors;

public class NotFoundException(string message) : Exception(message)
{
    public static NotFoundException SetMessageFromType<T>() where T : class
    {
        return new NotFoundException(typeof(T).Name + " was not found with the given identifier");
    }
}
