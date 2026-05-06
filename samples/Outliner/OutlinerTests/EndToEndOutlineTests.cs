using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Outliner.Services;
using StoryCADLib.Services.API;

namespace OutlinerTests
{
    [TestClass]
    public class EndToEndOutlineTests
    {
        public static IEnumerable<object[]> ProseInputs()
        {
            if (!Directory.Exists(App.InputDir))
                yield break;

            foreach (var path in Directory.GetFiles(App.InputDir))
            {
                var ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext == ".docx" || ext == ".pdf" || ext == ".txt")
                    yield return new object[] { path };
            }
        }

        public static string DisplayName(MethodInfo _, object[] data)
            => $"Pipeline: {Path.GetFileName((string)data[0])}";

        [TestMethod]
        [DynamicData(nameof(ProseInputs), DynamicDataDisplayName = nameof(DisplayName))]
        public async Task Pipeline_ProduceOutlineFromProse(string inputPath)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                Assert.Inconclusive("OPENAI_API_KEY not set; skipping live-LLM end-to-end test.");

            var kernel = Ioc.Default.GetRequiredService<Kernel>();
            var chatService = Ioc.Default.GetRequiredService<IChatCompletionService>();
            var api = Ioc.Default.GetRequiredService<StoryCADApi>();

            var reader = new ProseDocumentReader();
            var analyzer = new ProseAnalyzer(kernel, chatService);
            var builder = new OutlineBuilder(api);

            var prose = await reader.ReadAsync(inputPath);
            Assert.IsFalse(string.IsNullOrWhiteSpace(prose),
                $"Prose extracted from {Path.GetFileName(inputPath)} was empty.");

            var response = await analyzer.AnalyzeProse(prose);

            if (!string.IsNullOrWhiteSpace(analyzer.LastRawResponse))
            {
                var rawPath = Path.Combine(
                    App.OutputDir,
                    Path.GetFileNameWithoutExtension(inputPath) + ".raw.json");
                await File.WriteAllTextAsync(rawPath, analyzer.LastRawResponse);
            }

            if (analyzer.LastCost != null)
            {
                var costPath = Path.Combine(
                    App.OutputDir,
                    Path.GetFileNameWithoutExtension(inputPath) + ".costs.json");
                var costJson = System.Text.Json.JsonSerializer.Serialize(
                    analyzer.LastCost,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(costPath, costJson);
            }

            var rating = OutlineRating.ComputeAuto(
                response,
                Path.GetFileName(inputPath),
                analyzer.LastCost?.ModelId);
            var ratingPath = Path.Combine(
                App.OutputDir,
                Path.GetFileNameWithoutExtension(inputPath) + ".rating.json");
            var ratingJson = System.Text.Json.JsonSerializer.Serialize(
                rating,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(ratingPath, ratingJson);

            Assert.IsNotNull(response, "ProseAnalyzer returned null.");

            var outputPath = Path.Combine(
                App.OutputDir,
                Path.GetFileNameWithoutExtension(inputPath) + ".stbx");

            var ok = await builder.BuildOutlineFromResponse(response, outputPath);

            Assert.IsTrue(ok, "OutlineBuilder reported failure.");
            Assert.IsTrue(File.Exists(outputPath),
                $"Expected outline at {outputPath} was not produced.");
            Assert.IsTrue(new FileInfo(outputPath).Length > 0,
                $"Outline at {outputPath} was empty.");
        }
    }
}
