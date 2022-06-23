// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace KnowledgeMining.Blazor.Models
{
    public class DocumentMetadata
    {
        [JsonPropertyName("metadata_storage_path")]
        public string Id { get; set; }
        [JsonPropertyName("metadata_storage_name")]
        public string Name { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("keyPhrases")]
        public IEnumerable<string> KeyPhrases { get; set; }
        [JsonPropertyName("organizations")]
        public IEnumerable<string> Organizations { get; set; }
        [JsonPropertyName("persons")]
        public IEnumerable<string> Persons { get; set; }
        [JsonPropertyName("locations")]
        public IEnumerable<string> Locations { get; set; }
        [JsonPropertyName("text")]
        public IEnumerable<string> Text { get; set; }
        [JsonPropertyName("layoutText")]
        public IEnumerable<string> LayoutText { get; set; }
        [JsonPropertyName("imageTags")]
        public IEnumerable<string> ImageTags { get; set; }
        [JsonPropertyName("date")]
        public IEnumerable<string> Date { get; set; }
        [JsonPropertyName("mission")]
        public IEnumerable<string> Mission { get; set; }
    }
}