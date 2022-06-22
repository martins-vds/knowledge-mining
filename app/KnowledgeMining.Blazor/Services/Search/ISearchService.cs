// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Search.Documents.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KnowledgeMining.UI.Services.Search
{
    public interface ISearchService
    {
        Task<IEnumerable<string>> Autocomplete(string searchText, bool fuzzy, CancellationToken cancellationToken);
    }
}