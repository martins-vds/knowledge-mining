// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace KnowledgeMining.UI.Services.Search.Models
{
    public class DocumentFullMetadata : DocumentMetadata
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        public string? PreviewUrl { get; set; }
    }
}