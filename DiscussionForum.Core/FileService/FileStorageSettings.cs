namespace DiscussionForum.Core.FileService;

public sealed class FileStorageSettings
{
    public required string StorageUri { get; init; }
    public required string ContainerName { get; init; }
    public string FullUri => Path.Combine(StorageUri, ContainerName);
}
