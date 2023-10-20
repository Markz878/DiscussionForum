namespace DiscussionForum.Shared.Models.Errors;

public class UserNameTakenException : Exception
{
    public UserNameTakenException() : base("Username was already taken")
    {

    }
}
