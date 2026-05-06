using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Outliner.Services
{
    /// <summary>
    /// A rating record for one outline-generation run. Auto-computed completeness
    /// is the eval signal; UserRating/UserFeedback fields are null in test runs
    /// and populated by the UI when a human reviews the outline. Written to
    /// disk per-run as <input>.rating.json for later analysis.
    /// </summary>
    public sealed class OutlineRating
    {
        public string? InputFile { get; set; }
        public string? ModelId { get; set; }
        public string PromptVersion { get; set; } = OutlinePrompt.Version;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int CharacterCount { get; set; }
        public int SettingCount { get; set; }
        public int SceneCount { get; set; }
        public int ProblemCount { get; set; }

        public double CompletenessScore { get; set; }
        public string AutoRating { get; set; } = string.Empty;

        public string? UserRating { get; set; }
        public string? UserFeedback { get; set; }

        /// <summary>
        /// Computes a completeness score from the LLM response: fraction of
        /// string properties across all elements that are non-empty. Maps that
        /// score to a coarse thumbs_up / neutral / thumbs_down auto-rating.
        /// </summary>
        public static OutlineRating ComputeAuto(OnePassResponse response, string? inputFile, string? modelId)
        {
            int total = 0;
            int populated = 0;

            void Count(object? element)
            {
                if (element == null) return;
                foreach (var p in element.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (p.PropertyType != typeof(string)) continue;
                    total++;
                    var value = p.GetValue(element) as string;
                    if (!string.IsNullOrWhiteSpace(value)) populated++;
                }
            }

            Count(response.StoryOverview);
            foreach (var c in response.Characters ?? Enumerable.Empty<CharacterElement>()) Count(c);
            foreach (var s in response.Settings   ?? Enumerable.Empty<SettingElement>())   Count(s);
            foreach (var s in response.Scenes     ?? Enumerable.Empty<SceneElement>())     Count(s);
            foreach (var p in response.Problems   ?? Enumerable.Empty<ProblemElement>())   Count(p);

            double score = total == 0 ? 0.0 : (double)populated / total;
            string auto = score switch
            {
                >= 0.6 => "thumbs_up",
                >= 0.3 => "neutral",
                _      => "thumbs_down"
            };

            return new OutlineRating
            {
                InputFile = inputFile,
                ModelId = modelId,
                CharacterCount = response.Characters?.Count ?? 0,
                SettingCount   = response.Settings?.Count   ?? 0,
                SceneCount     = response.Scenes?.Count     ?? 0,
                ProblemCount   = response.Problems?.Count   ?? 0,
                CompletenessScore = Math.Round(score, 3),
                AutoRating = auto
            };
        }
    }

    /// <summary>
    /// Identifies the prompt revision used, so ratings can be tied to a specific
    /// prompt version for evaluation. Bump when changing OnePassSystemPrompt.md
    /// in a way that should affect the eval baseline.
    /// </summary>
    internal static class OutlinePrompt
    {
        public const string Version = "v2";
    }
}
