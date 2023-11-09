namespace DiscussionForum.Shared.Models.Errors;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public static NotFoundException SetMessageFromType<T>() where T : class
    {
        return new NotFoundException(typeof(T).Name + " was not found with the given identifier");
    }
}
