// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using KnowledgeMining.Domain.Entities;

namespace KnowledgeMining.UI.Services.Search.Models
{
    public class SearchResponse
    {
        public IEnumerable<DocumentMetadata> Documents { get; set; } = Enumerable.Empty<DocumentMetadata>();
        public IEnumerable<AggregateFacet> Facets { get; set; } = Enumerable.Empty<AggregateFacet>();
        public long TotalPages { get; set; }
        public IEnumerable<string> FacetableFields { get; set; } = Enumerable.Empty<string>();
        public long TotalCount { get; set; }
        public IEnumerable<AggregateFacet> Tags { get; set; } = Enumerable.Empty<AggregateFacet>();
        public string SearchId { get; set; }
    }
}