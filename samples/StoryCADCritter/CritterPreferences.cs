using System;
using System.IO;
using System.Text.Json;

namespace StoryCADCritter;

/// <summary>
/// User-tunable Critter settings, persisted to
/// %LocalAppData%/StoryCADCritter/preferences.json. Read at the start of each
/// run, so changes take effect on the next Run Critique — no restart needed.
/// </summary>
public sealed class CritterPreferences
{
    /// <summary>Max elements critiqued in parallel. 1..16; default 8.</summary>
    public int MaxConcurrency { get; set; } = 8;

    /// <summary>
    /// Where the Key Questions rubric appears in the report:
    /// "Inline" (a &lt;details&gt; block per element) or "Separate"
    /// (one consolidated section after the per-element critiques).
    /// </summary>
    public string KeyQuestionsPlacement { get; set; } = "Inline";

    /// <summary>
    /// OpenAI model id. Empty means "fall back to OPENAI_MODEL env var, then
    /// the hardcoded default (gpt-4o-mini)".
    /// </summary>
    public string SelectedModelId { get; set; } = string.Empty;
}

/// <summary>
/// Loads and saves <see cref="CritterPreferences"/>. Tolerant of a missing or
/// corrupted file — falls back to defaults rather than throwing.
/// </summary>
public sealed class PreferencesService
{
    private static readonly string DefaultPrefsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "StoryCADCritter",
        "preferences.json");

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    private readonly string _prefsPath;

    public PreferencesService() : this(DefaultPrefsPath) { }

    public PreferencesService(string prefsPath) => _prefsPath = prefsPath;

    public CritterPreferences Load()
    {
        if (!File.Exists(_prefsPath))
            return new CritterPreferences();
        try
        {
            var json = File.ReadAllText(_prefsPath);
            return JsonSerializer.Deserialize<CritterPreferences>(json) ?? new CritterPreferences();
        }
        catch
        {
            return new CritterPreferences();
        }
    }

    public void Save(CritterPreferences prefs)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_prefsPath)!);
            File.WriteAllText(_prefsPath, JsonSerializer.Serialize(prefs, JsonOpts));
        }
        catch
        {
            // Best-effort persistence; don't crash the app over this.
        }
    }
}
