// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace KnowledgeMining.Blazor.Models
{
    public class DocumentFullMetadata : DocumentMetadata
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}