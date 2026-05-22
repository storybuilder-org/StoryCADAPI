using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using StoryCADLib.Models;
using StoryCADLib.Services.API;

namespace StoryCADCritter
{
    /// <summary>
    /// Drives a per-element Key Questions critique walk over the current
    /// StoryCADApi outline. Constructor-injects every collaborator so tests can
    /// swap in a stub IChatCompletionService — no Ioc.Default lookups inside.
    ///
    /// Design (per issue #14):
    ///   - One LLM call per element (Character / Setting / Scene / Problem /
    ///     StoryOverview).
    ///   - Two-tier context granularity:
    ///       Problem  call → Problem + Protagonist + Antagonist (full bodies)
    ///       Scene    call → Scene + Setting (full) + cast as 1-line refs
    ///       Char/Set call → cross-refs as name + 1-line summary only
    ///   - Minimal-outline short-circuit: no Scenes OR fewer than 3
    ///     non-Overview elements → skip LLM, return a structural report.
    ///   - Defensive parsing: free-form / truncated / mis-shaped responses fall
    ///     back to raw text rather than crashing the run.
    ///   - Per-element failures (rate limit, network, 5xx) retry with
    ///     exponential backoff, then continue with the next element.
    /// </summary>
    public sealed class CritiqueOrchestrator
    {
        private readonly StoryCADApi _api;
        private readonly IChatCompletionService _chatService;
        private readonly Kernel? _kernel;
        private readonly string _systemPrompt;

        // Run-scoped cache so we don't re-serialize the same element when it's
        // referenced as a cross-ref body from two parents (e.g. a Character is
        // protagonist of two Problems).
        private readonly ConcurrentDictionary<Guid, string> _bodyCache = new();
        private readonly Dictionary<Guid, StoryElement> _byUuid = new();
        private const int MaxConcurrency = 8;

        // Cache of Key Questions per element-type string. Issue #14 wants these
        // visible in the report so the rubric is transparent.
        private readonly Dictionary<string, List<(string Topic, string Question)>> _keyQuestionsByType = new();

        private const int MaxRetries = 3;
        private static readonly TimeSpan InitialBackoff = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan PerCallTimeout = TimeSpan.FromSeconds(45);

        // Run-scoped progress state, set at the top of RunAsync. CritiqueElementAsync
        // reads these to emit retry-visibility messages while a slot is held inside
        // the timeout/backoff loop (otherwise the UI sees ~141s of dead air per
        // stuck element).
        private IProgress<CritiqueProgress>? _runProgress;
        private int _runDone;
        private int _runTotal;

        // Element types we walk. Folder / Section / Notes / Web / TrashCan /
        // StoryWorld are out-of-scope for the critique — they aren't part of
        // the dramatic-structure rubric.
        private static readonly StoryItemType[] CritiqueableTypes = new[]
        {
            StoryItemType.StoryOverview,
            StoryItemType.Problem,
            StoryItemType.Character,
            StoryItemType.Setting,
            StoryItemType.Scene
        };

        public CritiqueOrchestrator(
            StoryCADApi api,
            IChatCompletionService chatService,
            string systemPrompt,
            Kernel? kernel = null)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _systemPrompt = systemPrompt ?? throw new ArgumentNullException(nameof(systemPrompt));
            _kernel = kernel;
        }

