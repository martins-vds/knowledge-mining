// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace KnowledgeMining.UI.Models
{
    public class DocumentFullMetadata : DocumentMetadata
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        public Uri? PreviewUrl { get; set; }
    }
}