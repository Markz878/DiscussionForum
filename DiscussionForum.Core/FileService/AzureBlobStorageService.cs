using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;

namespace DiscussionForum.Core.FileService;

internal sealed class AzureBlobStorageService(
    BlobServiceClient blobService, 
    IOptions<FileStorageSettings> storageSettings, 
    ILogger<AzureBlobStorageService> logger) : IFileService
{
    private readonly BlobContainerClient _blobContainerClient = blobService.GetBlobContainerClient(storageSettings.Value.ContainerName);
    private static readonly BlobUploadOptions _blobOptions = new()
    {
        HttpHeaders = new BlobHttpHeaders()
        {
            CacheControl = "public, max-age=31536000"
        }
    };

    public async Task<string> Upload(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Uploading file with name {fileName} to storage", fileName);
        BlockBlobClient blob = _blobContainerClient.GetBlockBlobClient(fileName);
        await blob.UploadAsync(fileStream, _blobOptions, cancellationToken);
        return _blobContainerClient.Uri + "/" + fileName;
    }

    public async Task<Stream> Download(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            BlockBlobClient file = _blobContainerClient.GetBlockBlobClient(fileName);
            Stream data = await file.OpenReadAsync(cancellationToken: cancellationToken);
            return data;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new NotFoundException("The requested file was not found");
        }
    }

    public async Task<bool> Delete(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            BlobClient blob = _blobContainerClient.GetBlobClient(fileName);
            using Response blobResponse = await blob.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not delete file with name {fileName}", fileName);
            return false;
        }
    }

    public async Task<bool> CheckHealth(CancellationToken cancellationToken = default)
    {
        Response<BlobContainerProperties> props = await _blobContainerClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        return props.GetRawResponse().Status == 200;
    }
}
