using DiscussionForum.Core.Features.Messages;

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

    private static async Task<Results<FileStreamHttpResult, NotFound>> Download(Guid id, IFileService fileService, IMediator mediator, CancellationToken cancellationToken)
    {
        GetFileNameByIdResult? fileNameResult = await mediator.Send(new GetFileNameByIdQuery() { Id = id }, cancellationToken);
        if (fileNameResult == null)
        {
            return TypedResults.NotFound();
        }
        string blobStorageName = id + fileNameResult.FileName;
        Stream fileStream = await fileService.Download(blobStorageName, cancellationToken);
        return TypedResults.File(fileStream, fileDownloadName: fileNameResult.FileName);
    }
}
