using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        private readonly int _maxConcurrency;

        // Cache of Key Questions per element-type string. Issue #14 wants these
        // visible in the report so the rubric is transparent.
        private readonly Dictionary<string, List<(string Topic, string Question)>> _keyQuestionsByType = new();
        private readonly Dictionary<Guid, CritiquePlan> _plansByUuid = new();
        private readonly Dictionary<Guid, string> _spineCandidateReasons = new();

        private const int MaxRetries = 3;
        private static readonly TimeSpan InitialBackoff = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan PerCallTimeout = TimeSpan.FromSeconds(45);
        private static readonly HashSet<string> SignalStopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "about", "after", "again", "against", "alone", "before", "being", "between",
            "could", "does", "from", "have", "into", "more", "must", "than", "that",
            "their", "them", "there", "they", "this", "through", "when", "where",
            "which", "while", "with", "without", "woman", "story", "problem",
            "character", "protagonist", "antagonist", "goal", "motive", "theme",
            "premise", "learn"
        };

        internal const string ModeFull = "Full structural read";
        internal const string ModeSpine = "Story-spine candidate";
        internal const string ModeSupporting = "Supporting structural read";
        internal const string ModeFunctional = "Functional element read";
        internal const string ModeContext = "Context read";

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

        private sealed class CritiquePlan
        {
            public string Mode { get; init; } = ModeFull;
            public string Focus { get; init; } = string.Empty;
            public bool IsStorySpineCandidate { get; init; }
            public bool IsFunctional { get; init; }
        }

        public CritiqueOrchestrator(
            StoryCADApi api,
            IChatCompletionService chatService,
            string systemPrompt,
            Kernel? kernel = null,
            int maxConcurrency = 8)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _systemPrompt = systemPrompt ?? throw new ArgumentNullException(nameof(systemPrompt));
            _kernel = kernel;
            // Clamp to a sane range; the semaphore must be >= 1.
            _maxConcurrency = Math.Clamp(maxConcurrency, 1, 16);
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

            _bodyCache.Clear();
            _byUuid.Clear();
            _plansByUuid.Clear();
            _spineCandidateReasons.Clear();

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

            BuildCritiquePlans(ordered, run);

            progress?.Report(new CritiqueProgress(
                $"Critiquing {total} elements (up to {_maxConcurrency} in parallel)...", 0, total));

            using var semaphore = new SemaphoreSlim(_maxConcurrency, _maxConcurrency);
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

            // Significance/coherence is emergent: judge it only after the
            // per-element reads exist. (a) deterministic from the per-element
            // verdicts + declared roles; (b) one LLM synthesis call only when
            // (a) is under-determined.
            await BuildStoryProblemCoherenceAsync(ordered, run, cancellationToken);

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
            var baseQuestions = _keyQuestionsByType.TryGetValue(promptType, out var qs)
                ? qs
                : new List<(string Topic, string Question)>();
            // Personalize the shared per-type rubric for THIS element: generic
            // role nouns ("the character", "your protagonist") become real names,
            // and generic pronouns become the referent's actual gendered pronoun
            // (from the Sex field). Issue #14 — fixes the rubric mis-gendering.
            var keyQuestions = PersonalizeKeyQuestions(element, baseQuestions);
            var plan = _plansByUuid.TryGetValue(element.Uuid, out var p)
                ? p
                : new CritiquePlan();
            keyQuestions = FilterKeyQuestionsForPlan(element.ElementType, plan.Mode, keyQuestions);

            var critique = new ElementCritique
            {
                Uuid = element.Uuid,
                Name = element.Name,
                ElementType = promptType,
                KeyQuestions = keyQuestions,
                CritiqueMode = plan.Mode,
                CritiqueFocus = plan.Focus
            };

            string userMessage;
            try
            {
                userMessage = BuildUserMessage(element, promptType, keyQuestions, plan);
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
            List<(string Topic, string Question)> keyQuestions,
            CritiquePlan plan)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Element type: {promptType}");
            sb.AppendLine($"Element UUID: {element.Uuid}");
            sb.AppendLine($"Element name: {element.Name}");
            sb.AppendLine();

            sb.AppendLine("Critique plan:");
            sb.AppendLine($"- Mode: {plan.Mode}");
            if (!string.IsNullOrWhiteSpace(plan.Focus))
                sb.AppendLine($"- Focus: {plan.Focus}");
            sb.AppendLine("- Treat StoryCAD metadata as structural signals, not certain authorial intent. If signals conflict, surface the ambiguity instead of declaring one interpretation correct.");
            if (plan.IsFunctional)
            {
                sb.AppendLine("- This is a functional/minor element. Judge whether it serves its story purpose; do not require lead-character depth, a full arc, or complete backstory unless the outline itself makes those expectations relevant.");
                if (element.ElementType == StoryItemType.Character)
                    sb.AppendLine("- For a functional/minor character, acceptable concerns are unclear story function, duplicated cast function, weak connection to central characters, or continuity. Do not raise missing backstory, motivation, dimensionality, physical description, flaw, or arc as a concern unless the missing information blocks that function.");
            }
            else if (plan.IsStorySpineCandidate)
            {
                sb.AppendLine("- This element may carry the story spine. Give it the deepest structural attention and notice whether related elements support or obscure it.");
            }
            else
            {
                sb.AppendLine("- Calibrate the depth of critique to this element's support role in the wider outline.");
                if (element.ElementType == StoryItemType.Character && plan.Mode == ModeSupporting)
                    sb.AppendLine("- For a supporting character, focus on role clarity, relationship to high-signal characters, and contribution to the likely spine. Avoid lead-level backstory, psychological depth, visualization, flaw, or full-arc demands unless the outline makes that support role depend on them.");
            }
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

        // -- Structural planning -------------------------------------------

        private void BuildCritiquePlans(IReadOnlyList<StoryElement> ordered, CritiqueRunResult run)
        {
            var overviewSignalText = BuildOverviewSignalText(ordered);
            var referenceCounts = ordered.ToDictionary(e => e.Uuid, _ => 0);
            foreach (var el in ordered)
            {
                foreach (var guid in ExtractGuidsFromJson(GetBody(el.Uuid)).Distinct())
                {
                    if (guid != el.Uuid && referenceCounts.ContainsKey(guid))
                        referenceCounts[guid]++;
                }
            }

            var centralCharacters = new HashSet<Guid>();
            foreach (var character in ordered.Where(e => e.ElementType == StoryItemType.Character))
            {
                var body = GetBody(character.Uuid);
                var storyRole = TryGetStringProperty(body, "StoryRole", out var sr) ? sr : string.Empty;
                var refCount = referenceCounts.TryGetValue(character.Uuid, out var c) ? c : 0;
                if (IsCentralStoryRole(storyRole) || refCount >= 4)
                    centralCharacters.Add(character.Uuid);
            }

            var problemSignals = new List<(StoryElement Element, int OverviewOverlapScore)>();
            var spineCandidates = new HashSet<Guid>();
            foreach (var problem in ordered.Where(e => e.ElementType == StoryItemType.Problem))
            {
                var body = GetBody(problem.Uuid);
                var category = TryGetStringProperty(body, "ProblemCategory", out var pc) ? pc : string.Empty;
                var conflict = TryGetStringProperty(body, "ConflictType", out var ct) ? ct : string.Empty;
                TryGetGuidProperty(body, "Protagonist", out var protagonist);
                TryGetGuidProperty(body, "Antagonist", out var antagonist);

                var declaredStoryProblem = ContainsAny(category, "story problem", "main");
                var internalConflict = ContainsAny(conflict, "self")
                    || (protagonist != Guid.Empty && protagonist == antagonist);
                var centralInternalConflict = internalConflict && centralCharacters.Contains(protagonist);
                var overviewOverlapScore = ScoreTextOverlap(overviewSignalText, BuildProblemSignalText(problem, body));
                problemSignals.Add((problem, overviewOverlapScore));

                if (declaredStoryProblem || centralInternalConflict)
                {
                    spineCandidates.Add(problem.Uuid);
                    _spineCandidateReasons[problem.Uuid] = declaredStoryProblem && centralInternalConflict
                        ? "declared story problem and internal-conflict signal"
                        : declaredStoryProblem
                            ? "declared StoryCAD story-problem signal"
                            : "internal-conflict signal attached to a high-signal character";
                }
            }

            if (spineCandidates.Count == 0 && problemSignals.Count > 0)
            {
                var maxOverlap = problemSignals.Max(s => s.OverviewOverlapScore);
                if (maxOverlap >= 2)
                {
                    foreach (var signal in problemSignals.Where(s => s.OverviewOverlapScore == maxOverlap))
                    {
                        spineCandidates.Add(signal.Element.Uuid);
                        _spineCandidateReasons[signal.Element.Uuid] =
                            $"overview StoryProblem/Premise overlap ({signal.OverviewOverlapScore} shared signal word(s))";
                    }
                }
            }

            foreach (var problem in problemSignals.Where(s => spineCandidates.Contains(s.Element.Uuid)).Select(s => s.Element))
            {
                var body = GetBody(problem.Uuid);
                foreach (var roleProperty in new[] { "Protagonist", "Antagonist" })
                {
                    if (TryGetGuidProperty(body, roleProperty, out var characterGuid)
                        && _byUuid.TryGetValue(characterGuid, out var character)
                        && character.ElementType == StoryItemType.Character)
                    {
                        centralCharacters.Add(characterGuid);
                    }
                }
            }

            foreach (var element in ordered)
            {
                _plansByUuid[element.Uuid] = PlanForElement(
                    element, referenceCounts, centralCharacters, spineCandidates);
            }

            run.StructuralOrientation = RenderStructuralOrientation(
                ordered, referenceCounts, centralCharacters, spineCandidates);
            run.StructuralCompleteness = RenderStructuralCompleteness(ordered);
        }

        private CritiquePlan PlanForElement(
            StoryElement element,
            IReadOnlyDictionary<Guid, int> referenceCounts,
            HashSet<Guid> centralCharacters,
            HashSet<Guid> spineCandidates)
        {
            var body = GetBody(element.Uuid);
            var refCount = referenceCounts.TryGetValue(element.Uuid, out var rc) ? rc : 0;

            switch (element.ElementType)
            {
                case StoryItemType.StoryOverview:
                    return new CritiquePlan
                    {
                        Mode = ModeFull,
                        Focus = "Use the overview to notice the story's declared premise, problem, genre, and any ambiguity in what appears central."
                    };

                case StoryItemType.Character:
                {
                    var storyRole = TryGetStringProperty(body, "StoryRole", out var sr) ? sr : string.Empty;
                    var central = centralCharacters.Contains(element.Uuid);
                    var functional = IsFunctionalStoryRole(storyRole)
                        || (!central && refCount <= 1);

                    if (central)
                    {
                        return new CritiquePlan
                        {
                            Mode = ModeFull,
                            IsStorySpineCandidate = IsCentralStoryRole(storyRole),
                            Focus = $"High-signal character: StoryRole='{BlankIfEmpty(storyRole)}', referenced {refCount} time(s). Give this character a full developmental read."
                        };
                    }

                    if (functional)
                    {
                        return new CritiquePlan
                        {
                            Mode = ModeFunctional,
                            IsFunctional = true,
                            Focus = $"Functional/minor character: StoryRole='{BlankIfEmpty(storyRole)}', referenced {refCount} time(s). Review role clarity and whether the character earns or duplicates a function; do not require lead-level psychology."
                        };
                    }

                    return new CritiquePlan
                    {
                        Mode = ModeSupporting,
                        Focus = $"Supporting character: StoryRole='{BlankIfEmpty(storyRole)}', referenced {refCount} time(s). Review how this character supports the likely spine rather than demanding a full protagonist arc."
                    };
                }

                case StoryItemType.Problem:
                {
                    var category = TryGetStringProperty(body, "ProblemCategory", out var pc) ? pc : string.Empty;
                    var conflict = TryGetStringProperty(body, "ConflictType", out var ct) ? ct : string.Empty;
                    TryGetGuidProperty(body, "Protagonist", out var protagonist);
                    TryGetGuidProperty(body, "Antagonist", out var antagonist);

                    var declaredStoryProblem = ContainsAny(category, "story problem", "main");
                    var internalConflict = ContainsAny(conflict, "self")
                        || (protagonist != Guid.Empty && protagonist == antagonist);
                    var hasCentralRole = centralCharacters.Contains(protagonist)
                        || centralCharacters.Contains(antagonist);
                    var functionalRoles = IsFunctionalCharacter(protagonist, referenceCounts, centralCharacters)
                        && (antagonist == Guid.Empty || IsFunctionalCharacter(antagonist, referenceCounts, centralCharacters));

                    if (spineCandidates.Contains(element.Uuid))
                    {
                        var reason = _spineCandidateReasons.TryGetValue(element.Uuid, out var storedReason)
                            ? storedReason
                            : declaredStoryProblem && internalConflict
                                ? "declared story problem and internal-conflict signal"
                                : declaredStoryProblem
                                    ? "declared StoryCAD story-problem signal"
                                    : "internal-conflict signal attached to a high-signal character";
                        return new CritiquePlan
                        {
                            Mode = ModeSpine,
                            IsStorySpineCandidate = true,
                            Focus = $"Story-spine candidate ({reason}): ProblemCategory='{BlankIfEmpty(category)}', ConflictType='{BlankIfEmpty(conflict)}'. Give this problem a full read, but surface ambiguity if another problem also appears central."
                        };
                    }

                    if (functionalRoles || (ContainsAny(category, "sequence") && !hasCentralRole))
                    {
                        return new CritiquePlan
                        {
                            Mode = ModeFunctional,
                            IsFunctional = true,
                            Focus = $"Functional/pressure-event problem: ProblemCategory='{BlankIfEmpty(category)}', ConflictType='{BlankIfEmpty(conflict)}'. Review whether it pressures, reveals, or turns the likely story spine; do not require it to carry a full balanced protagonist/antagonist arc."
                        };
                    }

                    return new CritiquePlan
                    {
                        Mode = ModeSupporting,
                        Focus = $"Supporting problem: ProblemCategory='{BlankIfEmpty(category)}', ConflictType='{BlankIfEmpty(conflict)}'. Review how it connects to the likely story spine and whether its purpose is clear."
                    };
                }

                case StoryItemType.Scene:
                {
                    TryGetGuidProperty(body, "Protagonist", out var protagonist);
                    TryGetGuidProperty(body, "Antagonist", out var antagonist);
                    var cast = TryGetGuidListProperty(body, "CastMembers");
                    var touchesCentral = centralCharacters.Contains(protagonist)
                        || centralCharacters.Contains(antagonist)
                        || cast.Any(centralCharacters.Contains);

                    if (touchesCentral)
                    {
                        return new CritiquePlan
                        {
                            Mode = ModeSupporting,
                            Focus = "Scene touches a high-signal character. Review whether the scene changes pressure, understanding, relationship, or stakes around the likely story spine."
                        };
                    }

                    return new CritiquePlan
                    {
                        Mode = ModeFunctional,
                        IsFunctional = true,
                        Focus = "Scene does not directly reference a high-signal character. Review scene purpose and continuity; do not require it to solve the story's central problem by itself."
                    };
                }

                case StoryItemType.Setting:
                    return new CritiquePlan
                    {
                        Mode = ModeContext,
                        Focus = "Review whether the setting is clear, sensory, and useful to the scenes it supports."
                    };

                default:
                    return new CritiquePlan();
            }
        }

        private bool IsFunctionalCharacter(
            Guid characterGuid,
            IReadOnlyDictionary<Guid, int> referenceCounts,
            HashSet<Guid> centralCharacters)
        {
            if (characterGuid == Guid.Empty || !_byUuid.TryGetValue(characterGuid, out var el))
                return false;
            if (el.ElementType != StoryItemType.Character)
                return false;
            if (centralCharacters.Contains(characterGuid))
                return false;

            var body = GetBody(characterGuid);
            var storyRole = TryGetStringProperty(body, "StoryRole", out var sr) ? sr : string.Empty;
            var refCount = referenceCounts.TryGetValue(characterGuid, out var rc) ? rc : 0;
            return IsFunctionalStoryRole(storyRole) || refCount <= 1;
        }

        private string RenderStructuralOrientation(
            IReadOnlyList<StoryElement> ordered,
            IReadOnlyDictionary<Guid, int> referenceCounts,
            HashSet<Guid> centralCharacters,
            HashSet<Guid> spineCandidates)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Critter ran a deterministic orientation pass before the LLM walk. No extra LLM call was used for this pass; generated StoryCAD fields are treated as structural signals, not proof of authorial intent.");
            sb.AppendLine();

            var spineNames = ordered
                .Where(e => spineCandidates.Contains(e.Uuid))
                .Select(e =>
                {
                    var body = GetBody(e.Uuid);
                    var category = TryGetStringProperty(body, "ProblemCategory", out var pc) ? pc : string.Empty;
                    var conflict = TryGetStringProperty(body, "ConflictType", out var ct) ? ct : string.Empty;
                    var reason = _spineCandidateReasons.TryGetValue(e.Uuid, out var r)
                        ? $", signal='{r}'"
                        : string.Empty;
                    return $"[Problem] {e.Name} (ProblemCategory='{BlankIfEmpty(category)}', ConflictType='{BlankIfEmpty(conflict)}'{reason})";
                })
                .ToList();
            sb.AppendLine(spineNames.Count == 0
                ? "- Story-spine candidates: none strongly signaled; element critiques should surface ambiguity."
                : "- Story-spine candidates: " + string.Join("; ", spineNames));

            var highSignalCharacters = ordered
                .Where(e => centralCharacters.Contains(e.Uuid))
                .Select(e =>
                {
                    var body = GetBody(e.Uuid);
                    var storyRole = TryGetStringProperty(body, "StoryRole", out var sr) ? sr : string.Empty;
                    var refCount = referenceCounts.TryGetValue(e.Uuid, out var rc) ? rc : 0;
                    return $"{e.Name} (StoryRole='{BlankIfEmpty(storyRole)}', refs={refCount})";
                })
                .ToList();
            if (highSignalCharacters.Count > 0)
                sb.AppendLine("- High-signal characters: " + string.Join("; ", highSignalCharacters));

            var functionalCharacters = ordered
                .Where(e => e.ElementType == StoryItemType.Character
                    && _plansByUuid.TryGetValue(e.Uuid, out var p)
                    && p.Mode == ModeFunctional)
                .Select(e => e.Name)
                .Take(8)
                .ToList();
            if (functionalCharacters.Count > 0)
                sb.AppendLine("- Functional/minor cast: " + string.Join("; ", functionalCharacters));

            sb.AppendLine("- Critique calibration: spine candidates get the deepest read; supporting elements are judged by connection to the likely spine; functional elements are judged by story purpose and economy.");
            return sb.ToString().Trim();
        }

        // -- Completeness (deterministic, pre-walk) -------------------------

        /// <summary>
        /// Presence/absence only. Absence is decidable regardless of how messy
        /// the prose is, so this is reliable on incomplete outlines. It both
        /// lists structural gaps for the author and explains why significance
        /// (which problem is central, who really changes) can't be trusted up
        /// front when the declaring fields are blank.
        /// </summary>
        private string RenderStructuralCompleteness(IReadOnlyList<StoryElement> ordered)
        {
            var notes = new List<string>();

            var overview = ordered.FirstOrDefault(e => e.ElementType == StoryItemType.StoryOverview);
            if (overview != null)
            {
                // Story Idea is the Overview's base Description field.
                if (string.IsNullOrWhiteSpace(overview.Description))
                    notes.Add("- The Story Overview has no Story Idea. Capture the original idea that prompted the story.");
                if (!TryGetStringProperty(GetBody(overview.Uuid), "Premise", out _))
                    notes.Add("- The Story Overview has no Premise. A one-sentence premise anchors what the story is about.");
            }

            if (!HasDeclaredStoryProblem(ordered))
                notes.Add("- No problem is marked as the story problem. Mark which problem ends the story when it is resolved, so the central question is unambiguous.");

            foreach (var problem in ordered.Where(e => e.ElementType == StoryItemType.Problem))
            {
                var body = GetBody(problem.Uuid);
                var missing = new List<string>();
                if (!TryGetGuidProperty(body, "Protagonist", out _)) missing.Add("protagonist");
                if (!TryGetGuidProperty(body, "Antagonist", out _)) missing.Add("antagonist");
                if (!TryGetStringProperty(body, "ProtGoal", out _)) missing.Add("protagonist goal (the question it poses)");
                if (!TryGetStringProperty(body, "Outcome", out _)) missing.Add("resolution/outcome (the answer)");
                if (missing.Count > 0)
                    notes.Add($"- Problem \"{problem.Name}\" is missing: {string.Join(", ", missing)}.");
            }

            foreach (var character in ordered.Where(e => e.ElementType == StoryItemType.Character))
            {
                if (!TryGetStringProperty(GetBody(character.Uuid), "StoryRole", out _))
                    notes.Add($"- Character \"{character.Name}\" has no StoryRole. A role (Protagonist, Antagonist, Supporting, etc.) clarifies how central the character is.");
            }

            if (notes.Count == 0)
                return string.Empty;

            return "These structural fields are empty in the outline. Filling them sharpens both the critique and the story:"
                + Environment.NewLine + Environment.NewLine
                + string.Join(Environment.NewLine, notes);
        }

        // -- Story-problem coherence (post-walk: (a) deterministic, (b) LLM) --

        /// <summary>
        /// Judged after the per-element reads exist. (a) reads the per-Problem
        /// verdicts (questionAnswered / resolutionAgent) the walk produced and
        /// compares them against the declared roles — no prose re-parsing. (b)
        /// makes one LLM synthesis call only for declared problems whose verdict
        /// is missing or unclear.
        /// </summary>
        private async Task BuildStoryProblemCoherenceAsync(
            IReadOnlyList<StoryElement> ordered,
            CritiqueRunResult run,
            CancellationToken cancellationToken)
        {
            var declaredProblems = GetDeclaredStoryProblems(ordered);
            if (declaredProblems.Count == 0)
            {
                run.StoryProblemCoherence =
                    "I did not find a problem marked as the story problem, so I can't check whether the story's resolution answers its central question. The next revision should make clear which problem ends the story when it is resolved.";
                return;
            }

            var notes = new List<string>();
            var underdetermined = new List<StoryElement>();

            foreach (var problem in declaredProblems)
            {
                var parsed = run.ElementCritiques.FirstOrDefault(c => c.Uuid == problem.Uuid)?.Parsed;
                var answered = (parsed?.QuestionAnswered ?? string.Empty).Trim().ToLowerInvariant();
                var agent = (parsed?.ResolutionAgent ?? string.Empty).Trim().ToLowerInvariant();

                var body = GetBody(problem.Uuid);
                var protagonistName = ResolveRoleName(body, "Protagonist", "the protagonist");
                var antagonistName = ResolveRoleName(body, "Antagonist", "the antagonist");

                bool answeredKnown = answered is "yes" or "no";
                bool agentKnown = agent is "protagonist" or "antagonist" or "other" or "none";
                if (!answeredKnown && !agentKnown)
                {
                    underdetermined.Add(problem);
                    continue;
                }

                if (agent == "antagonist")
                    notes.Add($"The outline marks \"{problem.Name}\" as the story problem, with {protagonistName} opposed by {antagonistName}. But its resolution is carried by {antagonistName}, not {protagonistName}. If this is {protagonistName}'s story problem, show how {protagonistName}'s own goal, choice, or understanding resolves it rather than leaving the resolution to {antagonistName}.");
                else if (agent == "other")
                    notes.Add($"The outline marks \"{problem.Name}\" as the story problem, but its resolution is carried by someone or something other than {protagonistName}. Consider how {protagonistName}'s own choice or change resolves the problem they own.");
                else if (agent == "none")
                    notes.Add($"The outline marks \"{problem.Name}\" as the story problem, but no resolution is stated for it. Define how the problem is settled at the end.");
                else if (answered == "no")
                    notes.Add($"The outline marks \"{problem.Name}\" as the story problem, but its Outcome resolves a different thread than {protagonistName}'s goal. Check that the resolution answers the question the problem actually poses.");
                else // answered == "yes" with agent protagonist or unknown
                    notes.Add($"The story problem \"{problem.Name}\" is answered by its protagonist {protagonistName}: the question it poses is resolved by the one who owns it.");
            }

            if (underdetermined.Count > 0)
            {
                var synth = await SynthesizeCoherenceAsync(ordered, underdetermined, run, cancellationToken);
                if (!string.IsNullOrWhiteSpace(synth))
                    notes.Add(synth);
            }

            run.StoryProblemCoherence = string.Join(Environment.NewLine + Environment.NewLine, notes);
        }

        private bool HasDeclaredStoryProblem(IReadOnlyList<StoryElement> ordered) =>
            GetDeclaredStoryProblems(ordered).Count > 0;

        /// <summary>
        /// The declared story problem(s): the structured Overview.StoryProblem
        /// link wins; any Problem whose ProblemCategory marks it story/main is
        /// also included. Deduplicated.
        /// </summary>
        private List<StoryElement> GetDeclaredStoryProblems(IReadOnlyList<StoryElement> ordered)
        {
            var result = new List<StoryElement>();
            var seen = new HashSet<Guid>();

            foreach (var overview in ordered.Where(e => e.ElementType == StoryItemType.StoryOverview))
            {
                if (TryGetGuidProperty(GetBody(overview.Uuid), "StoryProblem", out var spGuid)
                    && _byUuid.TryGetValue(spGuid, out var sp)
                    && sp.ElementType == StoryItemType.Problem
                    && seen.Add(sp.Uuid))
                    result.Add(sp);
            }

            foreach (var problem in ordered.Where(e => e.ElementType == StoryItemType.Problem))
            {
                var category = TryGetStringProperty(GetBody(problem.Uuid), "ProblemCategory", out var pc) ? pc : string.Empty;
                if (ContainsAny(category, "story problem", "main") && seen.Add(problem.Uuid))
                    result.Add(problem);
            }

            return result;
        }

        private string ResolveRoleName(string body, string role, string fallback) =>
            TryGetGuidProperty(body, role, out var g) && _byUuid.TryGetValue(g, out var el)
                ? el.Name
                : fallback;

        /// <summary>
        /// (b) fallback: one LLM call to judge coherence for declared problems
        /// whose per-element verdict was missing/unclear. Failure here is
        /// non-fatal — the run still returns its deterministic findings.
        /// </summary>
        private async Task<string> SynthesizeCoherenceAsync(
            IReadOnlyList<StoryElement> ordered,
            IReadOnlyList<StoryElement> problems,
            CritiqueRunResult run,
            CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            sb.AppendLine("STORY-PROBLEM COHERENCE SYNTHESIS");
            sb.AppendLine();
            sb.AppendLine("The per-element read could not determine whether the declared story problem's resolution is driven by its protagonist. Using the data below, judge whether the Outcome answers the protagonist's question, and if not, name who carries the resolution. Respond in 1-3 plain sentences addressed to the author. Do not propose text edits.");
            sb.AppendLine();

            foreach (var overview in ordered.Where(e => e.ElementType == StoryItemType.StoryOverview))
            {
                sb.AppendLine("Story Overview:");
                sb.AppendLine(GetBody(overview.Uuid));
                sb.AppendLine();
            }
            foreach (var problem in problems)
            {
                sb.AppendLine($"Declared story problem \"{problem.Name}\":");
                sb.AppendLine(GetBody(problem.Uuid));
                sb.AppendLine();
            }

            var history = new ChatHistory();
            history.AddSystemMessage(_systemPrompt);
            history.AddUserMessage(sb.ToString());
            var settings = new OpenAIPromptExecutionSettings { Temperature = 0.4, TopP = 0.9 };

            try
            {
                using var perCallCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                perCallCts.CancelAfter(PerCallTimeout);
                var result = _kernel != null
                    ? await _chatService.GetChatMessageContentAsync(history, settings, _kernel, perCallCts.Token)
                    : await _chatService.GetChatMessageContentAsync(history, settings, null, perCallCts.Token);
                AccumulateCost(run.Cost, ExtractCost(result));
                return result.Content?.Trim() ?? string.Empty;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static List<(string Topic, string Question)> FilterKeyQuestionsForPlan(
            StoryItemType elementType,
            string mode,
            List<(string Topic, string Question)> questions)
        {
            if (questions.Count == 0)
                return questions;

            bool IsTopic((string Topic, string Question) q, params string[] parts) =>
                parts.Any(p => q.Topic.Contains(p, StringComparison.OrdinalIgnoreCase));

            bool Mentions((string Topic, string Question) q, params string[] parts) =>
                parts.Any(p => q.Question.Contains(p, StringComparison.OrdinalIgnoreCase));

            if (elementType == StoryItemType.Character)
            {
                if (mode == ModeFunctional)
                {
                    return questions
                        .Where(q => IsTopic(q, "Role", "General")
                            || Mentions(q, "cast too large", "eliminating characters", "combining"))
                        .ToList();
                }

                if (mode == ModeSupporting)
                {
                    return questions
                        .Where(q => IsTopic(q, "Role", "Relationships", "General")
                            || Mentions(q, "cast too large", "eliminating characters", "combining"))
                        .ToList();
                }
            }

            if (elementType == StoryItemType.Problem)
            {
                if (mode == ModeFunctional)
                {
                    return questions
                        .Where(q => Mentions(q,
                            "struggle between opposing forces",
                            "problem's premise",
                            "problem's theme",
                            "resolution as a scene",
                            "main or story problem"))
                        .ToList();
                }

                if (mode == ModeSupporting)
                {
                    return questions
                        .Where(q => !Mentions(q,
                            "audience should usually empathize",
                            "capable of growth",
                            "worthy opponent",
                            "evenly matched"))
                        .ToList();
                }
            }

            return questions;
        }

        private static bool IsCentralStoryRole(string storyRole) =>
            ContainsAny(storyRole, "protagonist", "antagonist", "major", "lead");

        private static bool IsFunctionalStoryRole(string storyRole) =>
            ContainsAny(storyRole, "minor", "background", "walk-on", "walk on", "cameo");

        private string BuildOverviewSignalText(IReadOnlyList<StoryElement> ordered)
        {
            var parts = new List<string>();
            foreach (var overview in ordered.Where(e => e.ElementType == StoryItemType.StoryOverview))
            {
                parts.Add(overview.Name);
                parts.Add(overview.Description);
                parts.AddRange(ExtractStringProperties(GetBody(overview.Uuid),
                    "StoryProblem", "Premise", "Concept"));
            }
            return string.Join(" ", parts);
        }

        private static string BuildProblemSignalText(StoryElement problem, string body)
        {
            var parts = new List<string>
            {
                problem.Name,
                problem.Description
            };
            parts.AddRange(ExtractStringProperties(body,
                "Premise", "Theme", "Subject", "Method",
                "ProtGoal", "ProtMotive", "ProtConflict",
                "AntagGoal", "AntagMotive", "AntagConflict"));
            return string.Join(" ", parts);
        }

        private static List<string> ExtractStringProperties(string json, params string[] propertyNames)
        {
            var values = new List<string>();
            var wanted = new HashSet<string>(propertyNames, StringComparer.OrdinalIgnoreCase);
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return values;

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (!wanted.Contains(prop.Name) || prop.Value.ValueKind != JsonValueKind.String)
                        continue;
                    var value = prop.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                        values.Add(value);
                }
            }
            catch
            {
                // Signal extraction is best-effort; malformed bodies should not
                // prevent the ordinary per-element critique from running.
            }
            return values;
        }


        internal static int ScoreTextOverlap(string left, string right)
        {
            var leftTokens = SignalTokens(left);
            var rightTokens = SignalTokens(right);
            leftTokens.IntersectWith(rightTokens);
            return leftTokens.Count;
        }

        private static HashSet<string> SignalTokens(string value)
        {
            var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match match in Regex.Matches(value ?? string.Empty, @"\b[\p{L}\p{N}][\p{L}\p{N}'-]{2,}\b"))
            {
                var token = match.Value.ToLowerInvariant().Trim('\'', '-');
                if (token.EndsWith("'s", StringComparison.Ordinal))
                    token = token[..^2];
                if (token.Length < 3 || SignalStopWords.Contains(token))
                    continue;
                tokens.Add(token);
            }
            return tokens;
        }

        private static bool ContainsAny(string value, params string[] needles) =>
            !string.IsNullOrWhiteSpace(value)
            && needles.Any(n => value.Contains(n, StringComparison.OrdinalIgnoreCase));

        private static string BlankIfEmpty(string value) =>
            string.IsNullOrWhiteSpace(value) ? "(blank)" : value;

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

        private static List<Guid> TryGetGuidListProperty(string json, string propertyName)
        {
            var values = new List<Guid>();
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return values;
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (!string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (prop.Value.ValueKind != JsonValueKind.Array)
                        return values;
                    foreach (var item in prop.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String
                            && Guid.TryParse(item.GetString(), out var g)
                            && g != Guid.Empty)
                            values.Add(g);
                    }
                    return values;
                }
            }
            catch
            {
                // ignore
            }
            return values;
        }

        // -- Key Questions personalization (issue #14) ---------------------

        /// <summary>
        /// Returns a per-element copy of the rubric with generic role nouns
        /// replaced by real names and generic pronouns replaced by the
        /// referent's actual gendered pronoun. The shared per-type list is
        /// never mutated. Setting/Scene/Overview pass through unchanged.
        /// </summary>
        private List<(string Topic, string Question)> PersonalizeKeyQuestions(
            StoryElement element, List<(string Topic, string Question)> questions)
        {
            if (questions.Count == 0) return questions;
            var body = GetBody(element.Uuid);

            switch (element.ElementType)
            {
                case StoryItemType.Character:
                {
                    var name = element.Name;
                    var sex = TryGetStringProperty(body, "Sex", out var s) ? s : null;
                    return questions
                        .Select(q => (q.Topic, PersonalizeCharacterQuestion(q.Question, name, sex)))
                        .ToList();
                }
                case StoryItemType.Problem:
                {
                    string? protName = null, protSex = null, antName = null, antSex = null;
                    Guid protGuid = Guid.Empty, antGuid = Guid.Empty;
                    if (TryGetGuidProperty(body, "Protagonist", out var pg) && _byUuid.TryGetValue(pg, out var pEl))
                    {
                        protGuid = pg;
                        protName = pEl.Name;
                        if (TryGetStringProperty(GetBody(pg), "Sex", out var ps)) protSex = ps;
                    }
                    if (TryGetGuidProperty(body, "Antagonist", out var ag) && _byUuid.TryGetValue(ag, out var aEl))
                    {
                        antGuid = ag;
                        antName = aEl.Name;
                        if (TryGetStringProperty(GetBody(ag), "Sex", out var asx)) antSex = asx;
                    }
                    // Person-vs-Self: the same character fills both roles. Compare
                    // GUIDs, not names — two distinct characters can share a name.
                    bool samePerson = protGuid != Guid.Empty && protGuid == antGuid;
                    return questions
                        .Select(q => (q.Topic, PersonalizeProblemQuestion(q.Question, protName, protSex, antName, antSex, samePerson)))
                        .ToList();
                }
                default:
                    return questions;
            }
        }

        internal static string PersonalizeCharacterQuestion(string text, string name, string? sex)
        {
            // "the / your / this character" -> the character's name.
            text = Regex.Replace(text, @"\b(the|your|this)\s+character\b", name, RegexOptions.IgnoreCase);
            // Pronouns refer to this single character.
            return ApplyGenderedPronouns(text, sex, name);
        }

        internal static string PersonalizeProblemQuestion(
            string text, string? protName, string? protSex, string? antName, string? antSex,
            bool samePerson = false)
        {
            // Walk the original text once. Each role noun ("protagonist" /
            // "antagonist") sets the current referent, so a pronoun that follows
            // is gendered by THAT role's character — even when the same sentence
            // also names the other role ("...are the protagonist and antagonist
            // evenly matched?"). A question is always about whichever role most
            // recently appeared before the pronoun.
            //
            // Before any role noun appears, default to the protagonist: the
            // generic "what does the character have at stake?" questions read as
            // protagonist questions. This is a deliberate default, not a bug.
            string? curSex = protSex;
            string curName = protName ?? "the character";

            const string pattern =
                @"\b(the|your)\s+(protagonist|antagonist)\b|\b(protagonist|antagonist)\b|\b(he|she|him|her|his|hers)\b";

            return Regex.Replace(text, pattern, m =>
            {
                string? role = m.Groups[2].Success ? m.Groups[2].Value
                             : m.Groups[3].Success ? m.Groups[3].Value
                             : null;
                if (role != null)
                {
                    bool isAnt = role.Equals("antagonist", StringComparison.OrdinalIgnoreCase);
                    curSex  = isAnt ? antSex : protSex;
                    string fallbackName = isAnt ? "the antagonist" : "the protagonist";
                    string? name = isAnt ? antName : protName;
                    curName = !string.IsNullOrWhiteSpace(name) ? name : fallbackName;

                    // "his/her own antagonist" is structural language for
                    // Person-vs-Self. Do not render "Becky must be her own
                    // Joseph" when the current problem's antagonist is a
                    // separate character.
                    if (isAnt && IsOwnRoleReference(text, m.Index))
                    {
                        curSex = protSex;
                        curName = protName ?? curName;
                        return m.Groups[1].Success ? $"{m.Groups[1].Value} {role}" : role;
                    }

                    // Person-vs-Self: keep the literal word "antagonist" for the
                    // second role so we never render "Becky and Becky" — the
                    // pronoun referent is still set above, gendered to that person.
                    if (isAnt && samePerson)
                        return m.Groups[1].Success ? $"{m.Groups[1].Value} {role}" : role;

                    if (string.IsNullOrEmpty(name)) return m.Value;  // no name on file — leave as written
                    return name;
                }

                // Pronoun — gender by the most recent role's character.
                return GenderPronoun(m.Value, curSex, curName, text, m.Index + m.Length);
            }, RegexOptions.IgnoreCase);
        }

        private static bool IsOwnRoleReference(string text, int roleIndex)
        {
            var prefixStart = Math.Max(0, roleIndex - 12);
            var prefix = text.Substring(prefixStart, roleIndex - prefixStart);
            return Regex.IsMatch(prefix, @"\bown\s+$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Replaces generic third-person pronouns with the referent's actual
        /// gendered forms. When Sex is unknown, falls back to the name (and
        /// "Name's" for the possessive) rather than guessing gender.
        /// </summary>
        private static string ApplyGenderedPronouns(string text, string? sex, string name)
        {
            return Regex.Replace(text, @"\b(he|she|him|her|his|hers)\b",
                m => GenderPronoun(m.Value, sex, name, text, m.Index + m.Length),
                RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Genders a single pronoun token to the referent's sex, falling back to
        /// the name when sex is unknown. <paramref name="fullText"/> and
        /// <paramref name="afterIndex"/> disambiguate "her" (possessive vs object).
        /// </summary>
        private static string GenderPronoun(string token, string? sex, string name, string fullText, int afterIndex)
        {
            bool male   = string.Equals(sex, "Male", StringComparison.OrdinalIgnoreCase);
            bool female = string.Equals(sex, "Female", StringComparison.OrdinalIgnoreCase);

            string subj, obj, poss, possIndep;
            if (male)        { subj = "he";  obj = "him"; poss = "his";       possIndep = "his"; }
            else if (female) { subj = "she"; obj = "her"; poss = "her";       possIndep = "hers"; }
            else             { subj = name;  obj = name;  poss = name + "'s"; possIndep = name + "'s"; }

            string lower = token.ToLowerInvariant();
            string repl = lower switch
            {
                "he" or "she" => subj,
                "him"         => obj,
                "his"         => poss,
                "hers"        => possIndep,
                "her"         => IsPossessiveContext(fullText, afterIndex) ? poss : obj,
                _             => token
            };
            // Preserve a leading capital (sentence start).
            if (char.IsUpper(token[0]) && repl.Length > 0)
                repl = char.ToUpperInvariant(repl[0]) + repl.Substring(1);
            return repl;
        }

        // "her" is possessive when immediately followed by whitespace + a word
        // ("her outlook"); object otherwise ("visualize her?").
        private static bool IsPossessiveContext(string text, int afterIndex)
        {
            int i = afterIndex;
            while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
            return i < text.Length && char.IsLetter(text[i]);
        }

        private static bool TryGetStringProperty(string json, string propertyName, out string value)
        {
            value = string.Empty;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return false;
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (!string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        var s = prop.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) { value = s; return true; }
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
        public static string RenderReport(
            CritiqueRunResult run,
            string outlineDisplayName,
            bool separateKeyQuestions = false)
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

            RenderHighlights(sb, run);

            if (!string.IsNullOrWhiteSpace(run.StoryProblemCoherence))
            {
                sb.AppendLine("## Story Problem Check");
                sb.AppendLine();
                sb.AppendLine(run.StoryProblemCoherence);
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(run.StructuralCompleteness))
            {
                sb.AppendLine("## Structural Completeness");
                sb.AppendLine();
                sb.AppendLine(run.StructuralCompleteness);
                sb.AppendLine();
            }

            var authorFacingAnchors = BuildAuthorFacingAnchors(run);
            foreach (var critique in run.ElementCritiques)
                RenderElement(sb, critique, authorFacingAnchors);

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

        private static void RenderHighlights(StringBuilder sb, CritiqueRunResult run)
        {
            var highlights = run.ElementCritiques
                .Where(c => c.Parsed != null)
                .Where(c => c.CritiqueMode != ModeFunctional && c.CritiqueMode != ModeContext)
                .OrderBy(c => HighlightRank(c))
                .SelectMany(c => c.Parsed!.Strengths.Select(s => (c.Name, c.ElementType, s.Finding)))
                .Where(s => !string.IsNullOrWhiteSpace(s.Finding))
                .Take(4)
                .ToList();

            if (highlights.Count == 0)
                return;

            sb.AppendLine("## What's Working");
            sb.AppendLine();
            foreach (var item in highlights)
                sb.AppendLine($"- **{item.Name}**: {item.Finding}");
            sb.AppendLine();
        }

        private static int HighlightRank(ElementCritique critique)
        {
            if (critique.ElementType == "Overview") return 0;
            if (critique.CritiqueMode == ModeSpine) return 1;
            if (critique.CritiqueMode == ModeFull) return 2;
            if (critique.CritiqueMode == ModeSupporting) return 3;
            return 4;
        }

        private static HashSet<string> BuildAuthorFacingAnchors(CritiqueRunResult run)
        {
            var anchors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var critique in run.ElementCritiques.Where(c => !IsPeripheralElement(c)))
            {
                var name = critique.Name?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (name.Length >= 4)
                    anchors.Add(name);

                foreach (var term in ExtractAuthorAnchorTerms(name))
                    anchors.Add(term);
            }

            return anchors;
        }

        private static bool IsPeripheralElement(ElementCritique critique) =>
            critique.CritiqueMode == ModeFunctional
            || critique.CritiqueMode == ModeContext;

        private static List<CritiqueFinding> FilterPeripheralConcerns(
            ElementCritique critique,
            IReadOnlySet<string> storyAnchors)
        {
            if (critique.Parsed == null)
                return new List<CritiqueFinding>();

            if (storyAnchors.Count == 0)
                return critique.Parsed.Concerns;

            return critique.Parsed.Concerns
                .Where(f => IsActionablePeripheralFinding(f, critique, storyAnchors))
                .ToList();
        }

        private static bool IsActionablePeripheralFinding(
            CritiqueFinding finding,
            ElementCritique critique,
            IReadOnlySet<string> storyAnchors)
        {
            if (string.IsNullOrWhiteSpace(finding.Finding))
                return false;

            var ownTerms = ExtractAuthorAnchorTerms(critique.Name)
                .Append(critique.Name)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var anchor in storyAnchors)
            {
                if (ownTerms.Contains(anchor))
                    continue;

                if (ContainsAnchor(finding.Finding, anchor))
                    return true;
            }

            return false;
        }

        private static IEnumerable<string> ExtractAuthorAnchorTerms(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            foreach (Match match in Regex.Matches(text, "[A-Za-z][A-Za-z']+"))
            {
                var term = match.Value.Trim('\'');
                if (term.Length < 5)
                    continue;
                if (SignalStopWords.Contains(term))
                    continue;
                if (IsGenericAuthorAnchor(term))
                    continue;

                yield return term;
            }
        }

        private static bool IsGenericAuthorAnchor(string term) =>
            ContainsAny(term,
                "change", "changed", "changes", "clear", "unclear",
                "theme", "themes", "conflict", "conflicts", "central",
                "function", "details", "reader", "readers", "outline",
                "emotion", "emotional", "physical", "specific", "storycad");

        private static bool ContainsAnchor(string text, string anchor)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(anchor))
                return false;

            if (anchor.Any(char.IsWhiteSpace))
                return text.Contains(anchor, StringComparison.OrdinalIgnoreCase);

            return Regex.IsMatch(
                text,
                $@"\b{Regex.Escape(anchor)}\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        /// <summary>
        /// Consolidated Key Questions section, emitted once after the per-element
        /// critiques when the user prefers the rubric out of line. Each element's
        /// (personalized) questions appear under its own heading.
        /// </summary>
        private static void RenderKeyQuestionsAppendix(StringBuilder sb, CritiqueRunResult run)
        {
            var withQuestions = run.ElementCritiques.Where(c => c.KeyQuestions.Count > 0).ToList();
            if (withQuestions.Count == 0) return;

            sb.AppendLine("## Key Questions");
            sb.AppendLine();
            sb.AppendLine("_The rubric each element was critiqued against._");
            sb.AppendLine();
            foreach (var critique in withQuestions)
            {
                sb.AppendLine($"### [{critique.ElementType}] {critique.Name}");
                sb.AppendLine();
                foreach (var grp in critique.KeyQuestions.GroupBy(q => q.Topic))
                {
                    sb.AppendLine($"**{grp.Key}**");
                    foreach (var q in grp)
                        sb.AppendLine($"- {q.Question}");
                    sb.AppendLine();
                }
            }
        }

        private static void RenderElement(
            StringBuilder sb,
            ElementCritique critique,
            IReadOnlySet<string> storyAnchors)
        {
            List<CritiqueFinding>? authorFacingConcerns = null;
            var conciseElement = IsPeripheralElement(critique);
            if (conciseElement && critique.Parsed != null)
            {
                authorFacingConcerns = FilterPeripheralConcerns(critique, storyAnchors);
                if (authorFacingConcerns.Count == 0)
                    return;
            }

            sb.AppendLine($"## [{critique.ElementType}] {critique.Name}");
            sb.AppendLine();

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
            var concerns = authorFacingConcerns ?? p.Concerns;

            if (!conciseElement)
                RenderSection(sb, "Strengths", p.Strengths);
            RenderSection(sb, "Concerns", concerns);

            if (!conciseElement && p.QuestionsForAuthor.Count > 0)
            {
                sb.AppendLine("### Questions for the author");
                sb.AppendLine();
                foreach (var q in p.QuestionsForAuthor)
                    sb.AppendLine($"- {q}");
                sb.AppendLine();
            }

            if (p.Strengths.Count == 0 && concerns.Count == 0 && p.QuestionsForAuthor.Count == 0)
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
                sb.AppendLine($"- {item.Finding}");
            sb.AppendLine();
        }
    }
}
