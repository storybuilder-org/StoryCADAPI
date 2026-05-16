using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADLib.Services.IoC;
using System.IO;

namespace OutlinerTests
{
    /// <summary>
    /// Global test setup and cleanup for all tests in the assembly.
    /// </summary>
    [TestClass]
    public static class TestSetup
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                var modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o";
                var kernel = Kernel.CreateBuilder()
                    .AddOpenAIChatCompletion(modelId, apiKey)
                    .Build();

                BootStrapper.Services.AddSingleton(kernel);
                BootStrapper.Services.AddSingleton(kernel.GetRequiredService<IChatCompletionService>());
            }

            BootStrapper.Initialise(headless: true);

            Directory.CreateDirectory(App.InputDir);
            Directory.CreateDirectory(App.OutputDir);

            foreach (var stale in Directory.GetFiles(App.OutputDir, "*.stbx"))
                File.Delete(stale);
            foreach (var stale in Directory.GetFiles(App.OutputDir, "*.raw.json"))
                File.Delete(stale);
            foreach (var stale in Directory.GetFiles(App.OutputDir, "*.costs.json"))
                File.Delete(stale);
            foreach (var stale in Directory.GetFiles(App.OutputDir, "*.rating.json"))
                File.Delete(stale);
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            // TestOutputs are intentionally left on disk so a human can open the
            // generated .stbx files in StoryCAD and judge the outline quality.
        }
    }
}
