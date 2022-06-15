// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using KnowledgeMining.UI.Models;
using KnowledgeMining.UI.Services.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KnowledgeMining.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ISearchService searchService, ILogger<HomeController> logger)
        {
            _searchService = searchService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        }

        public IActionResult Search([FromQuery] string q, [FromQuery] string facets = "", [FromQuery] int page = 1)
        {
            // Split the facets.
            //  Expected format: &facets=key1_val1,key1_val2,key2_val1
            var searchFacets = facets
                // Split by individual keys
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                // Split key/values
                .Select(f => f.Split("_", StringSplitOptions.RemoveEmptyEntries))
                // Group by keys
                .GroupBy(f => f[0])
                // Select grouped key/values into SearchFacet array
                .Select(g => new SearchFacet { Key = g.Key, Value = g.Select(f => f[1]).ToArray() })
                .ToArray();

            var viewModel = SearchView(new SearchOptions
            {
                q = q,
                searchFacets = searchFacets,
                currentPage = page
            });

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
        public SearchResultViewModel SearchView([FromForm] SearchOptions searchParams)
        {
            if (searchParams.q == null)
                searchParams.q = "*";
            if (searchParams.searchFacets == null)
                searchParams.searchFacets = new SearchFacet[0];
            if (searchParams.currentPage == 0)
                searchParams.currentPage = 1;

            string searchidId = null;

            searchidId = _searchService.GetSearchId().ToString();

            var viewModel = new SearchResultViewModel
            {
                documentResult = _searchService.GetDocuments(searchParams.q, searchParams.searchFacets, searchParams.currentPage, searchParams.polygonString),
                query = searchParams.q,
                selectedFacets = searchParams.searchFacets,
                currentPage = searchParams.currentPage,
                searchId = searchidId ?? null,
                applicationInstrumentationKey = _configuration.GetSection("InstrumentationKey")?.Value,
                facetableFields = _searchService.Model.Facets.Select(k => k.Name).ToArray()
            };
            return viewModel;
        }

        [HttpPost]
        public IActionResult GetDocumentById(string id = "")
        {
            var result = _searchService.GetDocumentById(id);

            return new JsonResult(result);
        }

        public class MapCredentials
        {
            public string MapKey { get; set; }
        }


        [HttpPost]
        public IActionResult GetMapCredentials()
        {
            string mapKey = _configuration.GetSection("AzureMapsSubscriptionKey")?.Value;

            return new JsonResult(
                new MapCredentials
                {
                    MapKey = mapKey
                });
        }

        [HttpPost]
        public IActionResult GetGraphData(string query, string[] fields, int maxLevels, int maxNodes)
        {
            string[] facetNames = fields;

            if (facetNames == null || facetNames.Length == 0)
            {
                string facetsList = _configuration.GetSection("GraphFacet")?.Value;

                facetNames = facetsList.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (query == null)
            {
                query = "*";
            }

            FacetGraphGenerator graphGenerator = new FacetGraphGenerator(_searchService);
            var graphJson = graphGenerator.GetFacetGraphNodes(query, facetNames.ToList<string>(), maxLevels, maxNodes);

            return Content(graphJson.ToString(), "application/json");
        }

        [HttpPost, HttpGet]
        public IActionResult Suggest(string term, bool fuzzy = true)
        {
            // Change to _docSearch.Suggest if you would prefer to have suggestions instead of auto-completion
            var response = _searchService.Autocomplete(term, fuzzy);

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
