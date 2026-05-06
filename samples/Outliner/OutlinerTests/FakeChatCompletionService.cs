using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace OutlinerTests
{
    /// <summary>
    /// Test double for IChatCompletionService that returns a canned response
    /// instead of calling OpenAI. Keeps stubbed pipeline tests deterministic
    /// and free of API-key/quota/network dependence.
    /// </summary>
    internal sealed class FakeChatCompletionService : IChatCompletionService
    {
        private readonly string _content;

        public FakeChatCompletionService(string content) => _content = content;

        public IReadOnlyDictionary<string, object?> Attributes { get; }
            = new Dictionary<string, object?>();

        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            var msg = new ChatMessageContent(AuthorRole.Assistant, _content)
            {
                ModelId = "stub"
            };
            return Task.FromResult<IReadOnlyList<ChatMessageContent>>(new[] { msg });
        }

        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, _content) { ModelId = "stub" };
        }
    }
}
