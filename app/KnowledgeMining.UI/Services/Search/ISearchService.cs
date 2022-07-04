// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using KnowledgeMining.UI.Models;
using KnowledgeMining.UI.Services.Search.Models;

namespace KnowledgeMining.UI.Services.Search
{
    public interface ISearchService
    {
        Task<IEnumerable<string>> Autocomplete(string searchText, bool fuzzy, CancellationToken cancellationToken);
        Task<SearchResponse> SearchDocuments(SearchRequest request, CancellationToken cancellationToken);
        Task<DocumentFullMetadata> GetDocumentDetails(string documentId, CancellationToken cancellationToken);
        Task<EntityMapData> GenerateEntityMap(string q, IEnumerable<string> facetNames, int maxLevels, int maxNodes, CancellationToken cancellationToken);
        ValueTask QueueIndexerJob(CancellationToken cancellationToken);
    }
}