using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StoryCADCritter
{
    /// <summary>
    /// Per-element LLM response — mirrors the JSON schema pinned in
    /// Prompts/CritiquePrompt.md ("Output" section). Property names use
    /// camelCase to match the schema; deserialization is case-insensitive.
    /// </summary>
    public sealed class CritiqueElementResponse
    {
        public string ElementUuid { get; set; } = string.Empty;
        public string ElementType { get; set; } = string.Empty;
        public string ElementName { get; set; } = string.Empty;
        public List<CritiqueFinding> Strengths { get; set; } = new();
        public List<CritiqueFinding> Concerns { get; set; } = new();
        public List<string> QuestionsForAuthor { get; set; } = new();
    }

    public sealed class CritiqueFinding
    {
        public string KeyQuestion { get; set; } = string.Empty;
        public string Finding { get; set; } = string.Empty;
    }

    /// <summary>
    /// Per-element result the orchestrator hands to the report writer. Captures
    /// the LLM's findings on success, or the raw response + error on a defensive
    /// fall-back so nothing the LLM said is silently dropped.
    /// </summary>
    public sealed class ElementCritique
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ElementType { get; set; } = string.Empty;

        /// <summary>Key questions actually sent to the LLM for this element (for transparency).</summary>
        public List<(string Topic, string Question)> KeyQuestions { get; set; } = new();

        /// <summary>How deeply this element was asked to be critiqued.</summary>
        public string CritiqueMode { get; set; } = string.Empty;

        /// <summary>Short explanation of the structural signals behind the critique mode.</summary>
        public string CritiqueFocus { get; set; } = string.Empty;

        /// <summary>Populated when parsing succeeded. Null when we fell back to raw text or hit a hard error.</summary>
        public CritiqueElementResponse? Parsed { get; set; }

        /// <summary>Raw LLM text — kept on parse failure so the report can surface it under a "couldn't parse cleanly" banner.</summary>
        public string? RawResponse { get; set; }

        /// <summary>Error message if the LLM call itself failed (after retries).</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Token usage + USD cost for this element's LLM call. Null when the call didn't happen.</summary>
        public CritiqueCost? Cost { get; set; }

        public bool ParseFailed => Parsed == null && string.IsNullOrEmpty(ErrorMessage) && !string.IsNullOrEmpty(RawResponse);
        public bool CallFailed => !string.IsNullOrEmpty(ErrorMessage);
        public bool Succeeded => Parsed != null;
    }

    /// <summary>
    /// Full result of a critique run. Exit-code mapping (see Program.cs):
    ///   0 = full success            (no errors, LLM walk completed)
    ///   1 = partial                 (LLM walk completed, but at least one element failed or fell back)
    ///   2 = hard failure            (no outline, no LLM access, etc.)
    ///   ShortCircuit                (outline too thin to critique — emitted a structural report instead)
    /// </summary>
    public sealed class CritiqueRunResult
    {
        public string OutlinePath { get; set; } = string.Empty;
        public string ReportPath { get; set; } = string.Empty;
        public string CostsPath { get; set; } = string.Empty;
        public string RawPath { get; set; } = string.Empty;
        public List<ElementCritique> ElementCritiques { get; set; } = new();
        public string StructuralOrientation { get; set; } = string.Empty;
        public bool ShortCircuited { get; set; }
        public string? ShortCircuitReason { get; set; }
        public string? HardFailureMessage { get; set; }
        public CritiqueCost Cost { get; set; } = new();

        public bool HardFailed => !string.IsNullOrEmpty(HardFailureMessage);
        public bool HasPerElementErrors =>
            ElementCritiques.Exists(e => e.CallFailed || e.ParseFailed);
    }

    public sealed record CritiqueProgress(string Status, int Done, int Total);

    /// <summary>
    /// JSON serializer options used for both deserializing LLM responses and
    /// (optionally) round-tripping CritiqueElementResponse to disk in tests.
    /// </summary>
    internal static class CritiqueJson
    {
        public static readonly System.Text.Json.JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
            ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
        };
    }
}
