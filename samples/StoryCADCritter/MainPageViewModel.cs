using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.UI.Xaml;
using Windows.Storage;
using Windows.Storage.Pickers;
using StoryCADLib.Services.API;

namespace StoryCADCritter;

public class MainPageViewModel : ObservableObject
{
    private readonly StoryCADApi _api;
    private readonly CritterPreferences _prefs;
    private readonly PreferencesService _prefsService;
    private StorageFile? _outlineFile;
    private StorageFolder? _outputFolder;
    private CancellationTokenSource? _runCts;

    public IAsyncRelayCommand PickOutlineCommand { get; }
    public IAsyncRelayCommand PickOutputFolderCommand { get; }
    public IAsyncRelayCommand RunCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand OpenReportCommand { get; }
    public IRelayCommand OpenOutputFolderCommand { get; }

    public MainPageViewModel()
    {
        _api = Ioc.Default.GetRequiredService<StoryCADApi>();
        _prefs = Ioc.Default.GetRequiredService<CritterPreferences>();
        _prefsService = Ioc.Default.GetRequiredService<PreferencesService>();

        _maxConcurrency = _prefs.MaxConcurrency;
        _keyQuestionsSeparate = string.Equals(_prefs.KeyQuestionsPlacement, "Separate", StringComparison.OrdinalIgnoreCase);
        _selectedModelId = string.IsNullOrWhiteSpace(_prefs.SelectedModelId) ? null : _prefs.SelectedModelId;
        if (_selectedModelId != null && !ModelOptions.Contains(_selectedModelId))
            ModelOptions.Insert(0, _selectedModelId);

        PickOutlineCommand = new AsyncRelayCommand(PickOutlineAsync, () => !IsRunning);
        PickOutputFolderCommand = new AsyncRelayCommand(PickOutputFolderAsync, () => !IsRunning);
        RunCommand = new AsyncRelayCommand(RunAsync, CanRun);
        CancelCommand = new RelayCommand(Cancel, () => IsRunning && _runCts is { IsCancellationRequested: false });
        OpenReportCommand = new RelayCommand(OpenReport, () => !string.IsNullOrEmpty(ReportPath) && File.Exists(ReportPath));
        OpenOutputFolderCommand = new RelayCommand(OpenOutputFolder, () => _outputFolder != null);
    }

    // ---- Settings (persisted on change; applied at the next run) ----

    public ObservableCollection<string> ModelOptions { get; } =
        new() { "gpt-4o-mini", "gpt-4o", "gpt-4-turbo" };

    private int _maxConcurrency = 8;
    public int MaxConcurrency
    {
        get => _maxConcurrency;
        set
        {
            var clamped = Math.Clamp(value, 1, 16);
            if (SetProperty(ref _maxConcurrency, clamped))
            {
                _prefs.MaxConcurrency = clamped;
                _prefsService.Save(_prefs);
            }
        }
    }

    private string? _selectedModelId;
    /// <summary>Empty/null means fall back to OPENAI_MODEL, then gpt-4o-mini.</summary>
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

    private bool _keyQuestionsSeparate;
    /// <summary>True = consolidated Key Questions section; false = inline per element.</summary>
    public bool KeyQuestionsSeparate
    {
        get => _keyQuestionsSeparate;
        set
        {
            if (SetProperty(ref _keyQuestionsSeparate, value))
            {
                _prefs.KeyQuestionsPlacement = value ? "Separate" : "Inline";
                _prefsService.Save(_prefs);
            }
        }
    }

    private string _outlinePathDisplay = "(no outline selected)";
    public string OutlinePathDisplay
    {
        get => _outlinePathDisplay;
        private set => SetProperty(ref _outlinePathDisplay, value);
    }

