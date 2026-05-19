using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenAI.Models;

namespace Outliner.Services
{
    /// <summary>
    /// Lists OpenAI chat models available to the configured API key by calling
    /// the /v1/models endpoint. Filtered to gpt-* ids since those are the
    /// chat-completion models this app uses.
    /// </summary>
    public sealed class ModelCatalogService
    {
        private readonly string _apiKey;
        private IReadOnlyList<string>? _cache;

        // Hidden from the picker:
        //   - gpt-3.5* — outdated, not worth surfacing for this app's use case
        //   - "pro" tier variants — pricing/latency profile makes them a bad
        //     default and users shouldn't pick them without intent
        //   - TTS / audio / realtime / transcription / image variants —
        //     wrong modality for an outline-generation app
        //   - codex variants — code-focused, not the right fit for prose analysis
        private static readonly string[] BlacklistPrefixes = { "gpt-3.5" };
        private static readonly string[] BlacklistSegments =
            { "pro", "audio", "tts", "realtime", "transcribe", "voice", "image", "codex" };

        public ModelCatalogService(string apiKey)
        {
            _apiKey = apiKey;
        }

        /// <summary>
        /// Returns true for model ids the picker should hide. Centralized here
        /// so the seeded list and the live /v1/models list use the same rules.
        /// </summary>
        public static bool IsBlacklisted(string id)
        {
            if (string.IsNullOrEmpty(id)) return true;
            foreach (var prefix in BlacklistPrefixes)
            {
                if (id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            // Hide ids with a banned dash-separated segment
            // (e.g. "o1-pro", "gpt-4o-audio-preview", "gpt-4o-mini-tts",
            //  "gpt-4o-realtime-preview", "gpt-4o-transcribe",
            //  "gpt-image-1", "gpt-5-codex").
            foreach (var segment in id.Split('-'))
            {
                foreach (var banned in BlacklistSegments)
                {
                    if (string.Equals(segment, banned, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        public async Task<IReadOnlyList<string>> GetChatModelsAsync(CancellationToken ct = default)
        {
            if (_cache != null) return _cache;

            try
            {
                var client = new OpenAIModelClient(_apiKey);
                var result = await client.GetModelsAsync(ct);
                _cache = result.Value
                    .Select(m => m.Id)
                    .Where(id => id.StartsWith("gpt-", StringComparison.OrdinalIgnoreCase))
                    .Where(id => !IsBlacklisted(id))
                    .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                return _cache;
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }
}
