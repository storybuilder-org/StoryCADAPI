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

        public string LastSingleInputFolder  { get; set; } = string.Empty;
        public string LastSingleOutputFolder { get; set; } = string.Empty;
        public string LastBatchInputFolder   { get; set; } = string.Empty;
    }

    /// <summary>
    /// Loads and saves OutlinerPreferences. Tolerant of missing or corrupted
    /// files — falls back to defaults rather than throwing during startup.
    /// </summary>
    public sealed class PreferencesService
    {
        private static readonly string PrefsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Outliner",
            "preferences.json");

        private static readonly JsonSerializerOptions JsonOpts =
            new() { WriteIndented = true };

        public OutlinerPreferences Load()
        {
            if (!File.Exists(PrefsPath))
                return new OutlinerPreferences();

            try
            {
                var json = File.ReadAllText(PrefsPath);
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
                Directory.CreateDirectory(Path.GetDirectoryName(PrefsPath)!);
                File.WriteAllText(PrefsPath, JsonSerializer.Serialize(prefs, JsonOpts));
            }
            catch
            {
                // Best-effort persistence; don't crash the app over this.
            }
        }
    }
}