    private string _outputFolderDisplay = "(no output folder selected)";
    public string OutputFolderDisplay
    {
        get => _outputFolderDisplay;
        private set => SetProperty(ref _outputFolderDisplay, value);
    }

    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    private readonly StringBuilder _log = new();
    private string _logText = string.Empty;
    public string LogText
    {
        get => _logText;
        private set => SetProperty(ref _logText, value);
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (SetProperty(ref _isRunning, value))
            {
                OnPropertyChanged(nameof(ProgressVisibility));
                OnPropertyChanged(nameof(CancelVisibility));
                PickOutlineCommand.NotifyCanExecuteChanged();
                PickOutputFolderCommand.NotifyCanExecuteChanged();
                RunCommand.NotifyCanExecuteChanged();
                CancelCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public Visibility CancelVisibility => IsRunning ? Visibility.Visible : Visibility.Collapsed;

    private int _progressDone;
    public int ProgressDone
    {
        get => _progressDone;
        private set
        {
            if (SetProperty(ref _progressDone, value))
                OnPropertyChanged(nameof(ProgressLabel));
        }
    }

    private int _progressTotal = 1;
    public int ProgressTotal
    {
        get => _progressTotal;
        private set
        {
            if (SetProperty(ref _progressTotal, value))
                OnPropertyChanged(nameof(ProgressLabel));
        }
    }

    public string ProgressLabel => $"{ProgressDone} / {ProgressTotal} elements";

    public Visibility ProgressVisibility => IsRunning ? Visibility.Visible : Visibility.Collapsed;

    private bool _hasResults;
    public bool HasResults
    {
        get => _hasResults;
        private set
        {
            if (SetProperty(ref _hasResults, value))
            {
                OnPropertyChanged(nameof(ResultsVisibility));
                OpenReportCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public Visibility ResultsVisibility => HasResults ? Visibility.Visible : Visibility.Collapsed;

    public string? ReportPath { get; private set; }

    private bool CanRun() =>
        !IsRunning && _outlineFile != null && _outputFolder != null;

    private async Task PickOutlineAsync()
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".stbx");
#if WINDOWS
        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.MainWindowHandle);
#endif
        var file = await picker.PickSingleFileAsync();
        if (file == null) return;
        _outlineFile = file;
        OutlinePathDisplay = file.Path;
        RunCommand.NotifyCanExecuteChanged();
    }

    private async Task PickOutputFolderAsync()
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
#if WINDOWS
        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.MainWindowHandle);
#endif
        var folder = await picker.PickSingleFolderAsync();
        if (folder == null) return;
        _outputFolder = folder;
        OutputFolderDisplay = folder.Path;
        RunCommand.NotifyCanExecuteChanged();
        OpenOutputFolderCommand.NotifyCanExecuteChanged();
    }

    private void Cancel()
    {
        _runCts?.Cancel();
        AppendLog("Cancel requested.");
        CancelCommand.NotifyCanExecuteChanged();
    }

    private async Task RunAsync()
    {
        if (_outlineFile == null || _outputFolder == null) return;

        HasResults = false;
        ReportPath = null;
        _log.Clear();
        LogText = string.Empty;
        ProgressDone = 0;
        ProgressTotal = 1;
        _runCts?.Dispose();
        _runCts = new CancellationTokenSource();

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            StatusText = "OPENAI_API_KEY is not set. Set the env var and restart.";
            AppendLog(StatusText);
            return;
        }
        var modelId = !string.IsNullOrWhiteSpace(_prefs.SelectedModelId)
            ? _prefs.SelectedModelId
            : Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";

