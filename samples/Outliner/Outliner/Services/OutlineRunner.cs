using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using StoryCADLib.Services.API;

namespace Outliner.Services
{
    /// <summary>
    /// Result of a single end-to-end pipeline run.
    /// </summary>
    public sealed class OutlineRunResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string OutputPath { get; set; } = string.Empty;
        public OnePassResponse? Response { get; set; }
        public OutlineCost? Cost { get; set; }
        public OutlineRating? Rating { get; set; }
    }

    /// <summary>
    /// Encapsulates the prose -> LLM -> outline pipeline plus the per-run
    /// artifact writes (.stbx, .raw.json, .costs.json, .rating.json) so the
    /// single-file UI mode, batch UI mode, and the test harness all share
    /// identical behaviour.
    /// </summary>
    public sealed class OutlineRunner
    {
        private static readonly JsonSerializerOptions JsonOpts =
            new() { WriteIndented = true };

        private readonly ProseDocumentReader _reader;
        private readonly ProseAnalyzer _analyzer;
        private readonly OutlineBuilder _builder;

        public OutlineRunner()
        {
            var kernel      = Ioc.Default.GetRequiredService<Kernel>();
            var chatService = Ioc.Default.GetRequiredService<IChatCompletionService>();
            var api         = Ioc.Default.GetRequiredService<StoryCADApi>();

            _reader   = new ProseDocumentReader();
            _analyzer = new ProseAnalyzer(kernel, chatService);
            _builder  = new OutlineBuilder(api);
        }

        public OutlineRunner(ProseDocumentReader reader, ProseAnalyzer analyzer, OutlineBuilder builder)
        {
            _reader   = reader   ?? throw new ArgumentNullException(nameof(reader));
            _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
            _builder  = builder  ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// Read prose from the given path, analyze it, build a .stbx at
        /// outputPath, and write all four per-run artifacts beside it.
        /// </summary>
        public async Task<OutlineRunResult> RunAsync(
            string inputPath,
            string outputPath,
            IProgress<string>? progress = null)
        {
            try
            {
                progress?.Report($"Reading {Path.GetFileName(inputPath)}...");
                var prose = await _reader.ReadAsync(inputPath);
                return await RunFromTextAsync(prose, Path.GetFileName(inputPath), outputPath, progress);
            }
            catch (Exception ex)
            {
                return new OutlineRunResult
                {
                    IsSuccess = false,
                    OutputPath = outputPath,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Same pipeline starting from already-loaded prose text — used by the
        /// single-file UI flow that lets the user edit the prose before submit.
        /// </summary>
        public async Task<OutlineRunResult> RunFromTextAsync(
            string proseText,
            string inputFileName,
            string outputPath,
            IProgress<string>? progress = null)
        {
            var result = new OutlineRunResult { OutputPath = outputPath };

            try
            {
                progress?.Report("Analyzing with LLM...");
                result.Response = await _analyzer.AnalyzeProse(proseText);
                result.Cost = _analyzer.LastCost;

                var basePath = Path.Combine(
                    Path.GetDirectoryName(outputPath) ?? string.Empty,
                    Path.GetFileNameWithoutExtension(outputPath));

                if (!string.IsNullOrWhiteSpace(_analyzer.LastRawResponse))
                    await File.WriteAllTextAsync(basePath + ".raw.json", _analyzer.LastRawResponse);

                if (result.Cost != null)
                    await File.WriteAllTextAsync(
                        basePath + ".costs.json",
                        JsonSerializer.Serialize(result.Cost, JsonOpts));

                progress?.Report("Building outline...");
                var built = await _builder.BuildOutlineFromResponse(result.Response, outputPath);

                result.Rating = OutlineRating.ComputeAuto(
                    result.Response,
                    inputFileName,
                    result.Cost?.ModelId);
                await File.WriteAllTextAsync(
                    basePath + ".rating.json",
                    JsonSerializer.Serialize(result.Rating, JsonOpts));

                result.IsSuccess = built;
                if (!built)
                    result.ErrorMessage = "OutlineBuilder reported failure (partial outline may have been saved).";
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }
    }
}
