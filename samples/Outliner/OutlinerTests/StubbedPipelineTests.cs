using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Outliner.Services;
using StoryCADLib.Services.API;

namespace OutlinerTests
{
    /// <summary>
    /// Pipeline tests against a recorded LLM response. Runs in default CI
    /// — deterministic, no API key required, no money spent, no LLM
    /// variance. Catches breakage in deserialization, GUID validation,
    /// and OutlineBuilder field mapping.
    /// </summary>
    [TestClass]
    public class StubbedPipelineTests
    {
        private static OutlineRunner BuildStubbedRunner(string cannedJson)
        {
            var kernel = Kernel.CreateBuilder().Build();
            var fake = new FakeChatCompletionService(cannedJson);
            var analyzer = new ProseAnalyzer(kernel, fake);
            var builder = new OutlineBuilder(Ioc.Default.GetRequiredService<StoryCADApi>());
            var reader = new ProseDocumentReader();
            return new OutlineRunner(reader, analyzer, builder);
        }

        [TestMethod]
        public async Task StubbedPipeline_MirrorMirror_ProducesExpectedElements()
        {
            var fixturePath = Path.Combine(App.FixturesDir, "Mirror, Mirror.raw.json");
            Assert.IsTrue(File.Exists(fixturePath),
                $"Missing fixture: {fixturePath}");

            var canned = await File.ReadAllTextAsync(fixturePath);
            var runner = BuildStubbedRunner(canned);

            var outputPath = Path.Combine(App.OutputDir, "stub_mirror.stbx");
            var result = await runner.RunFromTextAsync(
                "stub prose — content ignored when LLM is stubbed",
                "Mirror, Mirror.docx",
                outputPath);

            Assert.IsTrue(result.IsSuccess, result.ErrorMessage);
            Assert.IsNotNull(result.Response);
            Assert.AreEqual(2, result.Response.Characters?.Count, "Expected 2 characters (Jaime + Possibility).");
            Assert.AreEqual(1, result.Response.Settings?.Count,   "Expected 1 setting.");
            Assert.AreEqual(1, result.Response.Scenes?.Count,     "Expected 1 scene.");
            Assert.AreEqual(1, result.Response.Problems?.Count,   "Expected 1 problem.");
        }

        [TestMethod]
        public async Task StubbedPipeline_MirrorMirror_OverviewFieldsPopulated()
        {
            var fixturePath = Path.Combine(App.FixturesDir, "Mirror, Mirror.raw.json");
            var canned = await File.ReadAllTextAsync(fixturePath);
            var runner = BuildStubbedRunner(canned);

            var outputPath = Path.Combine(App.OutputDir, "stub_overview.stbx");
            var result = await runner.RunFromTextAsync(
                "stub prose",
                "Mirror, Mirror.docx",
                outputPath);

            Assert.IsTrue(result.IsSuccess);
            var overview = result.Response!.StoryOverview!;
            Assert.IsFalse(string.IsNullOrWhiteSpace(overview.Title));
            Assert.IsFalse(string.IsNullOrWhiteSpace(overview.Author));
            Assert.IsFalse(string.IsNullOrWhiteSpace(overview.Premise));
            Assert.AreEqual("Short-Short",     overview.StoryType);
            Assert.AreEqual("Science Fiction", overview.StoryGenre);
            Assert.IsFalse(string.IsNullOrWhiteSpace(overview.StoryProblem),
                "StoryProblem GUID expected on overview.");
        }

        [TestMethod]
        public async Task StubbedPipeline_MirrorMirror_PersonVsSelfAntagonistEqualsProtagonist()
        {
            var fixturePath = Path.Combine(App.FixturesDir, "Mirror, Mirror.raw.json");
            var canned = await File.ReadAllTextAsync(fixturePath);
            var runner = BuildStubbedRunner(canned);

            var outputPath = Path.Combine(App.OutputDir, "stub_pvs.stbx");
            var result = await runner.RunFromTextAsync(
                "stub prose",
                "Mirror, Mirror.docx",
                outputPath);

            Assert.IsTrue(result.IsSuccess);
            var problem = result.Response!.Problems![0];
            Assert.AreEqual("Person vs. Self", problem.ConflictType);
            Assert.AreEqual(problem.Protagonist, problem.Antagonist,
                "Person vs. Self requires Antagonist GUID == Protagonist GUID.");
        }

        [TestMethod]
        public async Task StubbedPipeline_MirrorMirror_RatingArtifactWritten()
        {
            var fixturePath = Path.Combine(App.FixturesDir, "Mirror, Mirror.raw.json");
            var canned = await File.ReadAllTextAsync(fixturePath);
            var runner = BuildStubbedRunner(canned);

            var outputPath = Path.Combine(App.OutputDir, "stub_rating.stbx");
            var ratingPath = Path.Combine(App.OutputDir, "stub_rating.rating.json");

            await runner.RunFromTextAsync(
                "stub prose",
                "Mirror, Mirror.docx",
                outputPath);

            Assert.IsTrue(File.Exists(ratingPath), $"Rating artifact missing: {ratingPath}");
            Assert.IsTrue(new FileInfo(ratingPath).Length > 0, "Rating artifact was empty.");
        }
    }
}
