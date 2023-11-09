namespace DiscussionForum.Server.HelperMethods;

public static class TaskHelpers
{
    public static async Task<(T1 result, T2 result2)> RunParallel<T1, T2>(Task<T1> func1, Task<T2> func2)
    {
        await Task.WhenAll(func1, func2);
        return (func1.Result, func2.Result);
    }
}
