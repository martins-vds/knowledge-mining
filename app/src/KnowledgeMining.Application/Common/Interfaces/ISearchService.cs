// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using KnowledgeMining.Application.Documents.Queries.SearchDocuments;
using KnowledgeMining.Domain.Entities;

namespace KnowledgeMining.Application.Common.Interfaces
{
    public interface ISearchService
    {
        Task<IEnumerable<string>> Autocomplete(string searchText, bool fuzzy, CancellationToken cancellationToken);
        Task<SearchDocumentsResponse> SearchDocuments(SearchDocumentsQuery request, CancellationToken cancellationToken);
        Task<DocumentMetadata> GetDocumentDetails(string documentId, CancellationToken cancellationToken);
        Task<EntityMap> GenerateEntityMap(string? q, IEnumerable<string> facetNames, int maxLevels, int maxNodes, CancellationToken cancellationToken);
        ValueTask QueueIndexerJob(CancellationToken cancellationToken);
    }
}