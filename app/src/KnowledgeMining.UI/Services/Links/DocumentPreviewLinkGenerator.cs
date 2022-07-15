using KnowledgeMining.Application.Common.Options;
using KnowledgeMining.UI.Api;
using Microsoft.Extensions.Options;

namespace KnowledgeMining.UI.Services.Links
{
    public class DocumentPreviewLinkGenerator : ILinkGenerator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linker;
        private readonly AzureSignalROptions _azureSignalROptions;

        public DocumentPreviewLinkGenerator(
            LinkGenerator linker,
            IOptions<AzureSignalROptions> azureSignalROptions,
            IHttpContextAccessor httpContextAccessor)
        {
            _linker = linker;
            _httpContextAccessor = httpContextAccessor;
            _azureSignalROptions = azureSignalROptions.Value;
        }

        public string GenerateDocumentPreviewUrl(string documentName)
        {
            var relativePath = _linker.GetPathByName(PreviewFileEndpoint.EndpointName, values: new { fileName = documentName });

            if (_azureSignalROptions.Enabled)
            {
                return relativePath!;
            }
            else
            {
                var link = new Uri($"{_httpContextAccessor?.HttpContext?.Request.Scheme}{Uri.SchemeDelimiter}{_httpContextAccessor?.HttpContext?.Request.Host}{relativePath}");

                return link.AbsoluteUri;
            }
        }
    }
}
