using System;
using System.Collections.Generic;

namespace StoryCADCritter
{
    /// <summary>
    /// Aggregate token usage and USD cost across all LLM calls in a critique run.
    /// Cost is computed against a hardcoded price table; figures are a snapshot,
    /// not authoritative. Refresh from https://openai.com/api/pricing/.
    /// </summary>
    public sealed class CritiqueCost
    {
        public string ModelId { get; set; } = string.Empty;
        public int LlmCallCount { get; set; }
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public int TotalTokens => InputTokens + OutputTokens;
        public decimal InputCostUsd { get; set; }
        public decimal OutputCostUsd { get; set; }
        public decimal TotalCostUsd => InputCostUsd + OutputCostUsd;
        public string PriceTableValidAsOf { get; set; } = ModelPriceTable.ValidAsOf;
    }

    internal static class ModelPriceTable
    {
        public const string ValidAsOf = "2026-05";

        // (inputPerMillion, outputPerMillion)
        private static readonly Dictionary<string, (decimal Input, decimal Output)> Prices =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["gpt-4o"]        = (2.50m, 10.00m),
                ["gpt-4o-mini"]   = (0.15m,  0.60m),
                ["gpt-4-turbo"]   = (10.00m, 30.00m),
                ["gpt-4"]         = (30.00m, 60.00m),
                ["gpt-3.5-turbo"] = (0.50m,  1.50m),
            };

        public static (decimal InputCost, decimal OutputCost) Compute(string modelId, int inputTokens, int outputTokens)
        {
            if (!Prices.TryGetValue(modelId, out var rate))
                return (0m, 0m);
            return (
                inputTokens  * rate.Input  / 1_000_000m,
                outputTokens * rate.Output / 1_000_000m);
        }
    }
}