        /// <summary>
        /// Runs the per-element walk against the outline that's currently open
        /// on the injected StoryCADApi. Caller is responsible for opening or
        /// building the outline first.
        /// </summary>
        public async Task<CritiqueRunResult> RunAsync(
            string outlinePath,
            IProgress<CritiqueProgress>? progress = null,
            string? outputDirectory = null,
            CancellationToken cancellationToken = default)
        {
            string ReportFor(string suffix)
            {
                if (string.IsNullOrEmpty(outputDirectory))
                    return outlinePath + suffix;
                var stem = Path.GetFileNameWithoutExtension(outlinePath);
                return Path.Combine(outputDirectory, stem + suffix);
            }

            var run = new CritiqueRunResult
            {
                OutlinePath = outlinePath,
                ReportPath = ReportFor(".critique.md"),
                CostsPath = ReportFor(".costs.json"),
                RawPath = ReportFor(".raw.json")
            };

            var allElementsResult = _api.GetAllElements();
            if (allElementsResult == null || !allElementsResult.IsSuccess || allElementsResult.Payload == null)
            {
                run.HardFailureMessage = $"Couldn't read elements from outline: {allElementsResult?.ErrorMessage ?? "no payload"}";
                return run;
            }

            var elements = allElementsResult.Payload
                .Where(e => CritiqueableTypes.Contains(e.ElementType))
                .ToList();

            foreach (var el in elements)
                _byUuid[el.Uuid] = el;

            // Short-circuit: outline too thin to critique meaningfully.
            var nonOverview = elements.Where(e => e.ElementType != StoryItemType.StoryOverview).ToList();
            var hasScenes = nonOverview.Any(e => e.ElementType == StoryItemType.Scene);
            if (!hasScenes || nonOverview.Count < 3)
            {
                run.ShortCircuited = true;
                run.ShortCircuitReason = !hasScenes
                    ? "Outline contains no Scenes — nothing to walk dramatically. Add at least one Scene to enable a per-element critique."
                    : $"Outline has only {nonOverview.Count} element(s) beyond the Story Overview (need 3+). Add more Characters, Settings, Problems, or Scenes to enable a per-element critique.";

                progress?.Report(new CritiqueProgress($"Short-circuit: {run.ShortCircuitReason}", 0, 0));
                return run;
            }

            // Walk in a deterministic order: Overview, then Problems, Characters,
            // Settings, Scenes. This is the order a developmental editor reads
            // the outline in — premise first, then conflict, then who, where,
            // and finally each beat.
            var ordered = elements
                .OrderBy(e => Array.IndexOf(CritiqueableTypes, e.ElementType))
                .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Preload Key Questions for the types we'll actually use.
            LoadKeyQuestionsForTypes(ordered.Select(e => MapTypeToPromptName(e.ElementType)).Distinct());

            int total = ordered.Count;
            _runProgress = progress;
            _runTotal = total;
            _runDone = 0;

            // Pre-fetch all element bodies serially. StoryCADApi reads aren't
            // documented as thread-safe, so we populate the cache up-front and
            // the parallel walk reads from it only.
            progress?.Report(new CritiqueProgress(
                $"Loading {total} element bodies...", 0, total));
            foreach (var el in ordered)
            {
                cancellationToken.ThrowIfCancellationRequested();
                GetBody(el.Uuid);
            }

            progress?.Report(new CritiqueProgress(
                $"Critiquing {total} elements (up to {MaxConcurrency} in parallel)...", 0, total));

            using var semaphore = new SemaphoreSlim(MaxConcurrency, MaxConcurrency);
            var critiques = new ElementCritique[total];

            var tasks = new Task[total];
            for (int idx = 0; idx < total; idx++)
            {
                int capturedIdx = idx;
                var el = ordered[idx];
                tasks[idx] = RunOne(el, capturedIdx);
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // Per-element failures are captured into ElementCritique.ErrorMessage
                // by CritiqueElementAsync, but a non-element exception (e.g. from
                // the semaphore or task plumbing) can still escape. Let it bubble.
                throw;
            }

            // Preserve deterministic order: append by ordered index.
            foreach (var c in critiques)
            {
                if (c == null) continue;
                run.ElementCritiques.Add(c);
                AccumulateCost(run.Cost, c.Cost);
            }

            async Task RunOne(StoryElement el, int idx)
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var critique = await CritiqueElementAsync(el, cancellationToken);
                    sw.Stop();
                    critiques[idx] = critique;
                    int done = Interlocked.Increment(ref _runDone);
                    var elapsed = sw.Elapsed.TotalSeconds;
                    string status;
                    if (critique.CallFailed)
                        status = $"{done}/{total} FAILED ({el.ElementType} '{el.Name}', {elapsed:F1}s): {critique.ErrorMessage}";
                    else if (critique.ParseFailed)
                        status = $"{done}/{total} parse fallback ({el.ElementType} '{el.Name}', {elapsed:F1}s)";
                    else
                        status = $"{done}/{total} complete ({el.ElementType} '{el.Name}', {elapsed:F1}s)";
                    progress?.Report(new CritiqueProgress(status, done, total));
                }
                finally
                {
                    semaphore.Release();
                }
            }

            // Recompute totals for the run-level price-table-derived costs in case
            // any per-call cost was zeroed because of an unknown model id.
            if (!string.IsNullOrEmpty(run.Cost.ModelId))
            {
                var (inCost, outCost) = ModelPriceTable.Compute(
                    run.Cost.ModelId, run.Cost.InputTokens, run.Cost.OutputTokens);
                run.Cost.InputCostUsd = inCost;
                run.Cost.OutputCostUsd = outCost;
            }

            return run;
        }

        // -- Cost extraction --------------------------------------------------

        /// <summary>
        /// Reads token usage from the SK result's Usage metadata (OpenAI connector
        /// populates an OpenAI.Chat.ChatTokenUsage there). Reflection keeps this
        /// decoupled from a specific OpenAI SDK version. Modeled after Outliner's
        /// ProseAnalyzer.ExtractCost.
        /// </summary>
        private static CritiqueCost ExtractCost(Microsoft.SemanticKernel.ChatMessageContent result)
        {
            var modelId = result.ModelId
                ?? Environment.GetEnvironmentVariable("OPENAI_MODEL")
                ?? "gpt-4o-mini";

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
                // Best-effort; unknown metadata shape yields zero tokens.
            }

            var (inputCost, outputCost) = ModelPriceTable.Compute(modelId, inputTokens, outputTokens);

            return new CritiqueCost
            {
                ModelId = modelId,
                LlmCallCount = 1,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                InputCostUsd = inputCost,
                OutputCostUsd = outputCost
            };
        }

        private static void AccumulateCost(CritiqueCost total, CritiqueCost? perCall)
        {
            if (perCall == null) return;
            if (string.IsNullOrEmpty(total.ModelId))
                total.ModelId = perCall.ModelId;
            total.LlmCallCount += perCall.LlmCallCount;
            total.InputTokens  += perCall.InputTokens;
            total.OutputTokens += perCall.OutputTokens;
        }

        private async Task<ElementCritique> CritiqueElementAsync(StoryElement element, CancellationToken cancellationToken)
        {
            var promptType = MapTypeToPromptName(element.ElementType);
            var keyQuestions = _keyQuestionsByType.TryGetValue(promptType, out var qs)
                ? qs
                : new List<(string Topic, string Question)>();

            var critique = new ElementCritique
            {
                Uuid = element.Uuid,
                Name = element.Name,
                ElementType = promptType,
                KeyQuestions = keyQuestions
            };

            string userMessage;
            try
            {
                userMessage = BuildUserMessage(element, promptType, keyQuestions);
            }
            catch (Exception ex)
            {
                critique.ErrorMessage = $"Failed to assemble prompt: {ex.Message}";
                return critique;
            }

            var history = new ChatHistory();
            history.AddSystemMessage(_systemPrompt);
            history.AddUserMessage(userMessage);

            var settings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.4,
                TopP = 0.9,
                ResponseFormat = "json_object"
            };

            string? rawContent = null;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var perCallCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                perCallCts.CancelAfter(PerCallTimeout);
                try
                {
                    var result = _kernel != null
                        ? await _chatService.GetChatMessageContentAsync(history, settings, _kernel, perCallCts.Token)
                        : await _chatService.GetChatMessageContentAsync(history, settings, null, perCallCts.Token);

                    rawContent = result.Content;
                    critique.Cost = ExtractCost(result);
                    lastException = null;
                    break;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (OperationCanceledException ex) when (attempt < MaxRetries)
                {
                    lastException = new TimeoutException(
                        $"LLM call timed out after {PerCallTimeout.TotalSeconds:F0}s.", ex);
                    var delay = TimeSpan.FromMilliseconds(InitialBackoff.TotalMilliseconds * Math.Pow(2, attempt - 1));
                    ReportRetry(element, attempt, delay, "timed out");
                    try { await Task.Delay(delay, cancellationToken); }
                    catch (OperationCanceledException) { throw; }
                }
                catch (OperationCanceledException ex)
                {
                    lastException = new TimeoutException(
                        $"LLM call timed out after {PerCallTimeout.TotalSeconds:F0}s.", ex);
                    break;
                }
                catch (Exception ex) when (attempt < MaxRetries && IsTransient(ex))
                {
                    lastException = ex;
                    var delay = TimeSpan.FromMilliseconds(InitialBackoff.TotalMilliseconds * Math.Pow(2, attempt - 1));
                    ReportRetry(element, attempt, delay, $"transient error ({ex.GetType().Name})");
                    try { await Task.Delay(delay, cancellationToken); }
                    catch (OperationCanceledException) { throw; }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    break;
                }
            }

            if (lastException != null && rawContent == null)
            {
                critique.ErrorMessage = $"LLM call failed after {MaxRetries} attempt(s): {lastException.Message}";
                return critique;
            }

            if (string.IsNullOrWhiteSpace(rawContent))
            {
                critique.ErrorMessage = "LLM returned an empty response.";
                return critique;
            }

            critique.RawResponse = rawContent;

            // Defensive parse: try the schema first; on any failure leave Parsed
            // null so the report writer surfaces the raw text under a
            // "couldn't parse cleanly" banner.
            try
            {
                var parsed = JsonSerializer.Deserialize<CritiqueElementResponse>(rawContent, CritiqueJson.Options);
                if (parsed != null && LooksLikeValidResponse(parsed))
                {
                    // Force the element-identity fields from our side — the LLM
                    // is allowed to drift these and we trust the orchestrator.
                    parsed.ElementUuid = element.Uuid.ToString();
                    parsed.ElementType = promptType;
                    parsed.ElementName = element.Name;
                    critique.Parsed = parsed;
                    critique.RawResponse = null; // don't double-render
                }
            }
            catch (JsonException)
            {
                // fall through — RawResponse preserved for the report
            }

            return critique;
        }

        /// <summary>
        /// Builds the user message body for one element. Two-tier granularity
        /// lives here.
        /// </summary>
        private string BuildUserMessage(
            StoryElement element,
            string promptType,
            List<(string Topic, string Question)> keyQuestions)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Element type: {promptType}");
            sb.AppendLine($"Element UUID: {element.Uuid}");
            sb.AppendLine($"Element name: {element.Name}");
            sb.AppendLine();

            sb.AppendLine("Element data:");
            sb.AppendLine(GetBody(element.Uuid));
            sb.AppendLine();

            // Cross-references — granularity depends on element type.
            var refs = BuildCrossReferences(element);
            if (!string.IsNullOrWhiteSpace(refs))
            {
                sb.AppendLine("Cross-references:");
                sb.AppendLine(refs);
                sb.AppendLine();
            }

            sb.AppendLine("Key Questions for this element type:");
            if (keyQuestions.Count == 0)
            {
                sb.AppendLine("(none defined for this element type — apply general developmental-edit judgment)");
            }
            else
            {
                foreach (var grp in keyQuestions.GroupBy(q => q.Topic))
                {
                    sb.AppendLine($"- {grp.Key}:");
                    foreach (var q in grp)
                        sb.AppendLine($"    * {q.Question}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Two-tier granularity selector. Returns the cross-reference block as
        /// a string (may be empty).
        /// </summary>
        private string BuildCrossReferences(StoryElement element)
        {
            var sb = new StringBuilder();
            var body = GetBody(element.Uuid);

            // Pull GUID-like values out of the serialized body — the API
            // exposes references by GUID strings inside the JSON.
            var referencedGuids = ExtractGuidsFromJson(body)
                .Where(g => g != element.Uuid && _byUuid.ContainsKey(g))
                .Distinct()
                .ToList();

            if (referencedGuids.Count == 0)
                return string.Empty;

            switch (element.ElementType)
            {
                case StoryItemType.Problem:
                {
                    // Problem call gets Protagonist + Antagonist full bodies.
                    // Other refs (if any) get 1-line summary.
                    var fullBodyRoles = new HashSet<Guid>();
                    foreach (var role in new[] { "Protagonist", "Antagonist" })
                    {
                        if (TryGetGuidProperty(body, role, out var roleGuid) && _byUuid.ContainsKey(roleGuid))
                            fullBodyRoles.Add(roleGuid);
                    }
                    foreach (var refGuid in referencedGuids)
                    {
                        if (fullBodyRoles.Contains(refGuid))
                            AppendFullBodyRef(sb, refGuid);
                        else
                            AppendOneLineRef(sb, refGuid);
                    }
                    break;
                }
                case StoryItemType.Scene:
                {
                    // Scene call gets Setting full body. Cast / Protagonist /
                    // Antagonist are 1-line refs (the prompt cares about scene
                    // structure, not deep character bios per scene).
                    if (TryGetGuidProperty(body, "Setting", out var settingGuid) && _byUuid.TryGetValue(settingGuid, out var setting)
                        && setting.ElementType == StoryItemType.Setting)
                    {
                        AppendFullBodyRef(sb, settingGuid);
                    }
                    foreach (var refGuid in referencedGuids)
                    {
                        if (_byUuid[refGuid].ElementType == StoryItemType.Setting)
                            continue; // already included as full body
                        AppendOneLineRef(sb, refGuid);
                    }
                    break;
                }
                default:
                {
                    // Character / Setting / StoryOverview: name + 1-line summary only.
                    foreach (var refGuid in referencedGuids)
                        AppendOneLineRef(sb, refGuid);
                    break;
                }
            }

            return sb.ToString();
        }

        private void AppendFullBodyRef(StringBuilder sb, Guid refGuid)
        {
            var refEl = _byUuid[refGuid];
            sb.AppendLine($"- [{refEl.ElementType}] {refEl.Name} ({refGuid}) — full body:");
            sb.AppendLine(GetBody(refGuid));
            sb.AppendLine();
        }

        private void AppendOneLineRef(StringBuilder sb, Guid refGuid)
        {
            var refEl = _byUuid[refGuid];
            sb.AppendLine($"- [{refEl.ElementType}] {refEl.Name} ({refGuid}) — {OneLineSummary(refEl)}");
        }

        private string OneLineSummary(StoryElement element)
        {
            // Description is StoryElement.Description on the base class — that
            // populates "Character Sketch", "Setting Summary", "Story Question",
            // etc. Use first non-empty line if present.
            if (!string.IsNullOrWhiteSpace(element.Description))
            {
                var firstLine = element.Description
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .FirstOrDefault(s => s.Length > 0);
                if (!string.IsNullOrWhiteSpace(firstLine))
                    return Truncate(firstLine, 200);
            }
            return $"(no summary on file for this {element.ElementType})";
        }

        private static string Truncate(string s, int max) =>
            s.Length <= max ? s : s.Substring(0, max - 1) + "…";

        private string GetBody(Guid uuid)
        {
            // StoryCADApi.GetElement returns OperationResult<object> whose Payload
            // is already the element serialized to a JSON string (element.Serialize()).
            // ToString() on that string is the identity — do NOT JsonSerializer it
            // again, or the body becomes a double-encoded escaped string.
            return _bodyCache.GetOrAdd(uuid, static (key, api) =>
            {
                var result = api.GetElement(key);
                return (result != null && result.IsSuccess && result.Payload != null)
                    ? result.Payload.ToString() ?? string.Empty
                    : $"{{ \"error\": \"could not retrieve element body for {key}\" }}";
            }, _api);
        }

        private void LoadKeyQuestionsForTypes(IEnumerable<string> promptTypes)
        {
            foreach (var t in promptTypes.Distinct())
            {
                if (_keyQuestionsByType.ContainsKey(t)) continue;
                var result = _api.GetKeyQuestions(t);
                if (result != null && result.IsSuccess && result.Payload != null)
                {
                    _keyQuestionsByType[t] = result.Payload.ToList();
                }
                else
                {
                    _keyQuestionsByType[t] = new List<(string, string)>();
                }
            }
        }

        /// <summary>
        /// Maps StoryItemType to the element-type name used in the prompt + Key
        /// Questions data (which uses "Overview" rather than "StoryOverview").
        /// </summary>
        private static string MapTypeToPromptName(StoryItemType t) => t switch
        {
            StoryItemType.StoryOverview => "Overview",
            StoryItemType.Problem       => "Problem",
            StoryItemType.Character     => "Character",
            StoryItemType.Setting       => "Setting",
            StoryItemType.Scene         => "Scene",
            _ => t.ToString()
        };

        // -- JSON helpers over the raw element body --------------------------

        private static IEnumerable<Guid> ExtractGuidsFromJson(string json)
        {
            // Walk the parsed JsonDocument and yield every value that parses
            // as a Guid. Tolerant of nested objects, arrays, and stringified
            // GUIDs (the StoryCADApi serializes both ways across element types).
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(json);
            }
            catch
            {
                yield break;
            }
            using (doc)
            {
                foreach (var g in WalkForGuids(doc.RootElement))
                    yield return g;
            }
        }

        private static IEnumerable<Guid> WalkForGuids(JsonElement el)
        {
            switch (el.ValueKind)
            {
                case JsonValueKind.String:
                    if (Guid.TryParse(el.GetString(), out var g) && g != Guid.Empty)
                        yield return g;
                    break;
                case JsonValueKind.Object:
                    foreach (var prop in el.EnumerateObject())
                        foreach (var inner in WalkForGuids(prop.Value))
                            yield return inner;
                    break;
                case JsonValueKind.Array:
                    foreach (var item in el.EnumerateArray())
                        foreach (var inner in WalkForGuids(item))
                            yield return inner;
                    break;
            }
        }

        private static bool TryGetGuidProperty(string json, string propertyName, out Guid value)
        {
            value = Guid.Empty;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return false;
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (!string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (prop.Value.ValueKind == JsonValueKind.String
                        && Guid.TryParse(prop.Value.GetString(), out var g)
                        && g != Guid.Empty)
                    {
                        value = g;
                        return true;
                    }
                }
            }
            catch
            {
                // ignore
            }
            return false;
        }

        // -- Response validation -------------------------------------------

        private static bool LooksLikeValidResponse(CritiqueElementResponse r)
        {
            // The schema requires arrays for strengths/concerns/questions and
            // strings for the identity fields. Anything else is treated as
            // "didn't parse cleanly" — the report will fall back to raw text.
            // Empty arrays ARE valid (the prompt permits them).
            return r.Strengths != null && r.Concerns != null && r.QuestionsForAuthor != null;
        }

        private void ReportRetry(StoryElement element, int attempt, TimeSpan delay, string reason)
        {
            _runProgress?.Report(new CritiqueProgress(
                $"'{element.Name}' {reason} (attempt {attempt}/{MaxRetries}), retrying in {delay.TotalSeconds:F0}s...",
                Volatile.Read(ref _runDone), _runTotal));
        }

        // -- Retry classification ------------------------------------------

        private static bool IsTransient(Exception ex)
        {
            // Be permissive: network blips, 5xx, rate limit. SK wraps the
            // OpenAI HTTP error in HttpOperationException / similar; we look
            // at the type name + message text rather than taking a hard
            // dependency on the connector-internal exception types.
            var typeName = ex.GetType().Name;
            if (typeName.Contains("HttpOperation", StringComparison.OrdinalIgnoreCase)) return true;
            if (typeName.Contains("HttpRequest", StringComparison.OrdinalIgnoreCase)) return true;
            if (typeName.Contains("TaskCanceled", StringComparison.OrdinalIgnoreCase)) return true;

            var msg = ex.Message ?? string.Empty;
            if (msg.Contains("429")) return true;
            if (msg.Contains("rate limit", StringComparison.OrdinalIgnoreCase)) return true;
            if (msg.Contains("timeout", StringComparison.OrdinalIgnoreCase)) return true;
            if (msg.Contains("temporarily", StringComparison.OrdinalIgnoreCase)) return true;
            // Rough 5xx detector
            for (int code = 500; code < 600; code++)
                if (msg.Contains(code.ToString())) return true;
            return false;
        }

        // -- Report writing ------------------------------------------------

        /// <summary>
        /// Renders a CritiqueRunResult to Markdown. Public + static so tests
        /// can hand it canned ElementCritiques and assert on the output.
        /// </summary>
        public static string RenderReport(CritiqueRunResult run, string outlineDisplayName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# Critique — {outlineDisplayName}");
            sb.AppendLine();
            sb.AppendLine($"_Generated by StoryCADCritter from `{run.OutlinePath}`._");
            sb.AppendLine();

            if (run.HardFailed)
            {
                sb.AppendLine("## Hard failure");
                sb.AppendLine();
                sb.AppendLine(run.HardFailureMessage);
                return sb.ToString();
            }

            if (run.ShortCircuited)
            {
                sb.AppendLine("## Outline too sparse for a per-element critique");
                sb.AppendLine();
                sb.AppendLine(run.ShortCircuitReason);
                sb.AppendLine();
                sb.AppendLine("No LLM calls were made. Flesh out the outline and re-run to receive a per-element walk.");
                return sb.ToString();
            }

            var errorCount = run.ElementCritiques.Count(e => e.CallFailed);
            var parseFallbackCount = run.ElementCritiques.Count(e => e.ParseFailed);
            var successCount = run.ElementCritiques.Count(e => e.Succeeded);

            sb.AppendLine("## Summary");
            sb.AppendLine();
            sb.AppendLine($"- Elements walked: **{run.ElementCritiques.Count}**");
            sb.AppendLine($"- Cleanly parsed: **{successCount}**");
            if (parseFallbackCount > 0)
                sb.AppendLine($"- Couldn't parse cleanly (raw text retained): **{parseFallbackCount}**");
            if (errorCount > 0)
                sb.AppendLine($"- LLM call failed: **{errorCount}**");
            sb.AppendLine();

            foreach (var critique in run.ElementCritiques)
                RenderElement(sb, critique);

            if (errorCount > 0)
            {
                sb.AppendLine("## Errors");
                sb.AppendLine();
                foreach (var c in run.ElementCritiques.Where(e => e.CallFailed))
                    sb.AppendLine($"- [{c.ElementType}] {c.Name} ({c.Uuid}): {c.ErrorMessage}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static void RenderElement(StringBuilder sb, ElementCritique critique)
        {
            sb.AppendLine($"## [{critique.ElementType}] {critique.Name}");
            sb.AppendLine();
            sb.AppendLine($"_UUID: `{critique.Uuid}`_");
            sb.AppendLine();

            if (critique.KeyQuestions.Count > 0)
            {
                sb.AppendLine("<details><summary>Key Questions used</summary>");
                sb.AppendLine();
                foreach (var grp in critique.KeyQuestions.GroupBy(q => q.Topic))
                {
                    sb.AppendLine($"**{grp.Key}**");
                    foreach (var q in grp)
                        sb.AppendLine($"- {q.Question}");
                    sb.AppendLine();
                }
                sb.AppendLine("</details>");
                sb.AppendLine();
            }

            if (critique.CallFailed)
            {
                sb.AppendLine($"> **LLM call failed.** {critique.ErrorMessage}");
                sb.AppendLine();
                return;
            }

            if (critique.Parsed == null)
            {
                sb.AppendLine("> **Couldn't parse the LLM response into the expected schema — raw text below.**");
                sb.AppendLine();
                sb.AppendLine("```");
                sb.AppendLine(critique.RawResponse ?? "(empty response)");
                sb.AppendLine("```");
                sb.AppendLine();
                return;
            }

            var p = critique.Parsed;
            RenderSection(sb, "Strengths", p.Strengths);
            RenderSection(sb, "Concerns", p.Concerns);

            if (p.QuestionsForAuthor.Count > 0)
            {
                sb.AppendLine("### Questions for the author");
                sb.AppendLine();
                foreach (var q in p.QuestionsForAuthor)
                    sb.AppendLine($"- {q}");
                sb.AppendLine();
            }

            if (p.Strengths.Count == 0 && p.Concerns.Count == 0 && p.QuestionsForAuthor.Count == 0)
            {
                sb.AppendLine("_The reader returned no findings — element may be too sparse to critique meaningfully._");
                sb.AppendLine();
            }
        }

        private static void RenderSection(StringBuilder sb, string heading, List<CritiqueFinding> items)
        {
            if (items.Count == 0) return;
            sb.AppendLine($"### {heading}");
            sb.AppendLine();
            foreach (var item in items)
            {
                if (!string.IsNullOrWhiteSpace(item.KeyQuestion) && !item.KeyQuestion.Equals("general", StringComparison.OrdinalIgnoreCase))
                    sb.AppendLine($"- _Re: {item.KeyQuestion}_");
                sb.AppendLine($"  - {item.Finding}");
            }
            sb.AppendLine();
        }
    }
}
