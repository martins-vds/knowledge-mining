using System.Text.Json.Serialization;

namespace KnowledgeMining.Domain.Entities
{
    public class DocumentMetadata
    {
        [JsonPropertyName("metadata_storage_path")]
        public string? Id { get; set; }
        [JsonPropertyName("metadata_storage_name")]
        public string? Name { get; set; }
        [JsonPropertyName("content")]
        public string? Content { get; set; }
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

        public IDictionary<string, object?> ToDictionary()
        {
            return new Dictionary<string, object?>()
            {
                { ToLowerFirstChar(nameof(KeyPhrases)), KeyPhrases },
                { ToLowerFirstChar(nameof(Organizations)), Organizations },
                { ToLowerFirstChar(nameof(Persons)), Persons },
                { ToLowerFirstChar(nameof(Locations)), Locations },
                { ToLowerFirstChar(nameof(Topics)), Topics },
                { ToLowerFirstChar(nameof(Text)), Text },
                { ToLowerFirstChar(nameof(Date)), Date },
                { ToLowerFirstChar(nameof(Mission)), Mission },
                { ToLowerFirstChar(nameof(DocumentType)), DocumentType },
                { ToLowerFirstChar(nameof(MergedContent)), MergedContent }
            };
        }

        // Took from https://stackoverflow.com/questions/21755757/first-character-of-string-lowercase-c-sharp
        private string ToLowerFirstChar(string str)
        {
            if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
                return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str[1..];

            return str;
        }
    }
}
