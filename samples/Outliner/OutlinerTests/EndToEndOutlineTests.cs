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
    /// <summary>
    /// Live-LLM smoke tests. Excluded from default CI via the LiveLLM
    /// category; run locally or nightly with --filter "TestCategory=LiveLLM".
    /// Costs money per run and produces non-deterministic output. The
    /// deterministic structural assertions live in StubbedPipelineTests.
    /// </summary>
    [TestClass]
    [TestCategory("LiveLLM")]
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

            var runner = new OutlineRunner();

            var outputPath = Path.Combine(
                App.OutputDir,
                Path.GetFileNameWithoutExtension(inputPath) + ".stbx");

            var result = await runner.RunAsync(inputPath, outputPath);

            Assert.IsNotNull(result.Response, "Pipeline returned null response.");
            Assert.IsTrue(result.IsSuccess,
                result.ErrorMessage ?? "Pipeline reported failure.");
            Assert.IsTrue(File.Exists(outputPath),
                $"Expected outline at {outputPath} was not produced.");
            Assert.IsTrue(new FileInfo(outputPath).Length > 0,
                $"Outline at {outputPath} was empty.");
        }
    }
}
