// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace KnowledgeMining.Domain.Entities
{
    public class EntityMapLink
    {
        [JsonPropertyName("source")]
        public int Source { get; set; }
        [JsonPropertyName("target")]
        public int Target { get; set; }
        [JsonPropertyName("distance")]
        public int Distance { get; set; }
    }
}