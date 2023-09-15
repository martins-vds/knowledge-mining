// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace KnowledgeMining.Domain.Entities
{
    public class EntityMapNode
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("color")]
        public string? Color { get; set; }
        [JsonPropertyName("layer")]
        public int Layer { get; set; }
        [JsonPropertyName("cornerStone")]
        public int CornerStone { get; set; }
    }
}