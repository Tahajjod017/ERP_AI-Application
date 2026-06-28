using GCTL.Core.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;

namespace GCTL.Service.AI
{
    public class OpenAIService : IAIService
    {
        private readonly ChatClient _chatClient;
        private readonly ILogger<OpenAIService> _logger;

        private const string SystemPrompt = @"
            You are an intelligent ERP assistant.
            You help employees with HR queries, job information, 
            attendance, payroll, and general ERP questions.
            Always be professional and helpful.
            Never make up data — only use the context provided.
            Respond in the same language the user writes in.";

        public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
        {
            _logger = logger;

            // Here is Open AI API Configuration System
            var apiKey = configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException(
                    "OpenAI API Key not found in User Secrets.");

            var model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
            var client = new OpenAIClient(apiKey);
            _chatClient = client.GetChatClient(model);
        }

        public async Task<string> AskAsync(string question, string context = "")
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(SystemPrompt)
                };

                if (!string.IsNullOrEmpty(context))
                    messages.Add(new SystemChatMessage($"Relevant data:\n{context}"));

                messages.Add(new UserChatMessage(question));

                var response = await _chatClient.CompleteChatAsync(messages);
                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI AskAsync failed");
                return "Sorry, I could not process your request. Please try again.";
            }
        }

        public async Task<string> SummarizeAsync(string data)
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(SystemPrompt),
                    new UserChatMessage($"Summarize this ERP data clearly:\n\n{data}")
                };

                var response = await _chatClient.CompleteChatAsync(messages);
                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI SummarizeAsync failed");
                return "Summary could not be generated.";
            }
        }

        public async Task<string> AnalyzeAsync(string data, string instruction)
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(SystemPrompt),
                    new SystemChatMessage($"Data:\n{data}"),
                    new UserChatMessage(instruction)
                };

                var response = await _chatClient.CompleteChatAsync(messages);
                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI AnalyzeAsync failed");
                return "Analysis could not be completed.";
            }
        }

        public async IAsyncEnumerable<string> StreamAsync(
            string question, string context = "")
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(SystemPrompt)
            };

            if (!string.IsNullOrEmpty(context))
                messages.Add(new SystemChatMessage($"Relevant data:\n{context}"));

            messages.Add(new UserChatMessage(question));

            await foreach (var update in _chatClient.CompleteChatStreamingAsync(messages))
            {
                foreach (var part in update.ContentUpdate)
                {
                    if (!string.IsNullOrEmpty(part.Text))
                        yield return part.Text;
                }
            }
        }
    }
}