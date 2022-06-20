// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using KnowledgeMining.UI.Models;
using KnowledgeMining.UI.Options;
using KnowledgeMining.UI.Services.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KnowledgeMining.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<HomeController> _logger;

        private readonly AzureMapsOptions _azureMapsOptions;
        private readonly GraphOptions _graphOptions;

        public HomeController(ISearchService searchService,
                              IOptions<AzureMapsOptions> azureMapsOptions,
                              IOptions<GraphOptions> graphOptions,
                              ILogger<HomeController> logger)
        {
            _searchService = searchService;
            _azureMapsOptions = azureMapsOptions.Value;
            _graphOptions = graphOptions.Value;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        }

        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] string facets, [FromQuery] int page, CancellationToken cancellationToken)
        {
            // Split the facets.
            //  Expected format: &facets=key1_val1,key1_val2,key2_val1
            var searchFacets = (facets ?? string.Empty)
                // Split by individual keys
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                // Split key/values
                .Select(f => f.Split("_", StringSplitOptions.RemoveEmptyEntries))
                // Group by keys
                .GroupBy(f => f[0])
                // Select grouped key/values into SearchFacet array
                .Select(g => new SearchFacet { Key = g.Key, Value = g.Select(f => f[1]).ToArray() })
                .ToArray();

            var viewModel = await Search(new SearchOptions()
            {
                q = q,
                searchFacets = searchFacets,
                currentPage = page > 0 ? page : 1,
            }, cancellationToken);

            return View(viewModel);
        }

        public class SearchOptions
        {
            public string q { get; set; }
            public SearchFacet[] searchFacets { get; set; }
            public int currentPage { get; set; }
            public string polygonString { get; set; }
        }

        [HttpPost]
        public async Task<SearchResultViewModel> Search([FromForm]SearchOptions searchParams, CancellationToken cancellationToken)
        {
            if (searchParams.q == null)
                searchParams.q = "*";
            if (searchParams.searchFacets == null)
                searchParams.searchFacets = Array.Empty<SearchFacet>();
            if (searchParams.currentPage == 0)
                searchParams.currentPage = 1;

            string searchidId = null;

            searchidId = _searchService.GetSearchId().ToString();

            var viewModel = new SearchResultViewModel
            {
                documentResult = await _searchService.GetDocuments(searchParams.q, searchParams.searchFacets, searchParams.currentPage, searchParams.polygonString, cancellationToken),
                query = searchParams.q,
                selectedFacets = searchParams.searchFacets,
                currentPage = searchParams.currentPage,
                searchId = searchidId ?? null,
                facetableFields = (await _searchService.GetSearchModel(cancellationToken)).Facets.Select(k => k.Name).ToArray()
            };
            return viewModel;
        }

        [HttpPost]
        public async Task<IActionResult> GetDocumentById(string id, CancellationToken cancellationToken)
        {
            var result = await _searchService.GetDocumentById(id ?? string.Empty, cancellationToken);

            return new JsonResult(result);
        }

        public class MapCredentials
        {
            public string MapKey { get; set; }
        }


        [HttpPost]
        public IActionResult GetMapCredentials()
        {
            return new JsonResult(
                new MapCredentials
                {
                    MapKey = _azureMapsOptions.SubscriptionKey
                });
        }

        [HttpPost]
        public async Task<IActionResult> GetGraphData(string query, string[] fields, int maxLevels, int maxNodes, CancellationToken cancellationToken)
        {
            string[] facetNames = fields;

            if (facetNames == null || facetNames.Length == 0)
            {
                string facetsList = _graphOptions.Facets;

                facetNames = facetsList.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (query == null)
            {
                query = "*";
            }

            FacetGraphGenerator graphGenerator = new FacetGraphGenerator(_searchService);
            var graphJson = await graphGenerator.GetFacetGraphNodes(query, facetNames.ToList<string>(), maxLevels, maxNodes, cancellationToken);

            return Content(graphJson.ToString(), "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> Suggest(string term, bool fuzzy, CancellationToken cancellationToken)
        {
            // Change to _docSearch.Suggest if you would prefer to have suggestions instead of auto-completion
            var response = await _searchService.Autocomplete(term, fuzzy, cancellationToken);

            List<string> suggestions = new List<string>();
            if (response != null)
            {
                foreach (var result in response.Results)
                {
                    suggestions.Add(result.Text);
                }
            }

            // Get unique items
            List<string> uniqueItems = suggestions.Distinct().ToList();

            return new JsonResult
            (
                uniqueItems
            );

        }
    }
}
