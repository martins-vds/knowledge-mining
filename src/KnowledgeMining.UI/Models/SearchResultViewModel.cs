using KnowledgeMining.UI.Services.Search;

namespace KnowledgeMining.UI.Models
{
    public class SearchResultViewModel
    {
        public DocumentResult documentResult { get; set; }

        public string query { get; set; }

        public SearchFacet[] selectedFacets { get; set; }

        public int currentPage { get; set; }

        public string searchId { get; set; }

        public string[] facetableFields { get; set; }
    }
}