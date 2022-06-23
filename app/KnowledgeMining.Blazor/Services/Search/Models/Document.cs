// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace KnowledgeMining.UI.Services.Search.Models
{
    public class Document
    {
        [JsonPropertyName("metadata_storage_path")]
        public string Id { get; set; }
        [JsonPropertyName("metadata_storage_name")]
        public string Name { get; set; }
        public string Content { get; set; }
        public IEnumerable<string> KeyPhrases { get; set; }
        public IEnumerable<string> Organizations { get; set; }
        public IEnumerable<string> Persons { get; set; }
        public IEnumerable<string> Locations { get; set; }
        public string Text { get; set; }
        public string LayoutText { get; set; }
        public IEnumerable<string> ImageTags { get; set; }
        public IEnumerable<string> Date { get; set; }
        public IEnumerable<string> Mission { get; set; }
    }
}