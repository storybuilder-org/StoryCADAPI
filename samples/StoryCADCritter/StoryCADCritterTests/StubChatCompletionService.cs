using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace StoryCADCritterTests;

/// <summary>
/// Test double for IChatCompletionService. Each call invokes Respond with the
/// last user message of the ChatHistory and returns its result. Tracks call
/// count so tests can assert short-circuit cases never invoked the LLM.
/// </summary>
internal sealed class StubChatCompletionService : IChatCompletionService
{
    private int _callCount;
    public int CallCount => _callCount;
    public Func<string, string> Respond { get; set; } = _ => DefaultValidJson;

    public const string DefaultValidJson = """
        {
          "elementUuid": "00000000-0000-0000-0000-000000000000",
          "elementType": "Element",
          "elementName": "Stub",
          "strengths": [{"keyQuestion": "stub", "finding": "stub strength"}],
          "concerns": [],
          "questionsForAuthor": []
        }
        """;

    public IReadOnlyDictionary<string, object?> Attributes { get; }
        = new Dictionary<string, object?>();

    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _callCount);
        var lastUser = chatHistory.LastOrDefault(m => m.Role == AuthorRole.User)?.Content ?? string.Empty;
        var content = Respond(lastUser);
        var msg = new ChatMessageContent(AuthorRole.Assistant, content) { ModelId = "stub" };
        return Task.FromResult<IReadOnlyList<ChatMessageContent>>(new[] { msg });
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield return new StreamingChatMessageContent(AuthorRole.Assistant, Respond(string.Empty)) { ModelId = "stub" };
    }
}
