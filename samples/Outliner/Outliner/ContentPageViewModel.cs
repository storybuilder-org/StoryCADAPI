using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.UI.Dispatching;
using System;
using System.IO;
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
        // Services
        private readonly ProseAnalyzer _proseAnalyzer;
        private readonly OutlineBuilder _outlineBuilder;
        private readonly ProseDocumentReader _proseReader;
        private readonly DispatcherQueue _dispatcher;

        // File management
        private StorageFile? _storyFile;
        private StorageFile? _outputFile;

        // Commands
        public IAsyncRelayCommand ReadStoryCommand { get; }
        public IAsyncRelayCommand SelectOutputFileCommand { get; }
        public IAsyncRelayCommand OutputFileCommand { get; }  // Alias for SelectOutputFileCommand
        public IAsyncRelayCommand CreateOutlineCommand { get; }

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
            set => SetProperty(ref _storyText, value);
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

        #endregion

        public ContentPageViewModel()
        {
            _dispatcher = DispatcherQueue.GetForCurrentThread();

            // Initialize services from DI container
            var kernel = Ioc.Default.GetRequiredService<Kernel>();
            var chatService = Ioc.Default.GetRequiredService<IChatCompletionService>();
            var api = Ioc.Default.GetRequiredService<StoryCADApi>();

            _proseAnalyzer = new ProseAnalyzer(kernel, chatService);
            _outlineBuilder = new OutlineBuilder(api);
            _proseReader = new ProseDocumentReader();

            // Initialize commands
            ReadStoryCommand = new AsyncRelayCommand(ReadStoryFileAsync);
            SelectOutputFileCommand = new AsyncRelayCommand(SelectOutputFileAsync);
            OutputFileCommand = SelectOutputFileCommand;  // Alias for XAML binding
            CreateOutlineCommand = new AsyncRelayCommand(
                CreateOutlineAsync,
                CanCreateOutline);
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
                ProgressValue = 0;

                // Phase 1: Analyze prose
                ProgressStatus = "Phase 1: Analyzing story with AI...";
                ProgressValue = 20;

                var analysisResult = await _proseAnalyzer.AnalyzeProse(StoryText);

                if (analysisResult == null)
                {
                    ContentText = "Failed to analyze story. Please try again.";
                    return;
                }

                ProgressValue = 50;
                var analysisText = $"Found: {analysisResult.Characters?.Count ?? 0} characters, " +
                                   $"{analysisResult.Settings?.Count ?? 0} settings, " +
                                   $"{analysisResult.Scenes?.Count ?? 0} scenes, " +
                                   $"{analysisResult.Problems?.Count ?? 0} problems";
                AnalysisResultText = analysisText;
                ChatHistoryText = $"Analysis complete:\n{analysisText}";

                // Phase 2: Build outline
                ProgressStatus = "Phase 2: Building StoryCAD outline...";
                ProgressValue = 70;

                var success = await _outlineBuilder.BuildOutlineFromResponse(
                    analysisResult,
                    _outputFile.Path);

                ProgressValue = 100;

                if (success)
                {
                    ContentText = $"✓ Outline successfully created and saved to:\n{_outputFile.Path}";
                    ProgressStatus = "Complete!";
                }
                else
                {
                    ContentText = "⚠ Outline was partially created. Some elements may be missing.";
                    ProgressStatus = "Completed with warnings";
                }
            }
            catch (InvalidOperationException ex)
            {
                ContentText = $"Validation error: {ex.Message}";
                ProgressStatus = "Failed";
            }
            catch (Exception ex)
            {
                ContentText = $"Error creating outline: {ex.Message}";
                ProgressStatus = "Failed";
            }
            finally
            {
                IsProcessing = false;
                CreateOutlineCommand.NotifyCanExecuteChanged();
            }
        }

    }
}