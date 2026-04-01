using System;
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
        /// </summary>
        private void ValidateGuids(OnePassResponse response)
        {
            if (response.Characters != null)
            {
                foreach (var character in response.Characters)
                {
                    if (string.IsNullOrWhiteSpace(character.Guid))
                    {
                        // Generate a GUID if missing
                        character.Guid = Guid.NewGuid().ToString();
                    }
                }
            }

            if (response.Settings != null)
            {
                foreach (var setting in response.Settings)
                {
                    if (string.IsNullOrWhiteSpace(setting.Guid))
                    {
                        setting.Guid = Guid.NewGuid().ToString();
                    }
                }
            }

            if (response.Scenes != null)
            {
                foreach (var scene in response.Scenes)
                {
                    if (string.IsNullOrWhiteSpace(scene.Guid))
                    {
                        scene.Guid = Guid.NewGuid().ToString();
                    }
                }
            }

            if (response.Problems != null)
            {
                foreach (var problem in response.Problems)
                {
                    if (string.IsNullOrWhiteSpace(problem.Guid))
                    {
                        problem.Guid = Guid.NewGuid().ToString();
                    }
                }
            }
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