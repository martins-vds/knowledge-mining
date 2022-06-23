namespace KnowledgeMining.UI.Services.Search.Models
{
    public class SearchRequest
    {
        public SearchRequest(string query,
                             int page,
                             string polygonString,
                             IEnumerable<SearchFacet> searchFacets)
        {
            SearchText = string.IsNullOrWhiteSpace(query) ? "*" : query.Replace("?", string.Empty);
            SearchFacets = (searchFacets ?? Array.Empty<SearchFacet>()).ToList().AsReadOnly();
            Page = page > 0 ? page : 1;
            PolygonString = string.IsNullOrWhiteSpace(polygonString) ? string.Empty : polygonString;
        }

        public string SearchText { get; private set; }
        public IReadOnlyList<SearchFacet> SearchFacets { get; private set; }
        public int Page { get; private set; }
        public string PolygonString { get; private set; }
    }
}
