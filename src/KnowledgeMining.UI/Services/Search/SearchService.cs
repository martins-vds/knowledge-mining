// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
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

        public string SearchId { get; set; }

        public SearchModel _searchModel;

        public string ErrorMessage { get; set; }

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

        public async ValueTask<SearchModel> GetSearchModel(CancellationToken cancellationToken)
        {
            if(_searchModel == null)
            {
                var fields = await _searchIndexClient.GetIndexAsync(_searchOptions.IndexName, cancellationToken);
                var schema = new SearchSchema().AddFields(fields.Value.Fields);
                _searchModel = new SearchModel(schema);
            }
            
            return _searchModel;
        }

        public async Task<SearchResults<SearchDocument>> Search(
            string searchText, 
            SearchFacet[] searchFacets, 
            string[] selectFilter, 
            int currentPage, 
            string polygonString,
            CancellationToken cancellationToken)
        {
            SearchOptions options = await GenerateSearchOptions(searchFacets, selectFilter, currentPage, polygonString, cancellationToken);

            //var s = GenerateSearchId(searchText, options);
            //SearchId = s.Result;

            return await _searchClient.SearchAsync<SearchDocument>(searchText, options, cancellationToken);
        }

        private async Task<SearchOptions> GenerateSearchOptions(
            SearchFacet[] searchFacets,
            string[] selectFilter, 
            int currentPage, 
            string polygonString,
            CancellationToken cancellationToken)
        {
            var model = await GetSearchModel(cancellationToken);
            SearchOptions options = new SearchOptions()
            {
                SearchMode = SearchMode.All,
                Size = 10,
                Skip = (currentPage - 1) * 10,
                IncludeTotalCount = true,
                QueryType = SearchQueryType.Full,
                HighlightPreTag = "<b>",
                HighlightPostTag = "</b>"
            };

            foreach (string s in selectFilter)
            {
                options.Select.Add(s);
            }

            var facets = model.Facets.Select(f => f.Name).ToList();
            foreach (string f in facets)
            {
                options.Facets.Add(f + ",sort:count");
            }

            foreach (string h in model.SearchableFields)
            {
                options.HighlightFields.Add(h);
            }


            string filter = null;
            var filterStr = string.Empty;

            if (searchFacets != null)
            {
                foreach (var item in searchFacets)
                {
                    var facet = model.Facets.Where(f => f.Name == item.Key).FirstOrDefault();

                    filterStr = string.Join(",", item.Value);

                    // Construct Collection(string) facet query
                    if (facet.Type == typeof(string[]))
                    {
                        if (string.IsNullOrEmpty(filter))
                            filter = $"{item.Key}/any(t: search.in(t, '{filterStr}', ','))";
                        else
                            filter += $" and {item.Key}/any(t: search.in(t, '{filterStr}', ','))";
                    }
                    // Construct string facet query
                    else if (facet.Type == typeof(string))
                    {
                        if (string.IsNullOrEmpty(filter))
                            filter = $"{item.Key} eq '{filterStr}'";
                        else
                            filter += $" and {item.Key} eq '{filterStr}'";
                    }
                    // Construct DateTime facet query
                    else if (facet.Type == typeof(DateTime))
                    {
                        // TODO: Date filters
                    }
                }
            }

            options.Filter = filter;

            // Add Filter based on geographic polygon if it is set.
            if (polygonString != null && polygonString.Length > 0)
            {
                string geoQuery = "geo.intersects(geoLocation, geography'POLYGON((" + polygonString + "))')";

                if (options.Filter != null && options.Filter.Length > 0)
                {
                    options.Filter += " and " + geoQuery;
                }
                else
                {
                    options.Filter = geoQuery;
                }
            }

            return options;
        }

        public async Task<SuggestResults<SearchDocument>> Suggest(string searchText, bool fuzzy, CancellationToken cancellationToken)
        {
            // Execute search based on query string
            SuggestOptions options = new SuggestOptions()
            {
                UseFuzzyMatching = fuzzy,
                Size = 8
            };

            return await _searchClient.SuggestAsync<SearchDocument>(searchText, "sg", options, cancellationToken);
        }

        public async Task<AutocompleteResults> Autocomplete(string searchText, bool fuzzy, CancellationToken cancellationToken)
        {
            // Execute search based on query string
            AutocompleteOptions options = new AutocompleteOptions()
            {
                Mode = AutocompleteMode.OneTermWithContext,
                UseFuzzyMatching = fuzzy,
                Size = 8
            };

            return await _searchClient.AutocompleteAsync(searchText, "sg", options, cancellationToken);
        }


        public async Task<SearchDocument> LookUp(string id, CancellationToken cancellationToken)
        {
            // Execute geo search based on query string
            return await _searchClient.GetDocumentAsync<SearchDocument>(id, cancellationToken: cancellationToken);
        }


        private async Task<string> GenerateSearchId(string searchText, SearchOptions options)
        {
            var response = await _searchClient.SearchAsync<SearchDocument>(searchText: searchText, options);
            IEnumerable<string> headerValues;
            string searchId = string.Empty;
            if (response.GetRawResponse().Headers.TryGetValues("x-ms-azs-searchid", out headerValues))
            {
                searchId = headerValues.FirstOrDefault();
            }
            return searchId;
        }

        public string GetSearchId()
        {
            if (SearchId != null) { return SearchId; }
            return string.Empty;
        }

        public async Task<SearchResults<SearchDocument>> GetFacets(string searchText, List<string> facetNames, int maxCount, CancellationToken cancellationToken)
        {
            var facets = new List<string>();

            foreach (var facet in facetNames)
            {
                facets.Add($"{facet}, count:{maxCount}");
            }

            // Execute search based on query string
            SearchOptions options = new SearchOptions()
            {
                SearchMode = SearchMode.Any,
                Size = 10,
                QueryType = SearchQueryType.Full
            };

            foreach (string s in facetNames)
            {
                options.Facets.Add(s);
            }

            return await _searchClient.SearchAsync<SearchDocument>(searchText, options, cancellationToken);
        }

        public async Task<DocumentResult> GetDocuments(string q, SearchFacet[] searchFacets, int currentPage, string polygonString, CancellationToken cancellationToken)
        {
            var sasToken = GetServiceSasUriForContainer(GetBlobContainerClient());
            var model = await GetSearchModel(cancellationToken);

            var selectFilter = model.SelectFilter;

            if (!string.IsNullOrEmpty(q))
            {
                q = q.Replace("?", "");
            }

            var response = await Search(q, searchFacets, selectFilter, currentPage, polygonString, cancellationToken);
            var searchId = GetSearchId().ToString();
            var facetResults = new List<Facet>();
            var tagsResults = new List<object>();

            if (response != null && response.Facets != null)
            {
                // Return only the selected facets from the Search Model
                foreach (var facetResult in response.Facets.Where(f => model.Facets.Where(x => x.Name == f.Key).Any()))
                {
                    var cleanValues = GetCleanFacetValues(facetResult);

                    facetResults.Add(new Facet
                    {
                        key = facetResult.Key,
                        value = cleanValues
                    });
                }

                foreach (var tagResult in response.Facets.Where(t => model.Tags.Where(x => x.Name == t.Key).Any()))
                {
                    var cleanValues = GetCleanFacetValues(tagResult);

                    tagsResults.Add(new
                    {
                        key = tagResult.Key,
                        value = cleanValues
                    });
                }
            }

            var result = new DocumentResult
            {
                Results = response == null ? null : response.GetResults(),
                Facets = facetResults,
                Tags = tagsResults,
                Count = response == null ? 0 : Convert.ToInt32(response.TotalCount),
                SearchId = searchId,
                IdField = _searchOptions.KeyField,
                Token = sasToken,
                IsPathBase64Encoded = _searchOptions.IsPathBase64Encoded
            };

            string json = JsonConvert.SerializeObject(facetResults);


            return result;
        }

        /// <summary>
        /// Initiates a run of the search indexer.
        /// </summary>
        public async Task RunIndexer(CancellationToken cancellationToken)
        {
            var indexStatus = await _searchIndexerClient.GetIndexerStatusAsync(_searchOptions.IndexerName);
            if (indexStatus.Value.LastResult.Status != IndexerExecutionStatus.InProgress)
            {
                await _searchIndexerClient.RunIndexerAsync(_searchOptions.IndexerName, cancellationToken);
            }
        }

        private Uri GetServiceSasUriForContainer(BlobContainerClient containerClient,
                                          string storedPolicyName = null)
        {
            // Check whether this BlobContainerClient object has been authorized with Shared Key.
            if (containerClient.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = containerClient.Name,
                    Resource = "c"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                    sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
                    sasBuilder.StartsOn = DateTimeOffset.UtcNow.AddMinutes(-10);
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(60);
                    sasBuilder.IPRange = new SasIPRange(IPAddress.None, IPAddress.None);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                return containerClient.GenerateSasUri(sasBuilder);
            }
            else
            {
                _logger.LogWarning(@"BlobContainerClient must be authorized with Shared Key 
                          credentials to create a service SAS.");
                return null;
            }
        }

        public async Task<DocumentResult> GetDocumentById(string id, CancellationToken cancellationToken)
        {
            var decodedPath = id;

            var response = await LookUp(id, cancellationToken);

            if (_searchOptions.IsPathBase64Encoded)
            {
                decodedPath = Base64Decode(id);
            }

            var sasToken = GetServiceSasUriForContainer(GetBlobContainerClient());

            var result = new DocumentResult
            {
                Result = response,
                Token = sasToken,
                DecodedPath = decodedPath,
                IdField = _searchOptions.KeyField,
                IsPathBase64Encoded = _searchOptions.IsPathBase64Encoded
            };

            return result;
        }

        private BlobContainerClient GetBlobContainerClient()
        {
            return _blobServiceClient.GetBlobContainerClient(_storageOptions.ContainerName);
        }

        private static string Base64Decode(string input)
        {
            if (input == null) throw new ArgumentNullException("input");
            int inputLength = input.Length;
            if (inputLength < 1) return null;

            // Get padding chars
            int numPadChars = input[inputLength - 1] - '0';
            if (numPadChars < 0 || numPadChars > 10)
            {
                return null;
            }

            // replace '-' and '_'
            char[] base64Chars = new char[inputLength - 1 + numPadChars];
            for (int iter = 0; iter < inputLength - 1; iter++)
            {
                char c = input[iter];

                switch (c)
                {
                    case '-':
                        base64Chars[iter] = '+';
                        break;

                    case '_':
                        base64Chars[iter] = '/';
                        break;

                    default:
                        base64Chars[iter] = c;
                        break;
                }
            }

            // Add padding chars
            for (int iter = inputLength - 1; iter < base64Chars.Length; iter++)
            {
                base64Chars[iter] = '=';
            }

            var charArray = Convert.FromBase64CharArray(base64Chars, 0, base64Chars.Length);
            return System.Text.Encoding.Default.GetString(charArray);
        }

        /// <summary>
        /// In some situations you may want to restrict the facets that are displayed in the
        /// UI. This allows you to add some heuristics to remove facets that you may consider unnecessary.
        /// </summary>
        /// <param name="facetResult"></param>
        /// <returns></returns>
        private static List<FacetValue> GetCleanFacetValues(KeyValuePair<string, IList<FacetResult>> facetResult)
        {
            List<FacetValue> cleanValues = new List<FacetValue>();

            if (facetResult.Key == "persons")
            {
                // only add names that are long enough 
                foreach (var element in facetResult.Value)
                {
                    if (element.Values.ToString().Length >= 4)
                    {

                        cleanValues.Add(new FacetValue() { value = element.Value.ToString(), count = element.Count });
                    }
                }

                return cleanValues;
            }
            else
            {
                List<FacetValue> outputFacets = facetResult.Value
                           .Select(x => new FacetValue() { value = x.Value.ToString(), count = x.Count })
                           .ToList();
                return outputFacets;
            }
        }
    }
}
