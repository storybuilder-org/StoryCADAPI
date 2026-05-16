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
    /// Pipeline tests against a recorded LLM response. Runs in default CI —
    /// deterministic, no API key required, no money spent, no LLM variance.
    /// Catches breakage in deserialization, GUID validation, OutlineBuilder
    /// field mapping, and the prose-reader -> analyzer -> builder wiring.
    /// </summary>
    [TestClass]
    public class StubbedPipelineTests
    {
        private const string FixtureFile = "Mirror, Mirror.raw.json";

        private static OutlineRunner BuildStubbedRunner(string cannedJson)
        {
            var kernel = Kernel.CreateBuilder().Build();
            var fake = new FakeChatCompletionService(cannedJson);
            var analyzer = new ProseAnalyzer(kernel, fake);
            var builder = new OutlineBuilder(Ioc.Default.GetRequiredService<StoryCADApi>());
            var reader = new ProseDocumentReader();
            return new OutlineRunner(reader, analyzer, builder);
        }

        private static async Task<string> LoadFixtureAsync()
        {
            var fixturePath = Path.Combine(App.FixturesDir, FixtureFile);
            Assert.IsTrue(File.Exists(fixturePath), $"Missing fixture: {fixturePath}");
            return await File.ReadAllTextAsync(fixturePath);
        }

        // Writes a tiny synthetic prose .txt so the test exercises
        // ProseDocumentReader on the way in. Content is irrelevant because
        // the LLM is stubbed.
        private static async Task<string> WriteSyntheticProseAsync(string baseName)
        {
            var prosePath = Path.Combine(App.OutputDir, baseName + ".stub_prose.txt");
            await File.WriteAllTextAsync(
                prosePath,
                "Synthetic prose for stubbed pipeline test. LLM call is faked; content is ignored.");
            return prosePath;
        }

        [TestMethod]
        public async Task StubbedPipeline_MirrorMirror_ProducesExpectedElements()
        {
            var canned = await LoadFixtureAsync();
            var runner = BuildStubbedRunner(canned);

            var prosePath = await WriteSyntheticProseAsync("stub_mirror");
            var outputPath = Path.Combine(App.OutputDir, "stub_mirror.stbx");
            var result = await runner.RunAsync(prosePath, outputPath);

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
            var canned = await LoadFixtureAsync();
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
            var canned = await LoadFixtureAsync();
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
        public async Task StubbedPipeline_MirrorMirror_CharacterStoryRoleAndRolePopulated()
        {
            var canned = await LoadFixtureAsync();
            var runner = BuildStubbedRunner(canned);

            var outputPath = Path.Combine(App.OutputDir, "stub_roles.stbx");
            var result = await runner.RunFromTextAsync(
                "stub prose",
                "Mirror, Mirror.docx",
                outputPath);

            Assert.IsTrue(result.IsSuccess);
            var characters = result.Response!.Characters!;
            Assert.IsTrue(characters.Count >= 2);

            var jaime = characters[0];
            Assert.AreEqual("Student",     jaime.Role,      "Role is free-form (e.g. Student), not the narrative role.");
            Assert.AreEqual("Protagonist", jaime.StoryRole, "StoryRole is the fixed-list narrative role.");

            var possibility = characters[1];
            Assert.AreEqual("AI Mirror",        possibility.Role);
            Assert.AreEqual("Supporting Role",  possibility.StoryRole);
        }

        [TestMethod]
        public async Task StubbedPipeline_MirrorMirror_ScenePurposePopulated()
        {
            var canned = await LoadFixtureAsync();
            var runner = BuildStubbedRunner(canned);

            var outputPath = Path.Combine(App.OutputDir, "stub_purpose.stbx");
            var result = await runner.RunFromTextAsync(
                "stub prose",
                "Mirror, Mirror.docx",
                outputPath);

            Assert.IsTrue(result.IsSuccess);
            var scene = result.Response!.Scenes![0];
            Assert.IsNotNull(scene.ScenePurpose, "Scene.ScenePurpose deserialized as null.");
            Assert.IsTrue(scene.ScenePurpose!.Count >= 1, "Expected at least one scene purpose.");
            CollectionAssert.Contains(scene.ScenePurpose, "Introduce Situation");
            CollectionAssert.Contains(scene.ScenePurpose, "Develop Characters");
        }

        [TestMethod]
        public async Task StubbedPipeline_MirrorMirror_CharacterSketchDistinctFromAppearance()
        {
            var canned = await LoadFixtureAsync();
            var runner = BuildStubbedRunner(canned);

            var outputPath = Path.Combine(App.OutputDir, "stub_sketch.stbx");
            var result = await runner.RunFromTextAsync(
                "stub prose",
                "Mirror, Mirror.docx",
                outputPath);

            Assert.IsTrue(result.IsSuccess);
            foreach (var character in result.Response!.Characters!)
            {
                if (string.IsNullOrWhiteSpace(character.CharacterSketch) ||
                    string.IsNullOrWhiteSpace(character.Appearance))
                    continue;
                Assert.AreNotEqual(character.CharacterSketch, character.Appearance,
                    $"Sketch and Appearance must not be identical for {character.Name}.");
            }
        }

        [TestMethod]
        public async Task StubbedPipeline_MirrorMirror_RatingArtifactWritten()
        {
            var canned = await LoadFixtureAsync();
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
