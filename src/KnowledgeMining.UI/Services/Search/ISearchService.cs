// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KnowledgeMining.UI.Services.Search
{
    public interface ISearchService
    {
        string GetSearchId();
        Task<AutocompleteResults> Autocomplete(string searchText, bool fuzzy, CancellationToken cancellationToken);
        Task<DocumentResult> GetDocumentById(string id, CancellationToken cancellationToken);
        Task<DocumentResult> GetDocuments(string q, SearchFacet[] searchFacets, int currentPage, string polygonString, CancellationToken cancellationToken);
        Task<SearchResults<SearchDocument>> GetFacets(string searchText, List<string> facetNames, int maxCount, CancellationToken cancellationToken);
        ValueTask<SearchModel> GetSearchModel(CancellationToken cancellationToken);
        Task<SearchDocument> LookUp(string id, CancellationToken cancellationToken);
        Task RunIndexer(CancellationToken cancellationToken);
        Task<SearchResults<SearchDocument>> Search(string searchText, SearchFacet[] searchFacets, string[] selectFilter, int currentPage, string polygonString, CancellationToken cancellationToken);
        Task<SuggestResults<SearchDocument>> Suggest(string searchText, bool fuzzy, CancellationToken cancellationToken);
    }
}