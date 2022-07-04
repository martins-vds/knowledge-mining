namespace KnowledgeMining.UI.Models
{
    public class SearchState
    {
        public IEnumerable<DocumentMetadata> Documents { get; set; } = Enumerable.Empty<DocumentMetadata>();
        public IEnumerable<AggregateFacet> Facets { get; set; } = Enumerable.Empty<AggregateFacet>();
        public int TotalPages { get; set; } = 0;
        public IEnumerable<string> FacetableFields { get; set; } = Enumerable.Empty<string>();
        public long TotalCount { get; set; } = 0;
        public IEnumerable<AggregateFacet> Tags { get; set; } = Enumerable.Empty<AggregateFacet>();

        public void Reset()
        {
            Documents = Enumerable.Empty<DocumentMetadata>();
            Facets = Enumerable.Empty<AggregateFacet>();
            TotalPages = 0;
            FacetableFields = Enumerable.Empty<string>();
            TotalCount = 0;
            Tags = Enumerable.Empty<AggregateFacet>();
        }
    }
}
