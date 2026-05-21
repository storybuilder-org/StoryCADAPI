using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADCritter;
using StoryCADLib.Models;
using StoryCADLib.Services.API;
using StoryCADLib.Services.IoC;

namespace StoryCADCritterTests;

[TestClass]
public class CritiqueOrchestratorTests
{
    private static StoryCADApi _api = null!;
    private static string _systemPrompt = null!;
    private static string _outputDir = null!;

    [ClassInitialize]
    public static void ClassSetup(TestContext _)
    {
        _api = Ioc.Default.GetRequiredService<StoryCADApi>();

        var promptPath = Path.Combine(AppContext.BaseDirectory, "Prompts", "CritiquePrompt.md");
        _systemPrompt = File.ReadAllText(promptPath);

        _outputDir = Path.Combine(AppContext.BaseDirectory, "TestOutputs");
        Directory.CreateDirectory(_outputDir);
    }

    [TestMethod]
    public async Task StubbedWalk_LighthouseKeeper_ProducesReport()
    {
        await LighthouseKeeperFixture.BuildAsync(_api);

        var stub = new StubChatCompletionService();
        var orchestrator = new CritiqueOrchestrator(_api, stub, _systemPrompt);

        var outlinePath = Path.Combine(_outputDir, "Lighthouse_HappyPath.demo");
        var run = await orchestrator.RunAsync(outlinePath, progress: null, outputDirectory: _outputDir);

        Assert.IsFalse(run.HardFailed, $"HardFailureMessage: {run.HardFailureMessage}");
        Assert.IsFalse(run.ShortCircuited, $"ShortCircuitReason: {run.ShortCircuitReason}");
        Assert.IsTrue(run.ElementCritiques.Count > 0, "No critiques produced.");
        Assert.AreEqual(run.ElementCritiques.Count, stub.CallCount, "One LLM call per critiqued element.");
        Assert.IsTrue(run.ElementCritiques.All(c => c.Parsed != null),
            $"Some critiques failed to parse: {string.Join(", ", run.ElementCritiques.Where(c => c.Parsed == null).Select(c => c.Name))}");
    }

    [TestMethod]
    public async Task StubbedWalk_MalformedResponse_FallsBackToRawText()
    {
        await LighthouseKeeperFixture.BuildAsync(_api);

        const string targetName = "Marina Torres";
        const string garbage = "Sorry, I cannot critique this element today.";
        var stub = new StubChatCompletionService
        {
            Respond = userMessage =>
                userMessage.Contains(targetName)
                    ? garbage
                    : StubChatCompletionService.DefaultValidJson
        };

        var orchestrator = new CritiqueOrchestrator(_api, stub, _systemPrompt);
        var outlinePath = Path.Combine(_outputDir, "Lighthouse_Malformed.demo");
        var run = await orchestrator.RunAsync(outlinePath, progress: null, outputDirectory: _outputDir);

        Assert.IsFalse(run.HardFailed, $"HardFailureMessage: {run.HardFailureMessage}");
        var failed = run.ElementCritiques.SingleOrDefault(c => c.Name == targetName);
        Assert.IsNotNull(failed, $"Couldn't find '{targetName}' in critiques.");
        Assert.IsTrue(failed.ParseFailed, "Targeted element should have ParseFailed == true.");
        Assert.AreEqual(garbage, failed.RawResponse);
        Assert.IsNull(failed.ErrorMessage, "ParseFailed should not also be CallFailed.");

        // Render the report and assert the parse-failure banner appears.
        var report = CritiqueOrchestrator.RenderReport(run, "Lighthouse_Malformed");
        StringAssert.Contains(report, "Couldn't parse the LLM response");
    }

    [TestMethod]
    public async Task ShortCircuit_OutlineWithoutScenes_SkipsLLM()
    {
        // Build a minimal outline: Overview + 1 Character, no Scenes.
        var created = await _api.CreateEmptyOutline("ShortCircuitTest", "Test Author", "0");
        Assert.IsTrue(created.IsSuccess, $"CreateEmptyOutline failed: {created.ErrorMessage}");
        var overviewGuid = created.Payload.FirstOrDefault();
        Assert.AreNotEqual(Guid.Empty, overviewGuid);
        _api.AddElement(StoryItemType.Character, overviewGuid.ToString(), "Solo Character");

        var stub = new StubChatCompletionService();
        var orchestrator = new CritiqueOrchestrator(_api, stub, _systemPrompt);
        var outlinePath = Path.Combine(_outputDir, "ShortCircuit.demo");
        var run = await orchestrator.RunAsync(outlinePath, progress: null, outputDirectory: _outputDir);

        Assert.IsTrue(run.ShortCircuited, "Thin outline should short-circuit.");
        Assert.IsNotNull(run.ShortCircuitReason);
        Assert.AreEqual(0, stub.CallCount, "No LLM calls expected on short-circuit.");
        Assert.AreEqual(0, run.ElementCritiques.Count);
    }
}
