// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using KnowledgeMining.Blazor.Models;
using KnowledgeMining.UI.Services.Search.Models;
using Microsoft.Extensions.Options;
using System.Net;

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
            AutocompleteOptions options = new()
            {
                Mode = AutocompleteMode.OneTermWithContext,
                UseFuzzyMatching = fuzzy,
                Size = _searchOptions.PageSize
            };

            var response = await _searchClient.AutocompleteAsync(searchText, _searchOptions.SuggesterName, options, cancellationToken);


            return response.Value.Results.Select(r => r.Text).Distinct();
        }

        public async Task<SearchResponse> SearchDocuments(SearchRequest request, CancellationToken cancellationToken)
        {
            var searchSchema = await GenerateSearchSchema(cancellationToken);
            var searchOptions = await GenerateSearchOptions(request, cancellationToken);

            var searchResults = await _searchClient.SearchAsync<DocumentMetadata>(request.SearchText, searchOptions, cancellationToken);

            if (searchResults == null || searchResults?.Value == null)
            {
                return new SearchResponse();
            }

            return new SearchResponse()
            {
                TotalCount = searchResults.Value.TotalCount ?? 0,
                Documents = searchResults.Value.GetResults().Select(d => d.Document),
                Facets = AggregateFacets(searchResults.Value.Facets),
                Tags = AggregateFacets(searchResults.Value.Facets),
                // Not sure if I need to return page in the search result
                TotalPages = CalculateTotalPages(searchResults.Value.TotalCount ?? 0),
                FacetableFields = searchSchema.Facets.Select(f => f.Name), // Not sure if I need to return page in the search result
                SearchId = GetSearchId(searchResults)
            };
        }

        private long CalculateTotalPages(long resultsTotalCount)
        {
            var pageCount = resultsTotalCount / _searchOptions.PageSize;

            if (resultsTotalCount % _searchOptions.PageSize > 0)
            {
                pageCount++;
            }

            return pageCount;
        }

        private string GetSearchId(Response<SearchResults<DocumentMetadata>> searchResults)
        {
            IEnumerable<string> headerValues;
            string searchId = null;
            if (searchResults.GetRawResponse().Headers.TryGetValues("x-ms-azs-searchid", out headerValues))
            {
                searchId = headerValues.FirstOrDefault();
            }
            return searchId ?? string.Empty;
        }

        private IEnumerable<AggregateFacet> AggregateFacets(IDictionary<string, IList<FacetResult>> facets)
        {
            return facets.Where(f => f.Value.Any()).Select(f => new AggregateFacet()
            {
                Name = f.Key,
                Count = f.Value.Count,
                Values = f.Value.Select(v => new Facet()
                {
                    Name = f.Key,
                    Value = v.AsValueFacetResult<string>().Value,
                    Count = v.Count ?? 0
                })
            });
        }

        private async Task<SearchOptions> GenerateSearchOptions(
            SearchRequest request,
            CancellationToken cancellationToken)
        {
            var schema = await GenerateSearchSchema(cancellationToken);
            var options = new SearchOptions()
            {
                SearchMode = SearchMode.All,
                Size = _searchOptions.PageSize,
                Skip = (request.Page - 1) * _searchOptions.PageSize,
                IncludeTotalCount = true,
                QueryType = SearchQueryType.Full,
                HighlightPreTag = "<b>",
                HighlightPostTag = "</b>"
            };

            foreach (string s in schema.SelectFilter)
            {
                options.Select.Add(s);
            }

            var facets = schema.Facets.Select(f => f.Name).ToList();
            foreach (string f in facets)
            {
                options.Facets.Add(f + ",sort:count");
            }

            foreach (string h in schema.SearchableFields)
            {
                options.HighlightFields.Add(h);
            }


            string filter = null;
            var filterStr = string.Empty;

            if (request.SearchFacets != null)
            {
                foreach (var item in request.SearchFacets)
                {
                    var facet = schema.Facets.Where(f => f.Name == item.Key).FirstOrDefault();

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
            if (!string.IsNullOrWhiteSpace(request.PolygonString))
            {
                string geoQuery = $"geo.intersects(geoLocation, geography'POLYGON(({request.PolygonString}))')";

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

        private BlobContainerClient GetBlobContainerClient()
        {
            return _blobServiceClient.GetBlobContainerClient(_storageOptions.ContainerName);
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
    }
}
