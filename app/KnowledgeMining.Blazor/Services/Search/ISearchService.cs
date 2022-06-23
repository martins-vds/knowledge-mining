// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using KnowledgeMining.UI.Services.Search.Models;

namespace KnowledgeMining.UI.Services.Search
{
    public interface ISearchService
    {
        Task<IEnumerable<string>> Autocomplete(string searchText, bool fuzzy, CancellationToken cancellationToken);
        Task<SearchResponse> SearchDocuments(SearchRequest request, CancellationToken cancellationToken);
    }
}