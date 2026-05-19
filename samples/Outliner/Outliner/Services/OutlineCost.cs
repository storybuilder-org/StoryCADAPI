using System;
using System.Collections.Generic;
using System.Linq;

namespace Outliner.Services
{
    /// <summary>
    /// Token usage and USD cost for a single LLM round-trip. Cost is computed
    /// against a hardcoded price table; figures are a snapshot, not authoritative.
    /// </summary>
    public sealed class OutlineCost
    {
        public string ModelId { get; set; } = string.Empty;
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public int TotalTokens => InputTokens + OutputTokens;
        public decimal InputCostUsd { get; set; }
        public decimal OutputCostUsd { get; set; }
        public decimal TotalCostUsd => InputCostUsd + OutputCostUsd;
        public string PriceTableValidAsOf { get; set; } = string.Empty;
    }

    /// <summary>
    /// Snapshot of OpenAI list prices, USD per million tokens.
    /// Refresh from https://openai.com/api/pricing/ when adding models.
    /// </summary>
    internal static class ModelPriceTable
    {
        public const string ValidAsOf = "2026-05";

        // (inputPerMillion, outputPerMillion)
        private static readonly Dictionary<string, (decimal Input, decimal Output)> Prices =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["gpt-4o"]      = (2.50m, 10.00m),
                ["gpt-4o-mini"] = (0.15m,  0.60m),
                ["gpt-4-turbo"] = (10.00m, 30.00m),
                ["gpt-4"]       = (30.00m, 60.00m),
                ["gpt-3.5-turbo"] = (0.50m, 1.50m),
            };

        public static (decimal InputCost, decimal OutputCost) Compute(string modelId, int inputTokens, int outputTokens)
        {
            if (!Prices.TryGetValue(modelId, out var rate))
                return (0m, 0m);
            return (
                inputTokens  * rate.Input  / 1_000_000m,
                outputTokens * rate.Output / 1_000_000m);
        }

        public static IReadOnlyList<string> KnownModels => Prices.Keys.OrderBy(k => k).ToList();
    }
}
