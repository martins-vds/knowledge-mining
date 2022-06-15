// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowledgeMining.UI.Services.Search
{
    public interface ISearchService
    {
        AutocompleteResults Autocomplete(string searchText, bool fuzzy);
        SearchOptions GenerateSearchOptions(SearchFacet[] searchFacets = null, string[] selectFilter = null, int currentPage = 1, string polygonString = null);
        DocumentResult GetDocumentById(string id);
        DocumentResult GetDocuments(string q, SearchFacet[] searchFacets, int currentPage, string polygonString = null);
        SearchResults<SearchDocument> GetFacets(string searchText, List<string> facetNames, int maxCount = 30);
        string GetSearchId();
        SearchDocument LookUp(string id);
        Task RunIndexer();
        SearchResults<SearchDocument> Search(string searchText, SearchFacet[] searchFacets = null, string[] selectFilter = null, int currentPage = 1, string polygonString = null);
        SuggestResults<SearchDocument> Suggest(string searchText, bool fuzzy);
    }
}