// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using KnowledgeMining.Blazor.Models;

namespace KnowledgeMining.Blazor.Services.Search.Models
{
    public class DocumentResponse
    {
        public Response<DocumentFullMetadata> FullMetadata { get; internal set; }
        public Uri Token { get; internal set; }
        public string FilePath { get; internal set; }
    }
}