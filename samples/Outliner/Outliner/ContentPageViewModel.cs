using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using StoryCADLib.Services.API;
using Outliner.Services;

namespace Outliner
{
    /// <summary>
    /// ViewModel for the content page that orchestrates the two-phase outline creation process.
    /// Phase 1: Prose analysis using ProseAnalyzer service
    /// Phase 2: Outline construction using OutlineBuilder service
    /// </summary>
    public class ContentPageViewModel : ObservableObject
    {
        // Pipeline + a peek at the analyzer for prose-length validation in
        // the file-loading step. The runner orchestrates analyze + build +
        // artifact writes for both single and batch modes.
        private readonly OutlineRunner _runner;
        private readonly ProseAnalyzer _proseAnalyzer;
        private readonly ProseDocumentReader _proseReader;

        // File management
        private StorageFile? _storyFile;
        private StorageFile? _outputFile;

        // Commands
        public IAsyncRelayCommand ReadStoryCommand { get; }
        public IAsyncRelayCommand SelectOutputFileCommand { get; }
        public IAsyncRelayCommand OutputFileCommand { get; }  // Alias for SelectOutputFileCommand
        public IAsyncRelayCommand CreateOutlineCommand { get; }
        public IRelayCommand ThumbsUpCommand { get; }
        public IRelayCommand ThumbsDownCommand { get; }
        public IRelayCommand OpenInStoryCadCommand { get; }

        // Feedback state — captured after a successful outline creation.
        // Public properties so VM tests can pre-populate them and exercise
        // SubmitFeedback without driving a full LLM round-trip.
        private OnePassResponse? _lastResponse;
        public OnePassResponse? LastResponse
        {
            get => _lastResponse;
            set => SetProperty(ref _lastResponse, value);
        }

        private OutlineCost? _lastCost;
        public OutlineCost? LastCost
        {
            get => _lastCost;
            set => SetProperty(ref _lastCost, value);
        }

        private string? _lastOutputPath;
        public string? LastOutputPath
        {
            get => _lastOutputPath;
            set
            {
                if (SetProperty(ref _lastOutputPath, value))
                    OpenInStoryCadCommand?.NotifyCanExecuteChanged();
            }
        }

        // Window handle for file picker initialization (read lazily from App)
        public IntPtr WindowHandle => App.MWindowHandle;

        #region Observable Properties

        private string _contentText = string.Empty;
        public string ContentText
        {
            get => _contentText;
            set => SetProperty(ref _contentText, value);
        }

        private string _storyText = string.Empty;
        public string StoryText
        {
            get => _storyText;
            set
            {
                if (SetProperty(ref _storyText, value))
                    RefreshTokenInfoEstimate();
            }
        }

        private string _tokenInfo = string.Empty;
        public string TokenInfo
        {
            get => _tokenInfo;
            set => SetProperty(ref _tokenInfo, value);
        }

        private void RefreshTokenInfoEstimate()
        {
            // Only refresh the estimate label while the user is still editing
            // prose. After a run, TokenInfo carries the actual usage and we
            // leave it alone until the next StoryText edit triggers a recompute.
            if (string.IsNullOrEmpty(_storyText))
            {
                TokenInfo = string.Empty;
                return;
            }
            var estimated = _proseAnalyzer?.EstimateTokenCount(_storyText) ?? (_storyText.Length / 4);
            TokenInfo = $"{estimated:N0} prompt tokens (system prompt not included)";
        }

