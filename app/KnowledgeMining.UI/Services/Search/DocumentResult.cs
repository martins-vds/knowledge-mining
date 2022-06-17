// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Search.Documents.Models;
using System;
using System.Collections.Generic;

namespace KnowledgeMining.UI.Services.Search
{
    public class DocumentResult
    {
        public List<Facet> Facets { get; set; }
        public SearchDocument Result { get; set; }
        public Pageable<SearchResult<SearchDocument>> Results { get; set; }
        public int? Count { get; set; }
        public Uri Token { get; set; }
        public string DecodedPath { get; set; }
        public List<object> Tags { get; set; }
        public string SearchId { get; set; }
        public string IdField { get; set; }
        public bool IsPathBase64Encoded { get; set; }
    }

}
