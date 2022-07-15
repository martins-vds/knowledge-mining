using KnowledgeMining.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Application.Documents.Queries.SearchDocuments
{
    public class SearchDocumentsResponse
    {
        public IEnumerable<DocumentMetadata> Documents { get; set; } = Enumerable.Empty<DocumentMetadata>();
        public IEnumerable<AggregateFacet> Facets { get; set; } = Enumerable.Empty<AggregateFacet>();
        public long TotalPages { get; set; }
        public IEnumerable<string> FacetableFields { get; set; } = Enumerable.Empty<string>();
        public long TotalCount { get; set; }
        public IEnumerable<AggregateFacet> Tags { get; set; } = Enumerable.Empty<AggregateFacet>();
        public string SearchId { get; set; }
    }

    public class SearchDocumentsQuery : IRequest<SearchDocumentsResponse>
    {
        public SearchDocumentsQuery(string query,
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