        private string _analysisResultText = string.Empty;
        public string AnalysisResultText
        {
            get => _analysisResultText;
            set => SetProperty(ref _analysisResultText, value);
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        private string _progressStatus = "Ready";
        public string ProgressStatus
        {
            get => _progressStatus;
            set => SetProperty(ref _progressStatus, value);
        }

        private string _chatHistoryText = string.Empty;
        public string ChatHistoryText
        {
            get => _chatHistoryText;
            set => SetProperty(ref _chatHistoryText, value);
        }

        private bool _showFeedbackPanel;
        public bool ShowFeedbackPanel
        {
            get => _showFeedbackPanel;
            set
            {
                if (SetProperty(ref _showFeedbackPanel, value))
                    OnPropertyChanged(nameof(FeedbackVisibility));
            }
        }

        public Visibility FeedbackVisibility =>
            _showFeedbackPanel ? Visibility.Visible : Visibility.Collapsed;

        private string _userFeedback = string.Empty;
        public string UserFeedback
        {
            get => _userFeedback;
            set => SetProperty(ref _userFeedback, value);
        }

        private string _feedbackStatus = string.Empty;
        public string FeedbackStatus
        {
            get => _feedbackStatus;
            set => SetProperty(ref _feedbackStatus, value);
        }

        #endregion

        public ContentPageViewModel()
        {
            // Build pipeline components and assemble the shared runner. The
            // analyzer reference is kept so we can validate prose length and
            // surface LastCost on the feedback panel.
            var kernel = Ioc.Default.GetRequiredService<Kernel>();
            var chatService = Ioc.Default.GetRequiredService<IChatCompletionService>();
            var api = Ioc.Default.GetRequiredService<StoryCADApi>();

            _proseAnalyzer = new ProseAnalyzer(kernel, chatService);
            _proseReader   = new ProseDocumentReader();
            var builder    = new OutlineBuilder(api);
            _runner        = new OutlineRunner(_proseReader, _proseAnalyzer, builder);

            // Initialize commands
            ReadStoryCommand = new AsyncRelayCommand(ReadStoryFileAsync);
            SelectOutputFileCommand = new AsyncRelayCommand(SelectOutputFileAsync);
            OutputFileCommand = SelectOutputFileCommand;  // Alias for XAML binding
            CreateOutlineCommand = new AsyncRelayCommand(
                CreateOutlineAsync,
                CanCreateOutline);
            ThumbsUpCommand = new RelayCommand(() => SubmitFeedback("thumbs_up"));
            ThumbsDownCommand = new RelayCommand(() => SubmitFeedback("thumbs_down"));
            OpenInStoryCadCommand = new RelayCommand(OpenInStoryCad, CanOpenInStoryCad);
        }

        private bool CanOpenInStoryCad() =>
            !string.IsNullOrEmpty(LastOutputPath) && File.Exists(LastOutputPath);

        /// <summary>
        /// Launches the generated .stbx via the OS shell association, which is
        /// what opens it in StoryCAD on a machine where StoryCAD is installed.
        /// </summary>
        private void OpenInStoryCad()
        {
            if (!CanOpenInStoryCad()) return;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = LastOutputPath!,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ContentText = $"Couldn't open in StoryCAD: {ex.Message}";
            }
        }

        /// <summary>
        /// Writes an OutlineRating beside the .stbx capturing the user's
        /// thumbs verdict and any free-text feedback. Heuristic auto-rating
        /// is also recorded so we have signal even when feedback is sparse.
        /// </summary>
        public void SubmitFeedback(string userRating)
        {
            if (LastResponse == null || string.IsNullOrEmpty(LastOutputPath))
                return;

            var rating = OutlineRating.ComputeAuto(
                LastResponse,
                _storyFile?.Name,
                LastCost?.ModelId);
            rating.UserRating = userRating;
            rating.UserFeedback = string.IsNullOrWhiteSpace(UserFeedback) ? null : UserFeedback;

            try
            {
                var ratingPath = Path.ChangeExtension(LastOutputPath, ".rating.json");
                var json = JsonSerializer.Serialize(
                    rating,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ratingPath, json);
                FeedbackStatus = $"Thanks — feedback saved to {Path.GetFileName(ratingPath)}.";
            }
            catch (Exception ex)
            {
                FeedbackStatus = $"Couldn't save feedback: {ex.Message}";
            }

            ShowFeedbackPanel = false;
        }

        /// <summary>
        /// Determines if the outline can be created.
        /// </summary>
        private bool CanCreateOutline()
        {
            return !string.IsNullOrWhiteSpace(StoryText) &&
                   !IsProcessing &&
                   _outputFile != null;
        }

