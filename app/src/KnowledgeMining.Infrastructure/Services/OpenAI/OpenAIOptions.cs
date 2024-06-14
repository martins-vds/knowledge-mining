using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Infrastructure.Services.OpenAI
{
    public class OpenAIOptions
    {
        public const string OpenAI = nameof(OpenAI);

        public Uri Endpoint { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        public string EmbeddingDeployment { get; set; } = string.Empty;
        public string CompletionsDeployment { get; set; } = string.Empty;
        public int MaxToken { get; set; } = 4000;
    }
}
