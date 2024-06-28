using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KnowledgeMining.Application.Common.Models
{
    public class ChatAnswer
    {
        [JsonPropertyName("answer")]
        public string Answer { get; set; } = string.Empty;
        [JsonPropertyName("excerpts")]
        public Excerpt[] Excerpts { get; set; } = [];
        [JsonPropertyName("thoughts")]
        public string Thoughts { get; set; } = string.Empty;
    }
}
