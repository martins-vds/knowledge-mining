using Azure.AI.OpenAI;
using KnowledgeMining.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KnowledgeMining.Infrastructure.Services.OpenAI
{
    public class OpenAIService(OpenAIClient client, IOptions<OpenAIOptions> options) : IChatService
    {
        private readonly OpenAIClient _openAIClient = client;
        private readonly OpenAIOptions _options = options.Value;
        private IList<ChatRequestMessage> _messages = new List<ChatRequestMessage>();

        public async Task<string> AskQuestionAboutDocument(string question, string document, CancellationToken ct = default)
        {
            var systemPrompt = @$" ## Source ##
{document}
## End ##

You answer needs to be a valid json object with the following format.
{{
    ""answer"": ""the answer to the question, add a source reference to the end of each sentence. e.g. Apple is a fruit [reference1.pdf][reference2.pdf]. If no source available, put the answer as I don't know."",
    ""thoughts"": ""brief thoughts on how you came up with the answer, e.g. what sources you used, what you thought about, etc.""
}}";

            ChatCompletionsOptions chatCompletions = new ChatCompletionsOptions()
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

            var answer = await _openAIClient.GetChatCompletionsAsync(chatCompletions);

            var answerJson = answer.Value.Choices.FirstOrDefault()?.Message.Content ?? throw new InvalidOperationException("Failed to get search query");

            var answerObject = JsonSerializer.Deserialize<JsonElement>(answerJson);

            var ans = answerObject.GetProperty("answer").GetString() ?? throw new InvalidOperationException("Failed to get answer");
            var thoughts = answerObject.GetProperty("thoughts").GetString() ?? throw new InvalidOperationException("Failed to get thoughts");

            return ans;
        }
    }
}
