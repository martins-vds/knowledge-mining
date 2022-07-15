using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KnowledgeMining.Domain.Entities
{
    public class DocumentMetadata
    {
        [JsonPropertyName("metadata_storage_path")]
        public string? Id { get; set; }
        [JsonPropertyName("metadata_storage_name")]
        public string? Name { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("keyPhrases")]
        public IEnumerable<string>? KeyPhrases { get; set; }
        [JsonPropertyName("organizations")]
        public IEnumerable<string>? Organizations { get; set; }
        [JsonPropertyName("persons")]
        public IEnumerable<string>? Persons { get; set; }
        [JsonPropertyName("locations")]
        public IEnumerable<string>? Locations { get; set; }
        [JsonPropertyName("topics")]
        public IEnumerable<string>? Topics { get; set; }
        [JsonPropertyName("text")]
        public IEnumerable<string>? Text { get; set; }
        [JsonPropertyName("layoutText")]
        public IEnumerable<string>? LayoutText { get; set; }
        [JsonPropertyName("imageTags")]
        public IEnumerable<string>? ImageTags { get; set; }
        [JsonPropertyName("date")]
        public string? Date { get; set; }
        [JsonPropertyName("mission")]
        public string? Mission { get; set; }
        [JsonPropertyName("documentType")]
        public string? DocumentType { get; set; }
        [JsonPropertyName("merged_content")]
        public string? MergedContent { get; set; }
    }
}
