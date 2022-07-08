// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using KnowledgeMining.UI.Extensions;
using KnowledgeMining.UI.Models;
using KnowledgeMining.UI.Services.Links;
using KnowledgeMining.UI.Services.Search.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using static KnowledgeMining.UI.Services.Search.FacetGraphGenerator;

namespace KnowledgeMining.UI.Services.Search
{
    public class SearchService : ISearchService
    {
        private readonly SearchClient _searchClient;
        private readonly SearchIndexClient _searchIndexClient;
        private readonly ILinkGenerator _linker;
        private readonly ChannelWriter<SearchIndexerJobContext> _jobChannel;

        private readonly Options.SearchOptions _searchOptions;
        private readonly Options.EntityMapOptions _entityMapOptions;

        private readonly ILogger _logger;

        public SearchService(SearchClient searchClient,
                             SearchIndexClient searchIndexClient,
                             ILinkGenerator linker,
                             ChannelWriter<SearchIndexerJobContext> jobChannel,
                             IOptions<Options.SearchOptions> searchOptions,
                             IOptions<Options.EntityMapOptions> entityMapOptions,
                             ILogger<SearchService> logger)
        {
            _searchClient = searchClient;
            _searchIndexClient = searchIndexClient;
            _linker = linker;
            _jobChannel = jobChannel;

            _searchOptions = searchOptions.Value;
            _entityMapOptions = entityMapOptions.Value;

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
                SearchId = ParseSearchId(searchResults)
            };
        }

        public async Task<DocumentFullMetadata> GetDocumentDetails(string documentId, CancellationToken cancellationToken)
        {
            var response = await _searchClient.GetDocumentAsync<DocumentFullMetadata>(documentId, cancellationToken: cancellationToken);

            Uri documentStoragePath;

            if (_searchOptions.IsPathBase64Encoded)
            {
                documentStoragePath = new Uri(Decode(documentId));
            }
            else
            {
                documentStoragePath = new Uri(documentId);
            }

            var documentName = documentStoragePath.GetFileName();
            var documentFullMetadata = response.Value;

            documentFullMetadata.PreviewUrl = _linker.GenerateDocumentPreviewUrl(documentName);
            return documentFullMetadata;
        }
        private async Task<SearchResults<SearchDocument>> GetFacets(string searchText, IEnumerable<string> facetNames, int maxCount, CancellationToken cancellationToken)
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

            return await _searchClient.SearchAsync<SearchDocument>(EscapeSpecialCharacters(searchText), options, cancellationToken);
        }

        private string EscapeSpecialCharacters(string searchText)
        {
            return Regex.Replace(searchText, @"([-+&|!(){}\[\]^""~*?:/\\])", @"\$1", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public async Task<EntityMapData> GenerateEntityMap(string q, IEnumerable<string> facetNames, int maxLevels, int maxNodes, CancellationToken cancellationToken)
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
                facets.AddRange(_entityMapOptions.Facets.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            var entityMap = new EntityMapData();

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

                NodeInfo densestNodeThisLayer = null;
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
                                var facetValue = facetResult.Value.ToString();

                                NodeInfo nodeInfo = new NodeInfo(-1, String.Empty);
                                if (NodeMap.TryGetValue(facetValue, out nodeInfo) == false)
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
                    var facet = schema.Facets.Where(f => f.Name == item.Name).FirstOrDefault();

                    filterStr = string.Join(",", item.Values);

                    // Construct Collection(string) facet query
                    if (facet.Type == typeof(string[]))
                    {
                        if (string.IsNullOrEmpty(filter))
                            filter = $"{item.Name}/any(t: search.in(t, '{filterStr}', ','))";
                        else
                            filter += $" and {item.Name}/any(t: search.in(t, '{filterStr}', ','))";
                    }
                    // Construct string facet query
                    else if (facet.Type == typeof(string))
                    {
                        if (string.IsNullOrEmpty(filter))
                            filter = $"{item.Name} eq '{filterStr}'";
                        else
                            filter += $" and {item.Name} eq '{filterStr}'";
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

        private string Decode(string base64EncodedPath)
        {
            if (base64EncodedPath == null) throw new ArgumentNullException("input");
            int inputLength = base64EncodedPath.Length;
            if (inputLength < 1) return null;

            // Get padding chars
            int numPadChars = base64EncodedPath[inputLength - 1] - '0';
            if (numPadChars < 0 || numPadChars > 10)
            {
                return null;
            }

            // replace '-' and '_'
            char[] base64Chars = new char[inputLength - 1 + numPadChars];
            for (int iter = 0; iter < inputLength - 1; iter++)
            {
                char c = base64EncodedPath[iter];

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
    }
}
