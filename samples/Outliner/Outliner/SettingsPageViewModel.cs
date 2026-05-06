using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Outliner.Services;

namespace Outliner
{
    /// <summary>
    /// ViewModel for the Settings page. Saves changes to OutlinerPreferences
    /// immediately as toggles flip — no separate Apply button needed.
    /// </summary>
    public sealed class SettingsPageViewModel : ObservableObject
    {
        private readonly OutlinerPreferences _prefs;
        private readonly PreferencesService _prefsService;

        public SettingsPageViewModel()
            : this(
                Ioc.Default.GetRequiredService<OutlinerPreferences>(),
                Ioc.Default.GetRequiredService<PreferencesService>())
        { }

        public SettingsPageViewModel(OutlinerPreferences prefs, PreferencesService prefsService)
        {
            _prefs = prefs;
            _prefsService = prefsService;

            _isStartupSingle = _prefs.StartupMode == "Single";
            _isStartupBatch  = _prefs.StartupMode == "Batch";
        }

        private bool _isStartupSingle;
        public bool IsStartupSingle
        {
            get => _isStartupSingle;
            set
            {
                if (SetProperty(ref _isStartupSingle, value) && value)
                {
                    _prefs.StartupMode = "Single";
                    _prefsService.Save(_prefs);
                }
            }
        }

        private bool _isStartupBatch;
        public bool IsStartupBatch
        {
            get => _isStartupBatch;
            set
            {
                if (SetProperty(ref _isStartupBatch, value) && value)
                {
                    _prefs.StartupMode = "Batch";
                    _prefsService.Save(_prefs);
                }
            }
        }
    }
}
