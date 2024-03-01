using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace DiscussionForum.Server.Pages.Components;
public partial class VersionedScript
{
    [Parameter][EditorRequired] public required string Src { get; set; }
    [Parameter] public bool Defer { get; set; } = true;
    [Inject] public required IMemoryCache Cache { get; init; }
    [Inject] public required IWebHostEnvironment Env { get; init; }

    private string GetSrc()
    {
        string? href = Cache.GetOrCreate(Src, _ =>
        {
            string filePath = Path.Combine(Env.WebRootPath, Src);
            string version = GetVersion(filePath);
            return $"{Src}?v={version}";
        });
        return href ?? Src;
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