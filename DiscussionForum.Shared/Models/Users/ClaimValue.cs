namespace DiscussionForum.Shared.Models.Users;

public sealed class ClaimValue
{
    public ClaimValue(string type, string value)
    {
        Type = type;
        Value = value;
    }

    public string Type { get; set; }
    public string Value { get; set; }
}