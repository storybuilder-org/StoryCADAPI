using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Outliner.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Outliner
{
    /// <summary>
    /// ViewModel for the batch processing page. The user picks an input folder
    /// containing prose files; outputs (.stbx + per-run artifacts) land in a
    /// sibling Outlines/ subfolder. Errors per file don't abort the batch.
    /// </summary>
    public sealed class BatchPageViewModel : ObservableObject
    {
        private readonly OutlineRunner _runner;
        private readonly OutlinerPreferences _prefs;
        private readonly PreferencesService _prefsService;
        private readonly DispatcherQueue _dispatcher;

        public BatchPageViewModel()
        {
            _dispatcher = DispatcherQueue.GetForCurrentThread();
            _runner = new OutlineRunner();
            _prefs = Ioc.Default.GetRequiredService<OutlinerPreferences>();
            _prefsService = Ioc.Default.GetRequiredService<PreferencesService>();

            PickInputFolderCommand = new AsyncRelayCommand(PickInputFolderAsync);
            RunBatchCommand = new AsyncRelayCommand(RunBatchAsync, () => Items.Count > 0 && !IsRunning);

            // If we remember a folder from last session, populate immediately.
            if (!string.IsNullOrWhiteSpace(_prefs.LastBatchInputFolder)
                && Directory.Exists(_prefs.LastBatchInputFolder))
            {
                InputFolder = _prefs.LastBatchInputFolder;
                OutputFolder = Path.Combine(InputFolder, "Outlines");
                PopulateItems();
            }
        }

        public ObservableCollection<BatchItem> Items { get; } = new();

        public IAsyncRelayCommand PickInputFolderCommand { get; }
        public IAsyncRelayCommand RunBatchCommand { get; }

        public IntPtr WindowHandle => App.MWindowHandle;

        private string _inputFolder = string.Empty;
        public string InputFolder
        {
            get => _inputFolder;
            set => SetProperty(ref _inputFolder, value);
        }

        private string _outputFolder = string.Empty;
        public string OutputFolder
        {
            get => _outputFolder;
            set => SetProperty(ref _outputFolder, value);
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (SetProperty(ref _isRunning, value))
                    RunBatchCommand.NotifyCanExecuteChanged();
            }
        }

        private string _summary = string.Empty;
        public string Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
        }

        private async Task PickInputFolderAsync()
        {
            var picker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WindowHandle);
            picker.FileTypeFilter.Add("*");

            var folder = await picker.PickSingleFolderAsync();
            if (folder == null) return;

            InputFolder  = folder.Path;
            OutputFolder = Path.Combine(InputFolder, "Outlines");

            _prefs.LastBatchInputFolder = InputFolder;
            _prefsService.Save(_prefs);

            PopulateItems();
            Summary = string.Empty;
        }

        private void PopulateItems()
        {
            Items.Clear();
            if (!Directory.Exists(InputFolder)) return;

            foreach (var file in Directory.GetFiles(InputFolder).OrderBy(f => f))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext == ".docx" || ext == ".pdf" || ext == ".txt")
                {
                    Items.Add(new BatchItem
                    {
                        FileName = Path.GetFileName(file),
                        FullPath = file
                    });
                }
            }
            RunBatchCommand.NotifyCanExecuteChanged();
        }

        private async Task RunBatchAsync()
        {
            if (Items.Count == 0) return;

            IsRunning = true;
            Summary = string.Empty;

            try
            {
                Directory.CreateDirectory(OutputFolder);
            }
            catch (Exception ex)
            {
                Summary = $"Could not create output folder: {ex.Message}";
                IsRunning = false;
                return;
            }

            decimal totalCost = 0m;
            int succeeded = 0;
            int failed = 0;

            foreach (var item in Items)
            {
                _dispatcher.TryEnqueue(() => item.Status = "Processing");

                var outputPath = Path.Combine(
                    OutputFolder,
                    Path.GetFileNameWithoutExtension(item.FileName) + ".stbx");

                try
                {
                    var result = await _runner.RunAsync(item.FullPath, outputPath);
                    if (result.IsSuccess)
                    {
                        _dispatcher.TryEnqueue(() =>
                        {
                            item.Status = "Done";
                            if (result.Cost != null) item.Cost = result.Cost.TotalCostUsd;
                        });
                        succeeded++;
                        if (result.Cost != null) totalCost += result.Cost.TotalCostUsd;
                    }
                    else
                    {
                        _dispatcher.TryEnqueue(() =>
                        {
                            item.Status = "Failed";
                            item.ErrorMessage = result.ErrorMessage;
                        });
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    _dispatcher.TryEnqueue(() =>
                    {
                        item.Status = "Failed";
                        item.ErrorMessage = ex.Message;
                    });
                    failed++;
                }
            }

            Summary = $"Batch complete: {succeeded} succeeded, {failed} failed. Total cost: ${totalCost:F4}";
            IsRunning = false;
        }
    }
}
