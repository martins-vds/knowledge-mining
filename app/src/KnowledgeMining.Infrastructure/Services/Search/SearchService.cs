// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using KnowledgeMining.Application.Common.Interfaces;
using KnowledgeMining.Application.Common.Options;
using KnowledgeMining.Application.Documents.Commands.DeleteDocument;
using KnowledgeMining.Application.Documents.Queries.SearchDocuments;
using KnowledgeMining.Domain.Entities;
using KnowledgeMining.UI.Services.Search.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace KnowledgeMining.Infrastructure.Services.Search
{
    public class SearchService : ISearchService
    {
        private readonly SearchClient _searchClient;
        private readonly SearchIndexClient _searchIndexClient;
        private readonly ChannelWriter<SearchIndexerJobContext> _jobChannel;

        private readonly Application.Common.Options.SearchOptions _searchOptions;
        private readonly EntityMapOptions _entityMapOptions;

        private readonly ILogger _logger;

        public SearchService(SearchClient searchClient,
                             SearchIndexClient searchIndexClient,
                             ChannelWriter<SearchIndexerJobContext> jobChannel,
                             IOptions<Application.Common.Options.SearchOptions> searchOptions,
                             IOptions<EntityMapOptions> entityMapOptions,
                             ILogger<SearchService> logger)
        {
            _searchClient = searchClient;
            _searchIndexClient = searchIndexClient;
            _jobChannel = jobChannel;

            _searchOptions = searchOptions.Value;
            _entityMapOptions = entityMapOptions.Value;

            _logger = logger;
        }

        // TODO: Add schema to a cache
        public async Task<Schema> GenerateSearchSchema(CancellationToken cancellationToken)
        {
            var response = await _searchIndexClient.GetIndexAsync(_searchOptions.IndexName, cancellationToken);

            return new Schema(response.Value.Fields);
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

        public async Task<SearchDocumentsResponse> SearchDocuments(SearchDocumentsQuery request, CancellationToken cancellationToken)
        {
            var searchSchema = await GenerateSearchSchema(cancellationToken);
            var searchOptions = GenerateSearchOptions(request, searchSchema);

            var searchResults = await _searchClient.SearchAsync<DocumentMetadata>(request.SearchText, searchOptions, cancellationToken);

            if (searchResults == null || searchResults?.Value == null)
            {
                return new SearchDocumentsResponse();
            }

            return new SearchDocumentsResponse()
            {
                TotalCount = searchResults.Value.TotalCount ?? 0,
                Documents = searchResults.Value.GetResults().Select(d => d.Document),
                Facets = SummarizeFacets(searchResults.Value.Facets),
                // Not sure if I need to return page in the search result
                TotalPages = CalculateTotalPages(searchResults.Value.TotalCount ?? 0),
                FacetableFields = searchSchema.Facets.Select(f => f.Name), // Not sure if I need to return page in the search result
                SearchId = ParseSearchId(searchResults)
            };
        }

        public async Task<DocumentMetadata> GetDocumentDetails(string documentId, CancellationToken cancellationToken)
        {
            var response = await _searchClient.GetDocumentAsync<DocumentMetadata>(documentId, cancellationToken: cancellationToken);

            return response.Value;
        }
        private async Task<SearchResults<SearchDocument>> GetFacets(string searchText, IEnumerable<string> facetNames, int maxCount, CancellationToken cancellationToken)
        {
            var facets = new List<string>();

            foreach (var facet in facetNames)
            {
                facets.Add($"{facet}, count:{maxCount}");
            }

            // Execute search based on query string
            var options = new Azure.Search.Documents.SearchOptions()
            {
                SearchMode = SearchMode.Any,
                Size = 10,
                QueryType = SearchQueryType.Full
            };

            foreach (string s in facetNames)
            {
                options.Facets.Add(s);
            }

            return await _searchClient.SearchAsync<SearchDocument>(EscapeSpecialCharacters(searchText), options, cancellationToken);
        }

        private string EscapeSpecialCharacters(string searchText)
        {
            return Regex.Replace(searchText, @"([-+&|!(){}\[\]^""~?:/\\])", @"\$1", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public async Task<EntityMap> GenerateEntityMap(string? q, IEnumerable<string> facetNames, int maxLevels, int maxNodes, CancellationToken cancellationToken)
        {
            var query = "*";

            // If blank search, assume they want to search everything
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = q;
            }

            var facets = new List<string>();

            if (facetNames?.Any() ?? false)
            {
                facets.AddRange(facetNames);
            }
            else
            {
                var schema = await GenerateSearchSchema(cancellationToken);
                var facetablesFacets = schema.Facets.Where(f => f.IsFacetable).Select(f => f.Name);
                if (facetablesFacets.Any())
                {
                    facets.AddRange(facetablesFacets!);
                }
            }

            var entityMap = new EntityMap();

            // Calculate nodes for N levels 
            int CurrentNodes = 0;
            int originalDistance = 100;

            List<FDGraphEdges> FDEdgeList = new List<FDGraphEdges>();
            // Create a node map that will map a facet to a node - nodemap[0] always equals the q term

            var NodeMap = new Dictionary<string, NodeInfo>();

            NodeMap[query] = new NodeInfo(CurrentNodes, "0")
            {
                Distance = originalDistance,
                Layer = 0
            };

            List<string> currentLevelTerms = new List<string>();

            List<string> NextLevelTerms = new List<string>();
            NextLevelTerms.Add(query);

            // Iterate through the nodes up to MaxLevels deep to build the nodes or when I hit max number of nodes
            for (var CurrentLevel = 0; CurrentLevel < maxLevels && maxNodes > 0; ++CurrentLevel, maxNodes /= 2)
            {
                currentLevelTerms = NextLevelTerms.ToList();
                NextLevelTerms.Clear();
                var levelNodeCount = 0;

                NodeInfo? densestNodeThisLayer = default;
                var density = 0;

                foreach (var k in NodeMap)
                    k.Value.Distance += originalDistance;

                foreach (var t in currentLevelTerms)
                {
                    if (levelNodeCount >= maxNodes)
                        break;

                    int facetsToGrab = 10;

                    if (maxNodes < 10)
                    {
                        facetsToGrab = maxNodes;
                    }

                    var response = await GetFacets(t, facets, facetsToGrab, cancellationToken);

                    if (response != null)
                    {
                        int facetColor = 0;

                        foreach (var facet in facets)
                        {
                            var facetVals = response.Facets[facet];
                            facetColor++;

                            foreach (var facetResult in facetVals)
                            {
                                var facetValue = facetResult!.Value.ToString() ?? string.Empty;

                                if (!NodeMap.TryGetValue(facetValue, out NodeInfo? nodeInfo))
                                {
                                    // This is a new node
                                    ++levelNodeCount;
                                    NodeMap[facetValue] = new NodeInfo(++CurrentNodes, facetColor.ToString())
                                    {
                                        Distance = originalDistance,
                                        Layer = CurrentLevel + 1
                                    };

                                    if (CurrentLevel < maxLevels)
                                    {
                                        NextLevelTerms.Add(facetValue);
                                    }
                                }

                                // Add this facet to the fd list
                                var newNode = NodeMap[facetValue];
                                var oldNode = NodeMap[t];
                                if (oldNode != newNode)
                                {
                                    oldNode.ChildCount += 1;
                                    if (densestNodeThisLayer == null || oldNode.ChildCount > density)
                                    {
                                        density = oldNode.ChildCount;
                                        densestNodeThisLayer = oldNode;
                                    }

                                    FDEdgeList.Add(new FDGraphEdges
                                    {
                                        Source = oldNode.Index,
                                        Target = newNode.Index,
                                        Distance = newNode.Distance
                                    });
                                }
                            }
                        }
                    }
                }

                if (densestNodeThisLayer != null)
                    densestNodeThisLayer.LayerCornerStone = CurrentLevel;
            }

            foreach (KeyValuePair<string, NodeInfo> entry in NodeMap)
            {
                entityMap.Nodes.Add(new EntityMapNode()
                {
                    Name = entry.Key,
                    Id = entry.Value.Index,
                    Color = entry.Value.ColorId,
                    Layer = entry.Value.Layer,
                    CornerStone = entry.Value.LayerCornerStone
                });
            }

            FDEdgeList.ForEach(e => entityMap.Links.Add(new EntityMapLink()
            {
                Distance = e.Distance,
                Source = e.Source,
                Target = e.Target
            }));

            return entityMap;
        }

        public ValueTask QueueIndexerJob(CancellationToken cancellationToken)
        {
            return _jobChannel.WriteAsync(new SearchIndexerJobContext(), cancellationToken);
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

        private string ParseSearchId(Response<SearchResults<DocumentMetadata>> searchResults)
        {
            string? searchId = default;

            if (searchResults.GetRawResponse().Headers.TryGetValues("x-ms-azs-searchid", out IEnumerable<string>? headerValues))
            {
                searchId = headerValues?.FirstOrDefault();
            }
            return searchId ?? string.Empty;
        }

        private IEnumerable<SummarizedFacet> SummarizeFacets(IDictionary<string, IList<FacetResult>> facets)
        {
            return facets.Select(f => new SummarizedFacet()
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

        private Azure.Search.Documents.SearchOptions GenerateSearchOptions(
            SearchDocumentsQuery request,
            Schema schema)
        {
            var options = new Azure.Search.Documents.SearchOptions()
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
            foreach (string? f in facets)
            {
                if(f is not null)
                {
                    options.Facets.Add($"{f},sort:count");
                }
            }

            foreach (string h in schema.SearchableFields)
            {
                options.HighlightFields.Add(h);
            }

            var filterBuilder = new StringBuilder();
            var filterInitialized = false;

            if (request.FacetFilters != null)
            {
                foreach (var facetFilter in request.FacetFilters)
                {
                    var facet = schema.Facets.FirstOrDefault(f => f.Name == facetFilter.Name);
                    var facetValues = string.Join(",", facetFilter.Values);

                    string? clause = default;

                    if (facet?.Type == typeof(string[]))
                    {
                        if (filterInitialized is false)
                        {
                            clause = $"{facetFilter.Name}/any(t: search.in(t, '{facetValues}', ','))";
                            filterInitialized = true;
                        }
                        else
                        {
                            clause = $" and {facetFilter.Name}/any(t: search.in(t, '{facetValues}', ','))";
                        }
                    }
                    else if (facet?.Type == typeof(string))
                    {
                        if(filterInitialized is false)
                        {
                            clause = $"{facetFilter.Name} eq '{facetValues}'";
                            filterInitialized = true;
                        }
                        else
                        {
                            clause = $" and {facetFilter.Name} eq '{facetValues}'";
                        }
                    }
                    else if (facet?.Type == typeof(DateTime))
                    {
                        // TODO: Construct DateTime facet query
                    }

                    filterBuilder.Append(clause);
                }
            }

            options.Filter = filterBuilder.ToString();

            // Add Filter based on geographic polygon if it is set.
            if (!string.IsNullOrWhiteSpace(request.PolygonString))
            {
                string geoQuery = $"geo.intersects(geoLocation, geography'POLYGON(({request.PolygonString}))')";

                if (options.Filter is not null && options.Filter.Length > 0)
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
    }
}
