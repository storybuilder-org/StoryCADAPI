using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Outliner.Services;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Outliner
{
    /// <summary>
    /// ViewModel for the Settings page. Persists changes to OutlinerPreferences
    /// immediately as toggles flip or folders are picked.
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

            _defaultInputFolder  = _prefs.DefaultInputFolder;
            _defaultOutputFolder = _prefs.DefaultOutputFolder;

            PickInputFolderCommand  = new AsyncRelayCommand(() => PickFolderAsync(p => DefaultInputFolder = p));
            PickOutputFolderCommand = new AsyncRelayCommand(() => PickFolderAsync(p => DefaultOutputFolder = p));
        }

        public IAsyncRelayCommand PickInputFolderCommand  { get; }
        public IAsyncRelayCommand PickOutputFolderCommand { get; }

        public IntPtr WindowHandle => App.MWindowHandle;

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

        private string _defaultInputFolder = string.Empty;
        public string DefaultInputFolder
        {
            get => _defaultInputFolder;
            set
            {
                if (SetProperty(ref _defaultInputFolder, value))
                {
                    _prefs.DefaultInputFolder = value;
                    _prefsService.Save(_prefs);
                }
            }
        }

        private string _defaultOutputFolder = string.Empty;
        public string DefaultOutputFolder
        {
            get => _defaultOutputFolder;
            set
            {
                if (SetProperty(ref _defaultOutputFolder, value))
                {
                    _prefs.DefaultOutputFolder = value;
                    _prefsService.Save(_prefs);
                }
            }
        }

        private async Task PickFolderAsync(Action<string> assign)
        {
            var picker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WindowHandle);
            picker.FileTypeFilter.Add("*");

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
                assign(folder.Path);
        }
    }
}
