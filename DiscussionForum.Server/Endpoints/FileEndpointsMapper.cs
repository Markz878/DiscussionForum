namespace DiscussionForum.Server.Endpoints;

public static class FileEndpointsMapper
{
    public static void MapFileRetrievalEndpoint(this RouteGroupBuilder builder)
    {
        RouteGroupBuilder accountGroup = builder.MapGroup("download")
            .WithTags("File retrieval")
            .AllowAnonymous();

        accountGroup.MapGet("{id:guid}", Download);
    }

    private static async Task<Results<FileStreamHttpResult, NotFound>> Download(Guid id, IFileService fileService, CancellationToken cancellationToken)
    {
        string? fileNameResult = await fileService.GetFileNameById(id, cancellationToken);
        if (fileNameResult == null)
        {
            return TypedResults.NotFound();
        }
        string blobStorageName = id + fileNameResult;
        Stream fileStream = await fileService.Download(blobStorageName, cancellationToken);
        return TypedResults.File(fileStream, fileDownloadName: fileNameResult);
    }
}
