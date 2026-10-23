using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NLog.Extensions.Logging;
using StoryCADLib.Models;
using StoryCADLib.Services.API;
using StoryCADLib.Services.Collaborator.Contracts;
using StoryCADLib.Services.Logging;
using StoryCollaborator.Models;
using StoryCollaborator.Services;
using StoryCollaborator.Workflows;

namespace StoryCADMcp.Tools;

/// <summary>
/// Collaborator #291, design sections 5.4 to 5.9 and 6. Each tool calls
/// <see cref="WorkflowSessionService"/>, the same core the Collaborator window uses. One session
/// at a time: a new <c>run_workflow</c> ends the one before it, and <c>close_outline</c> ends it.
/// </summary>
[McpServerToolType]
public static class CollaboratorTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static WorkflowSession? _session;
    private static CollaboratorSettings _settings = CollaboratorSettings.Default;
    private static int _busy;

    // The fields Collaborator wrote belong to the outline, not to one run (Collaborator #272).
    private static Dictionary<string, string> _touched = new(StringComparer.OrdinalIgnoreCase);
    private static StoryModel? _touchedModel;

    // One backup per outline, before the first write (design section 5.8).
    private static StoryModel? _backedUpModel;

    internal static void UseSession(WorkflowSession? session) => _session = session;

    internal static void EndSession()
    {
        _session = null;
        _backedUpModel = null;
    }

    /// <summary>True while a Collaborator session is open.</summary>
    internal static bool HasSession => _session != null;

    #region Tools

    [McpServerTool(Name = "list_workflows")]
    [Description("Lists the Collaborator workflows: label, title, what each does, the element type it works on, and the inputs it needs. Use the label with run_workflow.")]
    public static string ListWorkflows()
    {
        var list = WorkflowRegistry.All.Select(w =>
        {
            var io = w.GetIO();
            return new
            {
                label = w.Label,
                title = w.Title,
                description = w.Description,
                elementType = w.PrimaryElementType.ToString(),
                requiredInputs = io.RequiredInputs.Select(i => new { name = i.ElementLabel, type = i.ElementType.ToString() }),
                optionalInputs = io.OptionalInputs.Select(i => new { name = i.ElementLabel, type = i.ElementType.ToString() })
            };
        });
        return Out(JsonSerializer.Serialize(list, JsonOptions));
    }

    [McpServerTool(Name = "run_workflow")]
    [Description("Runs a Collaborator workflow on one element of the open outline and keeps its proposals. Nothing is written to the outline until accept or accept_all. A new run ends the session before it. Takes a minute or more and costs credit.")]
    public static async Task<string> RunWorkflow(
        StoryCADApi api,
        [Description("Workflow label from list_workflows")] string workflow,
        [Description("GUID of the element the run is about (from list_elements)")] string element,
        [Description("Optional: input name to element GUID, for inputs the run cannot find on its own")] Dictionary<string, string>? inputs = null)
    {
        if (api.CurrentModel == null)
            return Out("Error: No outline is currently open. Call open_outline first.");

        var chosen = WorkflowRegistry.All.FirstOrDefault(w =>
            string.Equals(w.Label, workflow, StringComparison.OrdinalIgnoreCase)
            || string.Equals(w.Title, workflow, StringComparison.OrdinalIgnoreCase));
        if (chosen == null)
            return Out($"Error: No workflow named '{workflow}'. Labels: {string.Join(", ", WorkflowRegistry.All.Select(w => w.Label))}");

        if (!Guid.TryParse(element, out var elementGuid))
            return Out($"Error: '{element}' is not a GUID. Use list_elements to find one.");
        var primary = api.GetStoryElement(elementGuid);
        if (!primary.IsSuccess)
            return Out($"Error: {primary.ErrorMessage}");
        if (chosen.PrimaryElementType != StoryItemType.Unknown
            && primary.Payload.ElementType != chosen.PrimaryElementType)
            return Out($"Error: {chosen.Title} works on a {chosen.PrimaryElementType}, but '{primary.Payload.Name}' is a "
                       + $"{primary.Payload.ElementType}. Use list_elements with type {chosen.PrimaryElementType} to find one.");

        var io = chosen.GetIO();
        var inputNames = io.RequiredInputs.Concat(io.OptionalInputs).Select(i => i.ElementLabel).ToList();
        var given = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in inputs ?? new Dictionary<string, string>())
        {
            if (!inputNames.Contains(pair.Key, StringComparer.OrdinalIgnoreCase))
                return Out($"Error: {chosen.Title} has no input named '{pair.Key}'. Inputs: {string.Join(", ", inputNames)}.");
            if (!Guid.TryParse(pair.Value, out var inputGuid))
                return Out($"Error: Input '{pair.Key}' is not a GUID: '{pair.Value}'.");
            if (!api.GetStoryElement(inputGuid).IsSuccess)
                return Out($"Error: Input '{pair.Key}' names no element of this outline: '{pair.Value}'.");
            given[pair.Key] = inputGuid;
        }

        if (!TryEnter(out var busyError))
            return Out(busyError);
        try
        {
            // Read the app's sign-in again: a token the app refreshed since the last run is used.
            var signInError = await CollaboratorSignIn.EnsureAsync();
            if (signInError != null)
                return Out($"Error: {signInError}");

            // Finding inputs can write to the outline (it creates a StoryWorld, sets a ProblemCategory),
            // so the backup comes first.
            var backupError = BackupOnce(api);
            if (backupError != null)
                return Out(backupError);

            if (!ReferenceEquals(_touchedModel, api.CurrentModel))
            {
                _touched = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _touchedModel = api.CurrentModel;
            }

            var session = new WorkflowSession(chosen, api.CurrentModel)
            {
                FilePath = OutlineTools.OpenedPath,
                TouchedFields = _touched
            };
            var service = Service(api);
            var resolution = service.ResolveInputs(session, primary.Payload, given);
            if (resolution.Cancelled || resolution.Failed)
            {
                var reason = string.IsNullOrWhiteSpace(resolution.BailReason)
                    ? $"Cannot find the input(s): {string.Join(", ", resolution.Missing)}. "
                      + "Use list_elements to find each GUID and pass it in inputs, for example {\"" + (resolution.Missing.FirstOrDefault() ?? "Name") + "\": \"<guid>\"}."
                    : resolution.BailReason;
                return Out($"Error: {reason}");
            }

            _session = session;
            if (chosen.Mode == WorkflowMode.Conversational)
                return Out(OpenInterview(service, session));
            return Out(await RunAndReport(service, session, () => service.RunAsync(session)));
        }
        finally
        {
            Leave();
        }
    }

    [McpServerTool(Name = "get_proposals")]
    [Description("Lists each proposal of the session: key, element, property, state (New, Refresh, or Has your text), current text, new text, explanation, and status (Open, Accepted, or Skipped).")]
    public static string GetProposals()
    {
        var session = _session;
        if (session?.Proposals == null)
            return Out("Error: No proposals. Call run_workflow first.");
        return Out(JsonSerializer.Serialize(session.Proposals.All.Select(Describe), JsonOptions));
    }

    [McpServerTool(Name = "accept")]
    [Description("Accepts one proposal and writes it to the outline. A proposal in state 'Has your text' replaces text you wrote, so it needs overwrite_your_text set to true.")]
    public static async Task<string> Accept(
        StoryCADApi api,
        [Description("Proposal key from get_proposals")] string key,
        [Description("Set true to replace text you wrote. Default false.")] bool overwrite_your_text = false)
    {
        if (!TryEnter(out var busyError))
            return Out(busyError);
        try
        {
            var problem = CheckOpenProposal(api, key, out var session, out var resolved);
            if (problem != null)
                return Out(problem);

            var pending = session!.Result?.PendingUpdates
                .FirstOrDefault(u => string.Equals(u.Key, resolved, StringComparison.OrdinalIgnoreCase));
            if (pending == null)
                return Out(OpenKeysError(session, $"'{key}' is not an open proposal."));

            var isProtect = pending.Kind == UpdateKind.Protect;
            if (isProtect && !overwrite_your_text)
                return Out($"Error: '{resolved}' replaces text you wrote. Nothing was written. "
                           + "Call accept again with overwrite_your_text true to replace it, or skip it.");

            var backupError = BackupOnce(api);
            if (backupError != null)
                return Out(backupError);

            var service = Service(api);
            var writes = session.WriteCount;
            var accepted = await service.AcceptAsync(session, resolved!);
            if (accepted && isProtect)
                await service.ApplyStagedAsync(session, true);

            if (!accepted || session.WriteCount == writes)
                return Out($"Error: '{resolved}' could not be written to the outline.");
            var saveError = await SaveCheckAsync(api, session);
            if (saveError != null)
                return Out(saveError);
            return Out(JsonSerializer.Serialize(Describe(session.Proposals!.Get(resolved!)!), JsonOptions));
        }
        finally
        {
            Leave();
        }
    }

    [McpServerTool(Name = "skip")]
    [Description("Skips one proposal. The outline keeps its text.")]
    public static string Skip(
        StoryCADApi api,
        [Description("Proposal key from get_proposals")] string key)
    {
        if (!TryEnter(out var busyError))
            return Out(busyError);
        try
        {
            var session = _session;
            if (session?.Proposals == null)
                return Out("Error: No proposals. Call run_workflow first.");

            var resolved = session.Proposals.ResolveKey(key);
            if (resolved == null || !Service(api).Skip(session, resolved))
                return Out(OpenKeysError(session, $"'{key}' is not an open proposal."));
            return Out(JsonSerializer.Serialize(Describe(session.Proposals.Get(resolved)!), JsonOptions));
        }
        finally
        {
            Leave();
        }
    }

    [McpServerTool(Name = "accept_all")]
    [Description("Accepts every open proposal that does not replace your text. With overwrite_your_text true it replaces your text as well.")]
    public static async Task<string> AcceptAll(
        StoryCADApi api,
        [Description("Set true to replace text you wrote. Default false.")] bool overwrite_your_text = false)
    {
        var session = _session;
        if (session?.Proposals == null)
            return Out("Error: No proposals. Call run_workflow first.");
        if (!ReferenceEquals(session.Model, api.CurrentModel))
            return Out("Error: The outline changed since the run. Call run_workflow again.");

        if (!TryEnter(out var busyError))
            return Out(busyError);
        try
        {
            var backupError = BackupOnce(api);
            if (backupError != null)
                return Out(backupError);

            session.LastAppliedKeys.Clear();
            await Service(api).AcceptAllAsync(session, overwrite_your_text);
            if (session.LastAppliedKeys.Count > 0)
            {
                var saveError = await SaveCheckAsync(api, session);
                if (saveError != null)
                    return Out(saveError);
            }

            return Out(JsonSerializer.Serialize(new
            {
                applied = session.LastAppliedKeys.ToList(),
                open = OpenKeys(session)
            }, JsonOptions));
        }
        finally
        {
            Leave();
        }
    }

    [McpServerTool(Name = "try_again")]
    [Description("Runs the workflow again. The open and skipped proposals go to the Worker as what to depart from. Takes a minute or more and costs credit.")]
    public static async Task<string> TryAgain(StoryCADApi api)
    {
        var session = _session;
        if (session == null)
            return Out("Error: No session. Call run_workflow first.");
        if (!ReferenceEquals(session.Model, api.CurrentModel))
            return Out("Error: The outline changed since the run. Call run_workflow again.");

        if (!TryEnter(out var busyError))
            return Out(busyError);
        try
        {
            var signInError = await CollaboratorSignIn.EnsureAsync();
            if (signInError != null)
                return Out($"Error: {signInError}");

            var service = Service(api);
            return Out(await RunAndReport(service, session, () => service.TryAgainAsync(session)));
        }
        finally
        {
            Leave();
        }
    }

    [McpServerTool(Name = "chat")]
    [Description("Sends one chat message about the proposals. The reply can change proposals; it never writes the outline. Costs credit.")]
    public static async Task<string> Chat(
        StoryCADApi api,
        [Description("The message")] string text)
    {
        var session = _session;
        if (session == null || (session.Proposals == null && !session.IsInterview))
            return Out("Error: No proposals to chat about. Call run_workflow first.");
        if (!ReferenceEquals(session.Model, api.CurrentModel))
            return Out("Error: The outline changed since the run. Call run_workflow again.");

        if (!TryEnter(out var busyError))
            return Out(busyError);
        try
        {
            var signInError = await CollaboratorSignIn.EnsureAsync();
            if (signInError != null)
                return Out($"Error: {signInError}");

            var before = Snapshot(session);
            var interview = session.IsInterview;
            var clock = Stopwatch.StartNew();
            string reply;
            try
            {
                reply = await Service(api).ChatAsync(session, text);
            }
            catch (Exception ex)
            {
                return Out($"Error: {session.LastError ?? ex.Message}");
            }

            clock.Stop();
            if (session.LastError != null)
                return Out($"Error: {session.LastError}");

            var after = Snapshot(session);
            return Out(JsonSerializer.Serialize(new
            {
                reply,
                // In an interview the next question is the pending question; the opening turn returns no reply text.
                question = interview ? session.InterviewPendingQuestion : null,
                changed = after.Where(p => !before.TryGetValue(p.Key, out var old) || old != p.Value)
                    .Select(p => p.Key).ToList(),
                seconds = Math.Round(clock.Elapsed.TotalSeconds, 1),
                cost = Cost(session.LastChatCost)
            }, JsonOptions));
        }
        finally
        {
            Leave();
        }
    }

    [McpServerTool(Name = "settings")]
    [Description("Shows or changes the Collaborator settings that the next run sends. Omit an argument to leave it as it is.")]
    public static string Settings(
        [Description("Concise, Balanced, or Detailed")] string? terseness = null,
        [Description("Preferred genres, comma-separated")] string? genre_preferences = null,
        [Description("Story forms you like, comma-separated")] string? story_form_likes = null,
        [Description("Story forms you want to avoid, comma-separated")] string? story_form_dislikes = null)
    {
        if (!TryEnter(out var busyError))
            return Out(busyError);
        try
        {
        if (terseness != null)
        {
            if (!Enum.TryParse<TersenessLevel>(terseness, true, out var level))
                return Out("Error: terseness must be Concise, Balanced, or Detailed.");
            _settings.Terseness = level;
        }

        if (genre_preferences != null) _settings.GenrePreferences = genre_preferences;
        if (story_form_likes != null) _settings.StoryFormLikes = story_form_likes;
        if (story_form_dislikes != null) _settings.StoryFormDislikes = story_form_dislikes;

        return Out(JsonSerializer.Serialize(new
        {
            terseness = _settings.Terseness.ToString(),
            genrePreferences = _settings.GenrePreferences,
            storyFormLikes = _settings.StoryFormLikes,
            storyFormDislikes = _settings.StoryFormDislikes
        }, JsonOptions));
        }
        finally
        {
            Leave();
        }
    }

    #endregion

    #region Helpers

    // Same logging as the app's Collaborator (StoryCAD CollaboratorLib/Collaborator.cs): the model's
    // answer and each Classify line go to the NLog file in the server's logs folder (Collaborator #286).
    private static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
    {
        builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        if (NLog.LogManager.Configuration != null)
            builder.AddNLog(NLog.LogManager.Configuration);
    });

    /// <summary>The service keeps no state, so a new one for each call is the same service.</summary>
    private static WorkflowSessionService Service(StoryCADApi api) =>
        new(api, LoggerFactory.CreateLogger<WorkflowSessionService>(), _settings, LoggerFactory,
            Ioc.Default.GetService<ILogService>());

    /// <summary>Runs a run or a try again and reports the proposals, the time and the cost.</summary>
    private static async Task<string> RunAndReport(
        WorkflowSessionService service, WorkflowSession session, Func<Task> run)
    {
        var before = session.Result;
        var clock = Stopwatch.StartNew();
        await run();
        clock.Stop();

        // A failed run leaves the last good result in place, so a changed result means success.
        if (ReferenceEquals(before, session.Result))
            return $"Error: {session.LastError ?? "The run did not finish."}"
                   + (session.Result != null ? " The proposals of the last good run are still open." : "");

        var result = session.Result!;
        var cost = result.Cost;
        return JsonSerializer.Serialize(new
        {
            workflow = session.Workflow.Label,
            proposals = session.Proposals?.All.Select(Describe).ToList(),
            kept = result.KeptProperties,
            seconds = Math.Round(clock.Elapsed.TotalSeconds, 1),
            cost = Cost(cost)
        }, JsonOptions);
    }

    private static object Describe(SessionProposalSet.Entry e) => new
    {
        key = e.Update.Key,
        element = e.Update.ElementLabel,
        property = e.Update.Spec.Property,
        state = e.Update.Kind switch
        {
            UpdateKind.Refresh => "Refresh",
            UpdateKind.Protect => "Has your text",
            _ => "New"
        },
        current = e.OutlineText,
        proposed = e.ProposedText,
        explanation = e.Update.CraftExplanation,
        status = e.Status.ToString()
    };

    private static object? Cost(ProxyCostInfo? cost) => cost == null
        ? null
        : new { usd = cost.CostUsd, model = cost.Model, inputTokens = cost.InputTokens, outputTokens = cost.OutputTokens };

    /// <summary>
    /// Opens a Character Interview the way the app does: <c>StartInterview</c>, then the writer picks
    /// what to explore. The AI session answers through <c>chat</c>.
    /// </summary>
    private static string OpenInterview(WorkflowSessionService service, WorkflowSession session)
    {
        service.StartInterview(session);
        session.Elements.TryGetValue("Character", out var character);
        return JsonSerializer.Serialize(new
        {
            workflow = session.Workflow.Label,
            interview = true,
            character = session.InterviewCharacterName,
            message = $"Answer as {session.InterviewCharacterName}, in their voice, in first person, with the chat tool. "
                      + "Nothing is written to the character until the interview is saved, and the MCP server cannot save an interview yet. "
                      + "First say what to explore.",
            choices = InterviewChoice.ListText(session.InterviewCharacterName,
                InterviewScript.BlankFields(character as CharacterModel))
        }, JsonOptions);
    }

    private static Dictionary<string, string> Snapshot(WorkflowSession session) =>
        session.Proposals == null
            ? new Dictionary<string, string>()
            : session.Proposals.All.ToDictionary(e => e.Update.Key, e => e.Status + "|" + e.ProposedText);

    private static List<string> OpenKeys(WorkflowSession session) =>
        session.Proposals!.All.Where(e => e.Status == ProposalSessionStatus.Open)
            .Select(e => e.Update.Key).ToList();

    private static string OpenKeysError(WorkflowSession session, string message) =>
        $"Error: {message} Open keys: {string.Join(", ", OpenKeys(session))}";

    /// <summary>Checks the session and finds the key. Returns an error message, or null.</summary>
    private static string? CheckOpenProposal(
        StoryCADApi api, string key, out WorkflowSession? session, out string? resolved)
    {
        session = _session;
        resolved = null;
        if (session?.Proposals == null)
            return "Error: No proposals. Call run_workflow first.";
        if (!ReferenceEquals(session.Model, api.CurrentModel))
            return "Error: The outline changed since the run. Call run_workflow again.";

        resolved = session.Proposals.ResolveKey(key);
        var entry = resolved == null ? null : session.Proposals.Get(resolved);
        if (entry == null)
            return OpenKeysError(session, $"No proposal is named '{key}'.");
        if (entry.Status != ProposalSessionStatus.Open)
            return OpenKeysError(session, $"'{resolved}' is already {entry.Status.ToString().ToLowerInvariant()}.");
        return null;
    }

    /// <summary>
    /// The service saves the outline file after a write but does not report a failed save. Wait
    /// for that save, then save once more and report the result, so a caller is never told a
    /// write is on disk when it is not. Returns an error message, or null.
    /// </summary>
    private static async Task<string?> SaveCheckAsync(StoryCADApi api, WorkflowSession session)
    {
        if (session.PendingSave != null)
        {
            try
            {
                await session.PendingSave;
            }
            catch
            {
                // The final save below reports the outcome.
            }
        }

        if (string.IsNullOrEmpty(session.FilePath))
            return "Error: The outline has no file yet, so the write is in memory only. Call save_outline with a path.";

        var saved = await api.WriteOutline(session.FilePath);
        return saved.IsSuccess
            ? null
            : $"Error: The write is in the outline in memory, but saving the file failed: {saved.ErrorMessage}";
    }

    /// <summary>Where the backups go. Tests point this at a folder of their own.</summary>
    internal static string? BackupFolderOverride { get; set; }

    internal static string BackupFolder => BackupFolderOverride ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StoryCADMcp", "Backups");

    /// <summary>
    /// One backup of the outline file before the first write to an outline (design section 5.8):
    /// a zip of the file on disk in <see cref="BackupFolder"/>. Returns an error message when the
    /// backup fails, and the write must not go on. An outline with no file yet has nothing to back up.
    /// </summary>
    internal static string? BackupOnce(StoryCADApi api)
    {
        if (ReferenceEquals(_backedUpModel, api.CurrentModel))
            return null;

        var source = OutlineTools.OpenedPath;
        if (string.IsNullOrEmpty(source) || !File.Exists(source))
        {
            _backedUpModel = api.CurrentModel;
            return null;
        }

        try
        {
            Directory.CreateDirectory(BackupFolder);
            var name = $"{Path.GetFileNameWithoutExtension(source)} as of {DateTime.Now:yyyyMMdd_HHmmss}.zip";
            var zipPath = Path.Combine(BackupFolder, name);
            using (var zip = System.IO.Compression.ZipFile.Open(zipPath, System.IO.Compression.ZipArchiveMode.Create))
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, source, Path.GetFileName(source));
            _backedUpModel = api.CurrentModel;
            return null;
        }
        catch (Exception ex)
        {
            return $"Error: The backup failed, so nothing was written: {ex.Message}";
        }
    }

    /// <summary>save_outline is a write too. During a Collaborator session it backs up first.</summary>
    internal static string? BackupBeforeSave(StoryCADApi api) => HasSession ? BackupOnce(api) : null;

    internal static bool TryEnter(out string error)
    {
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
        {
            error = "Error: A Collaborator call is still running. Wait for it to finish.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    internal static void Leave() => Interlocked.Exchange(ref _busy, 0);

    /// <summary>Every tool result passes through here, so the token never leaves the server.</summary>
    private static string Out(string text) => CollaboratorSignIn.Scrub(text);

    #endregion
}