        /// <summary>
        /// Opens a file picker and reads the selected story file.
        /// Supports .txt, .docx, and .pdf formats.
        /// </summary>
        private async Task ReadStoryFileAsync()
        {
            var picker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WindowHandle);

            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".docx");
            picker.FileTypeFilter.Add(".pdf");

            _storyFile = await picker.PickSingleFileAsync();
            if (_storyFile == null)
            {
                ContentText = "No file selected.";
                return;
            }

            try
            {
                ProgressStatus = $"Reading {_storyFile.Name}...";
                IsProcessing = true;

                StoryText = await _proseReader.ReadAsync(_storyFile.Path);

                if (!string.IsNullOrWhiteSpace(StoryText))
                {
                    ContentText = $"Loaded: {_storyFile.Name} ({StoryText.Length} characters)";

                    // Validate length
                    if (!_proseAnalyzer.ValidateProseLength(StoryText))
                    {
                        var tokenCount = _proseAnalyzer.EstimateTokenCount(StoryText);
                        ContentText += $"\nWarning: Story may be too long ({tokenCount} estimated tokens)";
                    }
                }
                else
                {
                    ContentText = "Failed to read file content.";
                }
            }
            catch (Exception ex)
            {
                ContentText = $"Error reading file: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
                ProgressStatus = "Ready";
                CreateOutlineCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Lets the user select where to save the output .stbx file.
        /// </summary>
        private async Task SelectOutputFileAsync()
        {
            var picker = new FileSavePicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WindowHandle);

            picker.FileTypeChoices.Add("Story Outline", new[] { ".stbx" });
            picker.SuggestedFileName = _storyFile != null
                ? Path.GetFileNameWithoutExtension(_storyFile.Name) + "_outline.stbx"
                : "Outline.stbx";

            _outputFile = await picker.PickSaveFileAsync();
            if (_outputFile != null)
            {
                ContentText = $"Output will be saved to: {_outputFile.Path}";
                CreateOutlineCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Main orchestration method that runs the two-phase outline creation process.
        /// Phase 1: Analyze prose with LLM
        /// Phase 2: Build outline with StoryCAD API
        /// </summary>
        private async Task CreateOutlineAsync()
        {
            if (!CanCreateOutline())
                return;

            try
            {
                IsProcessing = true;
                ProgressValue = 20;

                var progress = new Progress<string>(msg => ProgressStatus = msg);

                var inputName = _storyFile?.Name ?? "loaded prose";
                var result = await _runner.RunFromTextAsync(
                    StoryText,
                    inputName,
                    _outputFile.Path,
                    progress);

                ProgressValue = 100;

                if (result.IsSuccess && result.Response != null)
                {
                    AnalysisResultText =
                        $"Found: {result.Response.Characters?.Count ?? 0} characters, " +
                        $"{result.Response.Settings?.Count ?? 0} settings, " +
                        $"{result.Response.Scenes?.Count ?? 0} scenes, " +
                        $"{result.Response.Problems?.Count ?? 0} problems";
                    ChatHistoryText = $"Analysis complete:\n{AnalysisResultText}";
                    ContentText = $"✓ Outline successfully created and saved to:\n{_outputFile.Path}";
                    ProgressStatus = "Complete!";

                    LastResponse = result.Response;
                    LastCost = result.Cost;
                    LastOutputPath = _outputFile.Path;
                    UserFeedback = string.Empty;
                    FeedbackStatus = string.Empty;
                    ShowFeedbackPanel = true;

                    if (result.Cost != null)
                    {
                        TokenInfo =
                            $"{result.Cost.InputTokens:N0} in / {result.Cost.OutputTokens:N0} out tokens · " +
                            $"${result.Cost.TotalCostUsd:F4} ({result.Cost.ModelId})";
                    }
                }
                else
                {
                    ContentText = result.ErrorMessage ?? "Outline could not be created.";
                    ProgressStatus = "Failed";
                }
            }
            finally
            {
                IsProcessing = false;
                CreateOutlineCommand.NotifyCanExecuteChanged();
            }
        }

    }
}