using KnowledgeMining.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
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
        private IList<ChatMessage> _messages = new List<ChatMessage>();

        public async Task<string> AskQuestionAboutDocument(string question, string document, CancellationToken ct = default)
        {
            var systemPrompt = @$" ## Source ##
{document}
## End ##

You answer needs to be a json object with the following format.
{{
    ""answer"": // the answer to the question, add a source reference to the end of each sentence. e.g. Apple is a fruit [reference1.pdf][reference2.pdf]. If no source available, put the answer as I don't know.
    ""thoughts"": // brief thoughts on how you came up with the answer, e.g. what sources you used, what you thought about, etc.
}}";

            var systemMessage = new SystemChatMessage(systemPrompt);

            var userMessage = new UserChatMessage(question);

            var messages = new List<ChatMessage>
            {
                systemMessage,
                userMessage
            };

            var answer = await _openAIClient.GetChatClient(_options.CompletionsDeployment).CompleteChatAsync(messages, new ChatCompletionOptions
            {
                MaxTokens = _options.MaxToken,
                Temperature = (float)0.7,
                ResponseFormat = ChatResponseFormat.JsonObject
            });

            var answerJson = answer.Value.Content;

            return string.Join(Environment.NewLine, answerJson.Select(ans => ans.Text));
        }
    }
}
