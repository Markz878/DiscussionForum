namespace DiscussionForum.Shared.HelperMethods;

public static class DateTimeHelpers
{
    public static string GetDateTimeAge(DateTimeOffset dateTime)
    {
        TimeSpan diff = DateTimeOffset.UtcNow - dateTime;
        if (diff.TotalSeconds < 120)
        {
            return $"{Math.Floor(Math.Max(diff.TotalSeconds, 0))} seconds ago";
        }
        if (diff.TotalMinutes < 120)
        {
            return $"{Math.Floor(diff.TotalMinutes)} minutes ago";
        }
        else if (diff.TotalHours < 48)
        {
            return $"{Math.Floor(diff.TotalHours)} hours ago";
        }
        else
        {
            return $"{Math.Floor(diff.TotalDays)} days ago";
        }
    }
}
