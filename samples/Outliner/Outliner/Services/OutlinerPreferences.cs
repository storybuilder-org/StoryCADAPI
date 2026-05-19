using System;
using System.IO;
using System.Text.Json;

namespace Outliner.Services
{
    /// <summary>
    /// User-tunable Outliner settings, persisted to
    /// %LocalAppData%/Outliner/preferences.json.
    /// </summary>
    public sealed class OutlinerPreferences
    {
        /// <summary>Mode the app opens in: "Single" or "Batch".</summary>
        public string StartupMode { get; set; } = "Single";

        /// <summary>Default starting folder for input prose pickers (both modes).</summary>
        public string DefaultInputFolder  { get; set; } = string.Empty;

        /// <summary>Default starting folder for output outline pickers (both modes).</summary>
        public string DefaultOutputFolder { get; set; } = string.Empty;

        /// <summary>Last folder the user actually picked in batch mode (drives auto-population).</summary>
        public string LastBatchInputFolder  { get; set; } = string.Empty;
        public string LastBatchOutputFolder { get; set; } = string.Empty;

        /// <summary>
        /// OpenAI model id used for analysis. Empty means "fall back to OPENAI_MODEL
        /// env var, then the hardcoded default". Applied at app startup; change
        /// takes effect on next launch.
        /// </summary>
        public string SelectedModelId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Loads and saves OutlinerPreferences. Tolerant of missing or corrupted
    /// files — falls back to defaults rather than throwing during startup.
    /// </summary>
    public sealed class PreferencesService
    {
        private static readonly string DefaultPrefsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Outliner",
            "preferences.json");

        private static readonly JsonSerializerOptions JsonOpts =
            new() { WriteIndented = true };

        private readonly string _prefsPath;

        public PreferencesService() : this(DefaultPrefsPath) { }

        public PreferencesService(string prefsPath)
        {
            _prefsPath = prefsPath;
        }

        public OutlinerPreferences Load()
        {
            if (!File.Exists(_prefsPath))
                return new OutlinerPreferences();

            try
            {
                var json = File.ReadAllText(_prefsPath);
                return JsonSerializer.Deserialize<OutlinerPreferences>(json)
                       ?? new OutlinerPreferences();
            }
            catch
            {
                return new OutlinerPreferences();
            }
        }

        public void Save(OutlinerPreferences prefs)
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
}
