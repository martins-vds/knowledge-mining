// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;

namespace KnowledgeMining.UI.Services.Search.Models
{
    public class DocumentResponse
    {
        public Response<DocumentFullMetadata> FullMetadata { get; internal set; }
        public Uri Token { get; internal set; }
        public string FilePath { get; internal set; }
    }
}