        IsRunning = true;
        try
        {
            StatusText = $"Opening outline ({Path.GetFileName(_outlineFile.Path)})...";
            AppendLog(StatusText);
            var openResult = await _api.OpenOutline(_outlineFile.Path);
            if (openResult == null || !openResult.IsSuccess)
            {
                StatusText = $"Failed to open outline: {openResult?.ErrorMessage ?? "unknown error"}";
                AppendLog(StatusText);
                return;
            }

            string systemPrompt;
            try
            {
                systemPrompt = LoadPrompt();
            }
            catch (Exception ex)
            {
                StatusText = $"Couldn't load CritiquePrompt.md: {ex.Message}";
                AppendLog(StatusText);
                return;
            }

            Kernel kernel;
            IChatCompletionService chatService;
            try
            {
                kernel = Kernel.CreateBuilder()
                    .AddOpenAIChatCompletion(modelId, apiKey)
                    .Build();
                chatService = kernel.GetRequiredService<IChatCompletionService>();
            }
            catch (Exception ex)
            {
                StatusText = $"Couldn't initialise OpenAI client: {ex.Message}";
                AppendLog(StatusText);
                return;
            }

            var orchestrator = new CritiqueOrchestrator(_api, chatService, systemPrompt, kernel, _prefs.MaxConcurrency);
            var progress = new Progress<CritiqueProgress>(p =>
            {
                StatusText = p.Status;
                if (p.Total > 0)
                {
                    ProgressTotal = p.Total;
                    ProgressDone = p.Done;
                }
                AppendLog(p.Status);
            });

            AppendLog($"Model: {modelId}");
            AppendLog("Walking elements...");
            CritiqueRunResult run;
            try
            {
                run = await orchestrator.RunAsync(_outlineFile.Path, progress, _outputFolder.Path, _runCts.Token);
            }
            catch (OperationCanceledException)
            {
                StatusText = "Cancelled.";
                AppendLog(StatusText);
                return;
            }
            catch (Exception ex)
            {
                StatusText = $"Unhandled exception during critique: {ex.Message}";
                AppendLog(StatusText);
                return;
            }

            await WriteArtifactsAsync(run, Path.GetFileNameWithoutExtension(_outlineFile.Path));

            if (run.HardFailed)
                StatusText = $"Hard failure: {run.HardFailureMessage}";
            else if (run.ShortCircuited)
                StatusText = $"Short-circuited: {run.ShortCircuitReason}";
            else if (run.HasPerElementErrors)
                StatusText = "Done — completed with per-element failures. See the Errors section in the report.";
            else
                StatusText = $"Done. Report: {run.ReportPath}";

            AppendLog(StatusText);
            ReportPath = run.ReportPath;
            HasResults = File.Exists(run.ReportPath);
        }
        finally
        {
            IsRunning = false;
        }
    }

    private async Task WriteArtifactsAsync(CritiqueRunResult run, string outlineDisplay)
    {
        var separateKq = string.Equals(_prefs.KeyQuestionsPlacement, "Separate", StringComparison.OrdinalIgnoreCase);
        var report = CritiqueOrchestrator.RenderReport(run, outlineDisplay, separateKq);
        try
        {
            await File.WriteAllTextAsync(run.ReportPath, report);
            AppendLog($"Report written: {run.ReportPath}");
        }
        catch (Exception ex)
        {
            AppendLog($"WARNING: Couldn't write report: {ex.Message}");
        }

        var jsonOpts = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = null };

        try
        {
            await File.WriteAllTextAsync(run.CostsPath, JsonSerializer.Serialize(run.Cost, jsonOpts));
            AppendLog($"Costs written:  {run.CostsPath}");
        }
        catch (Exception ex)
        {
            AppendLog($"WARNING: Couldn't write costs: {ex.Message}");
        }

        try
        {
            var rawDoc = new
            {
                model = run.Cost.ModelId,
                structuralOrientation = run.StructuralOrientation,
                elements = run.ElementCritiques.ConvertAll(e => (object)new
                {
                    uuid = e.Uuid,
                    type = e.ElementType,
                    name = e.Name,
                    critiqueMode = e.CritiqueMode,
                    critiqueFocus = e.CritiqueFocus,
                    parsed = e.Parsed,
                    rawResponse = e.RawResponse,
                    error = e.ErrorMessage
                })
            };
            await File.WriteAllTextAsync(run.RawPath, JsonSerializer.Serialize(rawDoc, jsonOpts));
            AppendLog($"Raw written:    {run.RawPath}");
        }
        catch (Exception ex)
        {
            AppendLog($"WARNING: Couldn't write raw: {ex.Message}");
        }
    }

    private static string LoadPrompt()
    {
        string[] candidates =
        {
            Path.Combine(AppContext.BaseDirectory, "Prompts", "CritiquePrompt.md"),
            Path.Combine(Directory.GetCurrentDirectory(), "Prompts", "CritiquePrompt.md")
        };
        foreach (var p in candidates)
            if (File.Exists(p))
                return File.ReadAllText(p);
        throw new FileNotFoundException(
            "CritiquePrompt.md not found. Looked in: " + string.Join(" | ", candidates));
    }

    private void OpenReport()
    {
        if (string.IsNullOrEmpty(ReportPath) || !File.Exists(ReportPath)) return;
        try
        {
            Process.Start(new ProcessStartInfo { FileName = ReportPath, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            AppendLog($"Couldn't open report: {ex.Message}");
        }
    }

    private void OpenOutputFolder()
    {
        if (_outputFolder == null) return;
        try
        {
            Process.Start(new ProcessStartInfo { FileName = _outputFolder.Path, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            AppendLog($"Couldn't open folder: {ex.Message}");
        }
    }

    private void AppendLog(string line)
    {
        _log.AppendLine(line);
        LogText = _log.ToString();
    }
}
