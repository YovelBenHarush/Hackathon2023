using AI.Dev.OpenAI.GPT;
using Azure;
using Azure.AI.OpenAI;

namespace Microsoft.DataSec.DspmForDatabases.Tools.HackathonScanEngineRunner
{
    public class AIClient
    {
        private const int TokenLimit = 16384;
        private const int MaxResponseTokens = 1000;

        private readonly string _key = "<>";
        private readonly string _endpoint = "https://emeaopenai.azure-api.net/";
        private readonly string _deployment = "gpt-35-turbo-16k";
        private readonly OpenAIClient _client;
        private readonly ChatCompletionsOptions _defaultOptions;
        private readonly CompletionsOptions _defaultCompletionsOptions;

        private int _tokensUsed = 0;

        public AIClient(IList<ChatMessage> defaultInstructions)
        {
            _client = new OpenAIClient(new Uri(_endpoint), new AzureKeyCredential(_key));
            _defaultOptions = GetDefaultChatCompletionsOptions(defaultInstructions);
            _defaultCompletionsOptions = GetDefaultCompletionsOptions(defaultInstructions);
        }

        public async Task<ChatCompletions> GetChatCompletionsAsync(List<(ChatRole Role, string Message)> messages, CancellationToken cancellationToken)
        {
            try
            {
                for (int i = messages.Count - 1; i >= 0; i--)
                {
                    var msg = messages[i];
                    _tokensUsed += GPT3Tokenizer.Encode(msg.Message).Count();

                    if (_tokensUsed > TokenLimit)
                    {
                        messages.RemoveRange(0, i);
                        break;
                    }
                }

                var chatCompletionsOptions = _defaultOptions;
                var chatMessages = messages.Select(m => new ChatMessage(m.Role, m.Message)).ToList();
                chatMessages.ForEach(chatCompletionsOptions.Messages.Add);

                Response<ChatCompletions> response = await _client.GetChatCompletionsAsync(
                deploymentOrModelName: _deployment,
                chatCompletionsOptions,
                cancellationToken);

                chatMessages.ForEach(m => chatCompletionsOptions.Messages.Remove(m));

                return response.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed calculating classifications", ex);
                return null;
            }
        }

        public async Task<Completions> GetCompletionsAsync(List<string> messages, CancellationToken cancellationToken)
        {
            try
            {
                for (int i = messages.Count - 1; i >= 0; i--)
                {
                    var msg = messages[i];
                    _tokensUsed += GPT3Tokenizer.Encode(msg).Count();

                    if (_tokensUsed > TokenLimit)
                    {
                        messages.RemoveRange(0, i);
                        break;
                    }
                }

                var completionsOptions = _defaultCompletionsOptions;
                messages.ForEach(completionsOptions.Prompts.Add);

                Response<Completions> response = await _client.GetCompletionsAsync(
                deploymentOrModelName: _deployment,
                completionsOptions,
                cancellationToken);

                messages.ForEach(m => completionsOptions.Prompts.Remove(m));

                return response.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed calculating classifications", ex);
                return null;
            }
        }

        private ChatCompletionsOptions GetDefaultChatCompletionsOptions(IList<ChatMessage> systemMessages)
        {
            foreach (var msg in systemMessages)
            {
                _tokensUsed += GPT3Tokenizer.Encode(msg.Content).Count();
            }

            _tokensUsed += 3 * MaxResponseTokens;

            return new ChatCompletionsOptions(systemMessages)
            {
                Temperature = 0.0f,
                MaxTokens = MaxResponseTokens,
            };
        }

        private CompletionsOptions GetDefaultCompletionsOptions(IList<ChatMessage> systemMessages)
        {
            foreach (var msg in systemMessages)
            {
                _tokensUsed += GPT3Tokenizer.Encode(msg.Content).Count();
            }

            _tokensUsed += 3 * MaxResponseTokens;

            return new CompletionsOptions(systemMessages.Select(m => m.Content))
            {
                Temperature = 0.0f,
                MaxTokens = MaxResponseTokens,
            };
        }
    }
}
