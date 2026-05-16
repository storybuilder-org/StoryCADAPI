using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Outliner.Services
{
    /// <summary>
    /// Service responsible for Phase 1: Analyzing narrative prose using LLM.
    /// Takes raw story text and produces structured story elements in a single pass.
    /// </summary>
    public class ProseAnalyzer
    {
        public string? LastRawResponse { get; private set; }
        public OutlineCost? LastCost { get; private set; }

        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;
        private readonly OpenAIPromptExecutionSettings _executionSettings;
        private readonly ChatHistory _chatHistory;

        // Token limit for the model (configurable)
        private const int MaxTokensPerCall = 1000000;
        private const int TokensPerCharacterEstimate = 4; // Rough estimate

        public ProseAnalyzer(Kernel kernel, IChatCompletionService chatService)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));

            _chatHistory = new ChatHistory();
            _executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = 16000,
                Temperature = 0.7,
                TopP = 0.9,
                ResponseFormat = "json_object"
            };
        }

        /// <summary>
        /// Analyzes narrative prose in a single pass to extract all story elements.
        /// This is the core of Phase 1 processing.
        /// </summary>
        /// <param name="proseText">The raw story text to analyze</param>
        /// <param name="promptPath">Optional path to custom prompt file</param>
        /// <returns>Structured story elements with cross-references</returns>
        public async Task<OnePassResponse> AnalyzeProse(string proseText, string? promptPath = null)
        {
            if (string.IsNullOrWhiteSpace(proseText))
                throw new ArgumentException("Prose text cannot be empty", nameof(proseText));

            // Validate prose length
            if (!ValidateProseLength(proseText))
            {
                throw new InvalidOperationException(
                    $"Prose text is too long. Estimated tokens: {EstimateTokenCount(proseText)}, " +
                    $"Maximum: {MaxTokensPerCall}");
            }

            // Load system prompt
            var systemPrompt = LoadSystemPrompt(promptPath);

            // Prepare chat for single-pass analysis
            _chatHistory.Clear();
            _chatHistory.AddSystemMessage(systemPrompt);
            _chatHistory.AddUserMessage(
                $"Process the following story prose and generate a complete outline:\n\n{proseText}");

            try
            {
                // Execute LLM call
                var result = await _chatService.GetChatMessageContentAsync(
                    _chatHistory,
                    _executionSettings,
                    _kernel);

                LastRawResponse = result.Content;
                LastCost = ExtractCost(result);

                if (string.IsNullOrWhiteSpace(result.Content))
                {
                    throw new InvalidOperationException("LLM returned empty response");
                }

                // Parse and validate response
                var response = ParseResponse(result.Content);
                ValidateResponse(response);

                return response;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse LLM response as JSON: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to analyze prose: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates that the prose text is within acceptable token limits.
        /// </summary>
        /// <param name="prose">The prose text to validate</param>
        /// <returns>True if within limits, false otherwise</returns>
        public bool ValidateProseLength(string prose)
        {
            if (string.IsNullOrWhiteSpace(prose))
                return false;

            var estimatedTokens = EstimateTokenCount(prose);
            return estimatedTokens <= MaxTokensPerCall;
        }

        /// <summary>
        /// Estimates the token count for a given text.
        /// Uses a rough heuristic of 1 token per 4 characters.
        /// </summary>
        /// <param name="text">Text to estimate tokens for</param>
        /// <returns>Estimated token count</returns>
        public int EstimateTokenCount(string text)
        {
            return text.Length / TokensPerCharacterEstimate;
        }

        /// <summary>
        /// Pre-flight worst-case cost estimate for analyzing the given prose.
        /// Output tokens are estimated at the configured MaxTokens ceiling, so
        /// the returned figure is an upper bound — actual cost (LastCost after
        /// AnalyzeProse) is typically significantly lower. Useful for letting
        /// the UI warn before a long round-trip.
        /// </summary>
        public OutlineCost EstimateCost(string proseText, string? modelId = null)
        {
            var actualModel = modelId
                ?? Environment.GetEnvironmentVariable("OPENAI_MODEL")
                ?? "gpt-4o";
            var inputTokens  = EstimateTokenCount(proseText);
            var outputTokens = _executionSettings.MaxTokens ?? 16000;
            var (inputCost, outputCost) = ModelPriceTable.Compute(actualModel, inputTokens, outputTokens);

            return new OutlineCost
            {
                ModelId = actualModel,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                InputCostUsd = inputCost,
                OutputCostUsd = outputCost,
                PriceTableValidAsOf = ModelPriceTable.ValidAsOf
            };
        }

        /// <summary>
        /// Loads the system prompt from file or returns default.
        /// </summary>
        /// <param name="customPath">Optional custom prompt file path</param>
        /// <returns>The system prompt text</returns>
        public string LoadSystemPrompt(string? customPath = null)
        {
            string promptPath = customPath;

            if (string.IsNullOrWhiteSpace(promptPath))
            {
                // Default to OnePassSystemPrompt.md in various possible locations
                var possiblePaths = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "Prompts", "OnePassSystemPrompt.md"),
                    Path.Combine(AppContext.BaseDirectory, "OnePassSystemPrompt.md"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Prompts", "OnePassSystemPrompt.md"),
                    Path.Combine(Directory.GetCurrentDirectory(), "OnePassSystemPrompt.md")
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        promptPath = path;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(promptPath) || !File.Exists(promptPath))
            {
                // Return embedded default prompt if file not found
                return GetDefaultPrompt();
            }

            return File.ReadAllText(promptPath);
        }

        /// <summary>
        /// Parses the JSON response from the LLM into a OnePassResponse object.
        /// </summary>
        private OnePassResponse ParseResponse(string jsonContent)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            var response = JsonSerializer.Deserialize<OnePassResponse>(jsonContent, options);
            if (response == null)
            {
                throw new InvalidOperationException("Failed to deserialize LLM response");
            }

            return response;
        }

        /// <summary>
        /// Validates that the response contains at least the minimum required elements.
        /// </summary>
        private void ValidateResponse(OnePassResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            // Story overview is required
            if (response.StoryOverview == null)
            {
                throw new InvalidOperationException("Response missing story overview");
            }

            // At least some content should be present
            bool hasContent =
                (response.Characters != null && response.Characters.Count > 0) ||
                (response.Settings != null && response.Settings.Count > 0) ||
                (response.Scenes != null && response.Scenes.Count > 0) ||
                (response.Problems != null && response.Problems.Count > 0);

            if (!hasContent)
            {
                throw new InvalidOperationException("Response contains no story elements");
            }

            // Validate GUIDs are present for all elements
            ValidateGuids(response);
        }

        /// <summary>
        /// Validates that all elements have GUIDs for cross-referencing.
        /// LLMs frequently emit strings that look like GUIDs but contain non-hex
        /// characters; in that case we regenerate a fresh GUID and remap every
        /// cross-reference so cast/protagonist/setting links survive.
        /// </summary>
        private void ValidateGuids(OnePassResponse response)
        {
            var remap = new Dictionary<string, string>();

            EnsureValidGuids(response.Characters, c => c.Guid, (c, g) => c.Guid = g, remap);
            EnsureValidGuids(response.Settings,   s => s.Guid, (s, g) => s.Guid = g, remap);
            EnsureValidGuids(response.Scenes,     s => s.Guid, (s, g) => s.Guid = g, remap);
            EnsureValidGuids(response.Problems,   p => p.Guid, (p, g) => p.Guid = g, remap);

            if (remap.Count == 0) return;

            if (response.Scenes != null)
            {
                foreach (var scene in response.Scenes)
                {
                    scene.Protagonist        = Remap(scene.Protagonist, remap);
                    scene.Antagonist         = Remap(scene.Antagonist, remap);
                    scene.ViewpointCharacter = Remap(scene.ViewpointCharacter, remap);
                    scene.Setting            = Remap(scene.Setting, remap);
                    if (scene.Cast != null)
                    {
                        for (int i = 0; i < scene.Cast.Count; i++)
                            scene.Cast[i] = Remap(scene.Cast[i], remap) ?? scene.Cast[i];
                    }
                }
            }

            if (response.Problems != null)
            {
                foreach (var problem in response.Problems)
                {
                    problem.Protagonist = Remap(problem.Protagonist, remap);
                    problem.Antagonist  = Remap(problem.Antagonist, remap);
                }
            }
        }

        private static void EnsureValidGuids<T>(
            List<T>? items,
            Func<T, string> get,
            Action<T, string> set,
            Dictionary<string, string> remap)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                var current = get(item);
                if (string.IsNullOrWhiteSpace(current) || !Guid.TryParse(current, out _))
                {
                    var fresh = Guid.NewGuid().ToString();
                    if (!string.IsNullOrWhiteSpace(current))
                        remap[current] = fresh;
                    set(item, fresh);
                }
            }
        }

        private static string? Remap(string? value, Dictionary<string, string> remap)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return remap.TryGetValue(value, out var fresh) ? fresh : value;
        }

        /// <summary>
        /// Reads token usage from the SK result's Usage metadata (the OpenAI
        /// connector populates an OpenAI.Chat.ChatTokenUsage there). Reflection
        /// keeps us decoupled from a specific OpenAI SDK version.
        /// </summary>
        private static OutlineCost ExtractCost(Microsoft.SemanticKernel.ChatMessageContent result)
        {
            var modelId = result.ModelId
                ?? Environment.GetEnvironmentVariable("OPENAI_MODEL")
                ?? "gpt-4o";

            int inputTokens = 0;
            int outputTokens = 0;

            try
            {
                if (result.Metadata != null
                    && result.Metadata.TryGetValue("Usage", out var usageObj)
                    && usageObj != null)
                {
                    var t = usageObj.GetType();
                    inputTokens  = (t.GetProperty("InputTokenCount")?.GetValue(usageObj) as int?) ?? 0;
                    outputTokens = (t.GetProperty("OutputTokenCount")?.GetValue(usageObj) as int?) ?? 0;
                }
            }
            catch
            {
                // Best-effort; unknown metadata shape just yields zero tokens
            }

            var (inputCost, outputCost) = ModelPriceTable.Compute(modelId, inputTokens, outputTokens);

            return new OutlineCost
            {
                ModelId = modelId,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                InputCostUsd = inputCost,
                OutputCostUsd = outputCost,
                PriceTableValidAsOf = ModelPriceTable.ValidAsOf
            };
        }

        /// <summary>
        /// Returns the default embedded system prompt if no file is available.
        /// </summary>
        private string GetDefaultPrompt()
        {
            return @"# One-Pass Analysis System Prompt

You are provided with the full prose of a short story. Your task is to process the text in a single pass and build a comprehensive outline. You will maintain four internal lists as you scan through the text:

1. **CharactersList**: Every character identified, with their attributes
2. **SettingsList**: Every unique setting (location or time change)
3. **ScenesList**: Each continuous scene in the prose
4. **ProblemsList**: All conflicts (internal and external) detected

## Output Format

You must output a single JSON object with the following structure:
```json
{
  ""story_overview"": {
    ""title"": ""extracted title"",
    ""author"": ""extracted author"",
    ""premise"": ""generated premise if story problem is identified""
  },
  ""characters"": [...],
  ""settings"": [...],
  ""scenes"": [...],
  ""problems"": [...]
}
```

## Processing Instructions

Read the entire prose in one continuous pass. As you scan the text:

### 1. Extract Story Overview
- Identify the Title and Author from the prose
- If not found, use ""Working Title"" and ""Unknown Author""
- Generate a premise statement if a clear story problem exists

### 2. Detect Characters
For each character encountered:
- Generate a unique GUID string
- Extract all available attributes
- Note relationships to other characters

### 3. Identify Settings
Create a new setting when location or time shifts significantly.
Generate a unique GUID for each setting.

### 4. Track Scenes
A scene is a continuous dramatic unit with:
- Characters interacting
- A specific setting
- A conflict or purpose
Generate a unique GUID for each scene.

### 5. Identify Problems
Detect conflicts, challenges, and story questions.
Generate a unique GUID for each problem.

## Cross-References
When elements reference each other (e.g., a scene's cast, a problem's protagonist), use the GUID strings you generated for those elements.

## Important
- Generate all GUIDs as string format (e.g., ""550e8400-e29b-41d4-a716-446655440000"")
- Maintain consistency in GUID references
- Extract actual content from the prose, don't invent details
- If information is not available, use empty string """"
";
        }
    }
}