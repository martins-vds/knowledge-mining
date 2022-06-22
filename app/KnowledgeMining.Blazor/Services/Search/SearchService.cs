// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using KnowledgeMining.UI.Services.Search.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KnowledgeMining.UI.Services.Search
{
    public class SearchService : ISearchService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly SearchClient _searchClient;
        private readonly SearchIndexClient _searchIndexClient;
        private readonly SearchIndexerClient _searchIndexerClient;

        private readonly Options.SearchOptions _searchOptions;
        private readonly Options.StorageOptions _storageOptions;
        private readonly ILogger _logger;

        public SearchService(BlobServiceClient blobServiceClient,
                             SearchClient searchClient,
                             SearchIndexClient searchIndexClient,
                             SearchIndexerClient searchIndexerClient,
                             IOptions<Options.SearchOptions> searchOptions,
                             IOptions<Options.StorageOptions> storageOptions,
                             ILogger<SearchService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _searchClient = searchClient;
            _searchIndexClient = searchIndexClient;
            _searchIndexerClient = searchIndexerClient;

            _searchOptions = searchOptions.Value;
            _storageOptions = storageOptions.Value;

            _logger = logger;
        }

        // TODO: Add schema to a cache
        public async Task<SearchSchema> GenerateSearchSchema(CancellationToken cancellationToken)
        {
            var response = await _searchIndexClient.GetIndexAsync(_searchOptions.IndexName, cancellationToken);
            
            return new SearchSchema(response.Value.Fields);
        }

        public async Task<IEnumerable<string>> Autocomplete(string searchText, bool fuzzy, CancellationToken cancellationToken)
        {
            // Execute search based on query string
            AutocompleteOptions options = new ()
            {
                Mode = AutocompleteMode.OneTermWithContext,
                UseFuzzyMatching = fuzzy,
                Size = 8
            };

            var response = await _searchClient.AutocompleteAsync(searchText, _searchOptions.SuggesterName, options, cancellationToken);
            

            return response.Value.Results.Select(r => r.Text).Distinct();
        }
    }
}
