namespace DiscussionForum.Core.FileService;
public interface IFileService
{
    Task<string?> Upload(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<Stream> Download(string fileName, CancellationToken cancellationToken = default);
    Task<bool> Delete(string fileName, CancellationToken cancellationToken = default);
    Task<bool> CheckHealth(CancellationToken cancellationToken = default);
}
