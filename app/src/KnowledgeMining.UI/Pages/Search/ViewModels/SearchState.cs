using KnowledgeMining.Application.Documents.Queries.SearchDocuments;
using KnowledgeMining.Domain.Entities;

namespace KnowledgeMining.UI.Pages.Search.ViewModels
{
    public class SearchState
    {
        public IEnumerable<DocumentMetadata> Documents { get; set; } = Enumerable.Empty<DocumentMetadata>();
        public IEnumerable<SummarizedFacet> Facets { get; set; } = Enumerable.Empty<SummarizedFacet>();
        public int TotalPages { get; set; } = 0;
        public IEnumerable<string?> FacetableFields { get; set; } = Enumerable.Empty<string>();
        public long TotalCount { get; set; } = 0;
        public IEnumerable<SummarizedFacet> Tags { get; set; } = Enumerable.Empty<SummarizedFacet>();

        public void Reset()
        {
            Documents = Enumerable.Empty<DocumentMetadata>();
            Facets = Enumerable.Empty<SummarizedFacet>();
            TotalPages = 0;
            FacetableFields = Enumerable.Empty<string?>();
            TotalCount = 0;
            Tags = Enumerable.Empty<SummarizedFacet>();
        }
    }
}
