using System.Text.Json.Serialization;

namespace KnowledgeMining.Application.Common.Models
{
    public class Excerpt
    {
        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
        [JsonPropertyName("shortText")]
        public string ShortText { get; set; } = string.Empty;
    }
}
