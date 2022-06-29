// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace KnowledgeMining.Blazor.Services.Search.Models
{
    public class EntityMapData
    {
        [JsonPropertyName("nodes")]
        public IList<EntityMapNode> Nodes { get; set; }
        [JsonPropertyName("links")]
        public IList<EntityMapLink> Links { get; set; }

        public EntityMapData()
        {
            this.Nodes = new List<EntityMapNode>();
            this.Links = new List<EntityMapLink>();
        }
    }
}