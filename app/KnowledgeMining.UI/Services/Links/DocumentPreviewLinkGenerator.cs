using KnowledgeMining.UI.Api;

namespace KnowledgeMining.UI.Services.Links
{
    public class DocumentPreviewLinkGenerator : ILinkGenerator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linker;

        public DocumentPreviewLinkGenerator(
            LinkGenerator linker,
            IHttpContextAccessor httpContextAccessor)
        {
            _linker = linker;
            _httpContextAccessor = httpContextAccessor;
        }

        public Uri GenerateDocumentPreviewUrl(string documentName)
        {
            var relativePath = _linker.GetPathByName(PreviewFileEndpoint.EndpointName, values: new { fileName = documentName });
            return new Uri($"{_httpContextAccessor.HttpContext.Request.Scheme}{Uri.SchemeDelimiter}{_httpContextAccessor.HttpContext.Request.Host}{relativePath}");
        }
    }
}
