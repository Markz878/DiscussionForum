using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace DiscussionForum.Server.Pages.Components;
public partial class VersionedLink
{
    [Parameter][EditorRequired] public required string Href { get; set; }
    [Inject] public required IMemoryCache Cache { get; init; }
    [Inject] public required IWebHostEnvironment Env { get; init; }

    private string GetHref()
    {
        string? href = Cache.GetOrCreate(Href, _ =>
        {
            string filePath = Path.Combine(Env.WebRootPath, Href);
            string version = GetVersion(filePath);
            return $"{Href}?v={version}";
        });
        return href ?? Href;
    }

    private static string GetVersion(string filePath)
    {
        try
        {
            using SHA1 sha1 = SHA1.Create();
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            byte[] hash = sha1.ComputeHash(fs);
            return Convert.ToBase64String(hash);
        }
        catch (Exception)
        {
            byte[] hash = new byte[16];
            Random.Shared.NextBytes(hash);
            return Convert.ToBase64String(hash);
        }
    }
}