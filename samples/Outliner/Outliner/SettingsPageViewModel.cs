using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Outliner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private readonly ModelCatalogService? _catalog;

        public SettingsPageViewModel()
            : this(
                Ioc.Default.GetRequiredService<OutlinerPreferences>(),
                Ioc.Default.GetRequiredService<PreferencesService>(),
                Ioc.Default.GetService<ModelCatalogService>())
        { }

        public SettingsPageViewModel(
            OutlinerPreferences prefs,
            PreferencesService prefsService,
            ModelCatalogService? catalog = null)
        {
            _prefs = prefs;
            _prefsService = prefsService;
            _catalog = catalog;

            _isStartupSingle = _prefs.StartupMode == "Single";
            _isStartupBatch  = _prefs.StartupMode == "Batch";

            _defaultInputFolder  = _prefs.DefaultInputFolder;
            _defaultOutputFolder = _prefs.DefaultOutputFolder;

            // Seed with the priced models so the picker is usable instantly;
            // LoadModelsAsync() will replace this with the live /v1/models list.
            // Same blacklist filter as the live list to keep the seed consistent.
            ModelOptions = new ObservableCollection<string>(
                ModelPriceTable.KnownModels.Where(id => !ModelCatalogService.IsBlacklisted(id)));
            // Surface the persisted value even if it isn't in the seed list,
            // so the picker reflects what the kernel will actually use.
            if (!string.IsNullOrWhiteSpace(_prefs.SelectedModelId)
                && !ModelOptions.Contains(_prefs.SelectedModelId))
            {
                ModelOptions.Insert(0, _prefs.SelectedModelId);
            }
            _selectedModelId = string.IsNullOrWhiteSpace(_prefs.SelectedModelId)
                ? null
                : _prefs.SelectedModelId;

            PickInputFolderCommand  = new AsyncRelayCommand(() => PickFolderAsync(p => DefaultInputFolder = p));
            PickOutputFolderCommand = new AsyncRelayCommand(() => PickFolderAsync(p => DefaultOutputFolder = p));
        }

        public ObservableCollection<string> ModelOptions { get; }

        private string _modelLoadStatus = string.Empty;
        public string ModelLoadStatus
        {
            get => _modelLoadStatus;
            set => SetProperty(ref _modelLoadStatus, value);
        }

        /// <summary>
        /// Refreshes ModelOptions from the OpenAI /v1/models endpoint. Preserves
        /// the current SelectedModelId even if the live list doesn't include it.
        /// Silent no-op when no catalog service is wired (e.g. in unit tests).
        /// </summary>
        public async Task LoadModelsAsync()
        {
            if (_catalog == null) return;

            ModelLoadStatus = "Loading models...";
            var live = await _catalog.GetChatModelsAsync();
            if (live.Count == 0)
            {
                ModelLoadStatus = "Couldn't fetch model list — showing built-in defaults.";
                return;
            }

            var preserved = _selectedModelId;
            ModelOptions.Clear();
            if (!string.IsNullOrWhiteSpace(preserved) && !live.Contains(preserved))
                ModelOptions.Add(preserved);
            foreach (var id in live) ModelOptions.Add(id);

            // ComboBox SelectedItem clears when its source is repopulated; restore.
            if (!string.IsNullOrWhiteSpace(preserved))
                SelectedModelId = preserved;

            ModelLoadStatus = $"Loaded {live.Count} models.";
        }

        private string? _selectedModelId;
        public string? SelectedModelId
        {
            get => _selectedModelId;
            set
            {
                if (SetProperty(ref _selectedModelId, value))
                {
                    _prefs.SelectedModelId = value ?? string.Empty;
                    _prefsService.Save(_prefs);
                }
            }
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
