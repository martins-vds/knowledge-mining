using System.Net.Mime;

namespace KnowledgeMining.UI.Extensions
{
    public static class ResultsExtensions
    {
        public static IResult InlineFile(this IResultExtensions resultExtensions, byte[] fileContents, string fileName, string contentType, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(resultExtensions);

            return new InlineFileResult(fileContents, fileName, contentType, cancellationToken);
        }
    }

    class InlineFileResult : IResult
    {
        private readonly byte[] _fileContents;
        private readonly string _fileName;
        private readonly string _contentType;
        private readonly CancellationToken _cancellationToken;

        public InlineFileResult(byte[] fileContents, string fileName, string contentType, CancellationToken cancellationToken)
        {
            _fileContents = fileContents;
            _fileName = fileName;
            _contentType = contentType;
            _cancellationToken = cancellationToken;
        }

        public Task ExecuteAsync(HttpContext httpContext)
        {
            var contentDisposition = new ContentDisposition()
            {
                Inline = true,
                FileName = _fileName
            };

            httpContext.Response.ContentType = _contentType;
            httpContext.Response.Headers.ContentDisposition = contentDisposition.ToString();

            return httpContext.Response.BodyWriter.AsStream().WriteAsync(_fileContents, 0, _fileContents.Length, _cancellationToken);
        }
    }
}
