// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace KnowledgeMining.Domain.Entities
{
    public class EntityMap
    {
        [JsonPropertyName("nodes")]
        public IList<EntityMapNode> Nodes { get; set; }
        [JsonPropertyName("links")]
        public IList<EntityMapLink> Links { get; set; }

        public EntityMap()
        {
            this.Nodes = new List<EntityMapNode>();
            this.Links = new List<EntityMapLink>();
        }
    }
}