using System;
using System.Collections.Generic;
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
    public async Task StubbedWalk_PromptCarriesSerializedElementData_NotBareUuid()
    {
        await LighthouseKeeperFixture.BuildAsync(_api);

        // Capture every user message the orchestrator sends. The walk is
        // parallel, so guard the list.
        var prompts = new List<string>();
        var stub = new StubChatCompletionService
        {
            Respond = userMessage =>
            {
                lock (prompts) prompts.Add(userMessage);
                return StubChatCompletionService.DefaultValidJson;
            }
        };

        var orchestrator = new CritiqueOrchestrator(_api, stub, _systemPrompt);
        var outlinePath = Path.Combine(_outputDir, "PromptData.demo");
        await orchestrator.RunAsync(outlinePath, progress: null, outputDirectory: _outputDir);

        // The "Save the Lighthouse" Problem sets ProtGoal in the fixture. Its
        // prompt must contain both the JSON field name and the actual value,
        // which only happens if GetBody serializes the element rather than
        // emitting the bare UUID. Regression guard for the bug where
        // StoryElement.ToString() (just the UUID) was sent as "element data".
        List<string> snapshot;
        lock (prompts) snapshot = prompts.ToList();

        var problemPrompt = snapshot.FirstOrDefault(p => p.Contains("Save the Lighthouse"));
        Assert.IsNotNull(problemPrompt, "No prompt for the 'Save the Lighthouse' problem was captured.");
        StringAssert.Contains(problemPrompt, "ProtGoal",
            "Problem prompt is missing serialized field names — GetBody may be sending only the UUID.");
        // Substring stops before "father's" — System.Text.Json escapes the
        // apostrophe to ', so we assert on apostrophe-free text.
        StringAssert.Contains(problemPrompt, "Preserve the lighthouse and her father",
            "Problem prompt is missing the actual ProtGoal value — element data isn't reaching the LLM.");
    }

    [TestMethod]
    public void PreferencesService_RoundTripsAllFields()
    {
        var path = Path.Combine(_outputDir, $"prefs_{Guid.NewGuid():N}.json");
        var svc = new PreferencesService(path);

        svc.Save(new CritterPreferences
        {
            MaxConcurrency = 12,
            KeyQuestionsPlacement = "Separate",
            SelectedModelId = "gpt-4o"
        });
        var loaded = svc.Load();

        Assert.AreEqual(12, loaded.MaxConcurrency);
        Assert.AreEqual("Separate", loaded.KeyQuestionsPlacement);
        Assert.AreEqual("gpt-4o", loaded.SelectedModelId);

        // Missing file → defaults, not a throw.
        var fresh = new PreferencesService(Path.Combine(_outputDir, $"missing_{Guid.NewGuid():N}.json")).Load();
        Assert.AreEqual(8, fresh.MaxConcurrency);
        Assert.AreEqual("Inline", fresh.KeyQuestionsPlacement);
    }

    [TestMethod]
    public void RenderReport_SeparatePlacement_ConsolidatesKeyQuestions()
    {
        var run = new CritiqueRunResult { OutlinePath = "demo.stbx" };
        run.ElementCritiques.Add(new ElementCritique
        {
            Uuid = Guid.NewGuid(),
            Name = "Joseph",
            ElementType = "Character",
            KeyQuestions = new() { ("Role Tab", "Is Joseph fleshed out?") },
            Parsed = new CritiqueElementResponse { ElementName = "Joseph", ElementType = "Character" }
        });

        var inline = CritiqueOrchestrator.RenderReport(run, "demo", separateKeyQuestions: false);
        StringAssert.Contains(inline, "<details><summary>Key Questions used</summary>");

        var separate = CritiqueOrchestrator.RenderReport(run, "demo", separateKeyQuestions: true);
        StringAssert.Contains(separate, "## Key Questions");
        Assert.IsFalse(separate.Contains("<details>"),
            "Separate placement should not emit inline <details> blocks.");
    }

    [TestMethod]
    public void PersonalizeKeyQuestions_MaleCharacter_UsesNameAndMasculinePronouns()
    {
        // A rubric question that mixes the generic role noun with female-default
        // pronouns — the exact mis-gendering seen against male characters.
        const string q = "Is your character fleshed out? Does she have the dimensionality of a real person? Can you visualize her?";

        var result = CritiqueOrchestrator.PersonalizeCharacterQuestion(q, "Joseph", "Male");

        StringAssert.Contains(result, "Joseph", "Generic 'your character' should become the name.");
        StringAssert.Contains(result, "Does he have", "Subject pronoun should be masculine.");
        StringAssert.Contains(result, "visualize him", "Object pronoun should be masculine.");
        Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(result, @"\b(she|her|hers)\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase),
            $"No feminine pronouns should remain: {result}");
    }

    [TestMethod]
    public void PersonalizeKeyQuestions_UnknownSex_FallsBackToName()
    {
        // Arrogance (the horse) has no Sex on file — never guess a gender.
        const string q = "Can you visualize her? Is she central to her outlook?";

        var result = CritiqueOrchestrator.PersonalizeCharacterQuestion(q, "Arrogance", null);

        StringAssert.Contains(result, "visualize Arrogance", "Object pronoun should fall back to the name.");
        StringAssert.Contains(result, "Arrogance's outlook", "Possessive should become Name's.");
        Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(result, @"\b(she|he|her|him|his)\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase),
            $"No gendered pronouns should remain: {result}");
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
