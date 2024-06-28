using Azure.AI.OpenAI;
using KnowledgeMining.Application.Common.Interfaces;
using KnowledgeMining.Application.Common.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KnowledgeMining.Infrastructure.Services.OpenAI
{
    public class OpenAIService(OpenAIClient client, IChunkSearchService searchService, IOptions<OpenAIOptions> options) : IChatService
    {
        private readonly OpenAIClient _openAIClient = client;
        private readonly OpenAIOptions _options = options.Value;
        private readonly IChunkSearchService _searchService = searchService;
        private IList<ChatRequestMessage> _messages = new List<ChatRequestMessage>();

        public async Task<ChatAnswer> AskQuestionAboutDocument(string question, string content, string documentId = "", CancellationToken ct = default)
        {            
            var questionEmbedding = await _openAIClient.GetEmbeddingsAsync(new EmbeddingsOptions(_options.EmbeddingDeployment, [question]), ct);

            var chunks = await _searchService.QueryDocumentChuncksAsync(questionEmbedding.Value.Data[0].Embedding.ToArray(), documentId, ct);

            var systemPrompt = CreateSystemPromptFromChunks(chunks);

            var chatCompletions = new ChatCompletionsOptions()
            {
                DeploymentName = _options.CompletionsDeployment,
                Temperature = (float)0.7,
                MaxTokens = _options.MaxToken,
                NucleusSamplingFactor = (float)0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
                Messages =
                {
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestUserMessage(question)
                }
            };

            var answer = await _openAIClient.GetChatCompletionsAsync(chatCompletions, ct);

            var answerJson = answer.Value.Choices.FirstOrDefault()?.Message.Content ?? throw new InvalidOperationException("Failed to get search query");

            try
            {
                return JsonSerializer.Deserialize<ChatAnswer>(answerJson) ?? new ChatAnswer() {
                    Answer = "Failed to get answer",
                };
            }
            catch (JsonException)
            {
                return new ChatAnswer() {
                    Answer = "Failed to get answer",
                };
            }
        }

        private string CreateSystemPromptFromChunks(IEnumerable<string> excerpts)
        {
            var answerTemplate = @"
{
    ""answer"": ""the answer to the question. If no excerpt available, put the answer as I don't know."",
    ""excerpts"": [
        {""source"": ""source excerpt e.g. [reference 1]"", ""shortText"": ""excerpt text up to 200 characters""},
        {""source"": ""source excerpt e.g. [reference 2]"", ""shortText"": ""excerpt text up to 200 characters""}
    ],
    ""thoughts"": ""brief thoughts on how you came up with the answer, e.g. what excerpts you used, what you thought about, etc.""
}
";

            var sb = new StringBuilder();
            sb.AppendLine("## Document Excerpts ##");
            
            for (int i = 0; i < excerpts.Count(); i++)
            {
                sb.AppendLine($"### Excerpt {i + 1} ###");
                sb.AppendLine(excerpts.ElementAt(i));
                sb.AppendLine($"### End Excerpt {i + 1} ###");
            }

            sb.AppendLine("## End Document Excerpts ##");
            sb.AppendLine();
            sb.AppendLine("You answer needs to be a valid json object with the following format.");
            sb.Append(answerTemplate);

            return sb.ToString();
        }
    }
}
