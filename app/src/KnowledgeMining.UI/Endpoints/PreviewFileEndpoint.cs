using KnowledgeMining.Application.Common.Interfaces;
using KnowledgeMining.UI.Extensions;

namespace KnowledgeMining.UI.Api
{
    public static class PreviewFileEndpoint
    {
        public const string Route = "api/documents/{fileName}";
        public const string EndpointName = "preview";

        public static async Task<IResult> DownloadInlineFile(
            string fileName,
            IStorageService storageService,
            CancellationToken cancellationToken)
        {
            var fileContents = await storageService.DownloadDocument(fileName, cancellationToken);

            var contentType = FileExtensions.GetContentTypeForFileExtension(fileName.GetFileExtension());

            return Results.Extensions.InlineFile(fileContents, fileName, contentType, cancellationToken);
        }
    }
}
