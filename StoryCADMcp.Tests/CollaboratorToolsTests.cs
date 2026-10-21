using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADLib.Models;
using StoryCADLib.Services;
using Microsoft.Extensions.DependencyInjection;
using StoryCADMcp;
using StoryCADMcp.Tools;
using StoryCollaborator.Models;
using StoryCollaborator.Services;
using StoryCollaborator.Workflows;

namespace StoryCADMcp.Tests;

// Collaborator #291, design section 9, tests 17 to 20. No test calls the Worker: a test that
// needs a run puts a WorkflowResult with known proposals into a WorkflowSession, and the code
// after the model call runs for real.
[TestClass]
public class CollaboratorToolsTests
{
    private const string FakeToken = "tok-SECRET-0f3a9c.payload.signature";

    private List<Guid> _guids = null!;
    private Guid _overview;

    [TestInitialize]
    public async Task Setup()
    {
        CollaboratorTools.EndSession();
        _guids = await TestDataSetup.CreateTestOutline("Collaborator Tools Test");
        _overview = _guids
            .Select(g => TestDataSetup.Api.GetStoryElement(g).Payload)
            .First(e => e.ElementType == StoryItemType.StoryOverview).Uuid;
    }

    [TestCleanup]
    public void Cleanup() => CollaboratorTools.EndSession();

    private WorkflowSession OpenSession(UpdateKind kind, string writerText = "")
    {
        if (writerText.Length > 0)
        {
            var written = TestDataSetup.Api.UpdateElementProperty(_overview, "Description", writerText);
            Assert.IsTrue(written.IsSuccess, written.ErrorMessage);
        }

        var session = new WorkflowSession(WorkflowRegistry.All.First(), TestDataSetup.Api.CurrentModel);
        session.Result = WorkflowResult.Succeeded();
        session.Result.PendingUpdates.Add(new PendingUpdate(
            "Overview", _overview, new PropertySpec("Description"), "A new story idea.", kind, writerText));
        session.Proposals = new SessionProposalSet();
        session.Proposals.ReplaceFromPending(session.Result.PendingUpdates);
        CollaboratorTools.UseSession(session);
        return session;
    }

    [TestMethod]
    public async Task RunWorkflow_NoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = await CollaboratorTools.RunWorkflow(
            TestDataSetup.Api, WorkflowRegistry.All.First().Label, _overview.ToString());

        Assert.IsTrue(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("outline", StringComparison.OrdinalIgnoreCase), result);
    }

    [TestMethod]
    public async Task Accept_UnknownKey_ReturnsOpenKeys()
    {
        OpenSession(UpdateKind.Fill);

        var result = await CollaboratorTools.Accept(TestDataSetup.Api, "Nobody.Nothing");

        Assert.IsTrue(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("Overview.Description"), result);
    }

    [TestMethod]
    public async Task Accept_ProtectWithoutOverwrite_ReturnsErrorAndWritesNothing()
    {
        var session = OpenSession(UpdateKind.Protect, "The writer wrote this.");

        var result = await CollaboratorTools.Accept(TestDataSetup.Api, "Overview.Description");

        Assert.IsTrue(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("overwrite_your_text"), result);
        var element = TestDataSetup.Api.GetStoryElement(_overview).Payload;
        Assert.AreEqual("The writer wrote this.", element.Description);
        Assert.AreEqual(ProposalSessionStatus.Open, session.Proposals!.Get("Overview.Description")!.Status);
        Assert.AreEqual(0, session.WriteCount);
    }

    [TestMethod]
    public async Task ToolResults_NeverContainActivationToken()
    {
        // An expired token in a preferences file the server reads. No Worker call is possible:
        // the token is past its expiry, the path is "store", and dev activation is off.
        var prefsPath = Path.Combine(TestDataSetup.TempDir, "token_prefs.json");
        File.WriteAllText(prefsPath, System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["StoreUserGuid"] = Guid.NewGuid().ToString(),
            ["StoreActivationJwt"] = FakeToken,
            ["StoreActivationJwtExpiry"] = "2020-01-01T00:00:00Z",
            ["StoreActivationPath"] = "store"
        }));
        var model = Ioc.Default.GetRequiredService<PreferenceService>().Model;
        var (savedGuid, savedJwt, savedExpiry, savedPath) = (model.StoreUserGuid, model.StoreActivationJwt,
            model.StoreActivationJwtExpiry, model.StoreActivationPath);
        var devActivation = Environment.GetEnvironmentVariable("COLLAB_DEV_ACTIVATION");
        var priorPath = CollaboratorSignIn.PreferencesPathOverride;
        Environment.SetEnvironmentVariable("COLLAB_DEV_ACTIVATION", null);
        CollaboratorSignIn.PreferencesPathOverride = prefsPath;
        try
        {
            OpenSession(UpdateKind.Fill);
            var label = WorkflowRegistry.All.First().Label;
            var results = new List<string>
            {
                CollaboratorTools.ListWorkflows(),
                await CollaboratorTools.RunWorkflow(TestDataSetup.Api, label, _overview.ToString()),
                CollaboratorTools.GetProposals(),
                await CollaboratorTools.Accept(TestDataSetup.Api, "Nobody.Nothing"),
                CollaboratorTools.Skip(TestDataSetup.Api, "Nobody.Nothing"),
                await CollaboratorTools.AcceptAll(TestDataSetup.Api),
                CollaboratorTools.Settings(terseness: "Concise"),
                CollaboratorTools.Settings()
            };

            // The run stopped at the expired sign-in, so no request left the test.
            StringAssert.Contains(results[1], CollaboratorSignIn.ExpiredMessage);

            foreach (var result in results)
            {
                Assert.IsFalse(result.Contains(FakeToken), result);
                Assert.IsFalse(result.Contains("tok-SECRET"), result);
            }
        }
        finally
        {
            CollaboratorSignIn.PreferencesPathOverride = priorPath;
            Environment.SetEnvironmentVariable("COLLAB_DEV_ACTIVATION", devActivation);
            CollaboratorTools.Settings(terseness: "Balanced");
            (model.StoreUserGuid, model.StoreActivationJwt, model.StoreActivationJwtExpiry,
                model.StoreActivationPath) = (savedGuid, savedJwt, savedExpiry, savedPath);
        }
    }

    // Opens a saved copy of the outline through open_outline, as a client does, and seeds a
    // Fill proposal for its overview. No Worker call: the result is made by hand.
    private async Task<(WorkflowSession Session, Guid Overview, string File)> OpenSavedOutlineWithFill()
    {
        var file = await TestDataSetup.SaveToTemp($"backup_{Guid.NewGuid():N}.stbx");
        var opened = await OutlineTools.OpenOutline(TestDataSetup.Api, file);
        Assert.IsFalse(opened.StartsWith("Error:"), opened);
        var overview = TestDataSetup.Api.GetAllElements().Payload
            .First(e => e.ElementType == StoryItemType.StoryOverview);

        var session = new WorkflowSession(WorkflowRegistry.All.First(), TestDataSetup.Api.CurrentModel)
        {
            FilePath = file
        };
        session.Elements["Overview"] = overview;
        session.Result = WorkflowResult.Succeeded();
        session.Result.PendingUpdates.Add(new PendingUpdate(
            "Overview", overview.Uuid, new PropertySpec("Description"), "A new story idea.", UpdateKind.Fill, ""));
        session.Proposals = new SessionProposalSet();
        session.Proposals.ReplaceFromPending(session.Result.PendingUpdates);
        CollaboratorTools.UseSession(session);
        return (session, overview.Uuid, file);
    }

    [TestMethod]
    public async Task Accept_FirstWrite_BacksUpTheFileAndSavesTheWrite()
    {
        var folder = Path.Combine(TestDataSetup.TempDir, $"backups_{Guid.NewGuid():N}");
        CollaboratorTools.BackupFolderOverride = folder;
        try
        {
            var (_, overview, file) = await OpenSavedOutlineWithFill();

            var result = await CollaboratorTools.Accept(TestDataSetup.Api, "Overview.Description");

            Assert.IsFalse(result.StartsWith("Error:"), result);
            var zips = Directory.GetFiles(folder, "*.zip");
            Assert.AreEqual(1, zips.Length);
            using (var zip = System.IO.Compression.ZipFile.OpenRead(zips[0]))
                Assert.AreEqual(Path.GetFileName(file), zip.Entries.Single().Name);

            // The write reached the file, not only the model.
            await OutlineTools.OpenOutline(TestDataSetup.Api, file);
            var reread = TestDataSetup.Api.GetStoryElement(overview).Payload;
            Assert.AreEqual("A new story idea.", reread.Description);
        }
        finally
        {
            CollaboratorTools.BackupFolderOverride = null;
        }
    }

    [TestMethod]
    public async Task Accept_BackupFails_ReturnsErrorAndWritesNothing()
    {
        // A file where the backup folder should be makes the backup fail.
        var blocker = Path.Combine(TestDataSetup.TempDir, $"blocker_{Guid.NewGuid():N}");
        File.WriteAllText(blocker, "not a folder");
        CollaboratorTools.BackupFolderOverride = blocker;
        try
        {
            var (session, overview, _) = await OpenSavedOutlineWithFill();

            var result = await CollaboratorTools.Accept(TestDataSetup.Api, "Overview.Description");

            Assert.IsTrue(result.StartsWith("Error:"), result);
            Assert.IsTrue(result.Contains("backup", StringComparison.OrdinalIgnoreCase), result);
            Assert.AreEqual(0, session.WriteCount);
            Assert.AreNotEqual("A new story idea.", TestDataSetup.Api.GetStoryElement(overview).Payload.Description);
        }
        finally
        {
            CollaboratorTools.BackupFolderOverride = null;
        }
    }

    [TestMethod]
    public async Task RunWorkflow_ElementOfWrongType_ReturnsErrorBeforeTheRun()
    {
        var workflow = WorkflowRegistry.All.First(w =>
            w.Mode == WorkflowMode.OneShot && w.PrimaryElementType != StoryItemType.StoryOverview
            && w.PrimaryElementType != StoryItemType.Unknown);

        var result = await CollaboratorTools.RunWorkflow(TestDataSetup.Api, workflow.Label, _overview.ToString());

        Assert.IsTrue(result.StartsWith("Error:"), result);
        StringAssert.Contains(result, workflow.PrimaryElementType.ToString());
    }

    [TestMethod]
    public async Task RunWorkflow_InputGuidNotInOutline_ReturnsErrorBeforeTheRun()
    {
        var workflow = WorkflowRegistry.All.First(w =>
            w.Mode == WorkflowMode.OneShot && w.PrimaryElementType == StoryItemType.StoryOverview
            && w.GetIO().OptionalInputs.Count + w.GetIO().RequiredInputs.Count > 0);
        var io = workflow.GetIO();
        var name = io.RequiredInputs.Concat(io.OptionalInputs).First().ElementLabel;

        var result = await CollaboratorTools.RunWorkflow(TestDataSetup.Api, workflow.Label, _overview.ToString(),
            new Dictionary<string, string> { [name] = Guid.NewGuid().ToString() });

        Assert.IsTrue(result.StartsWith("Error:"), result);
        StringAssert.Contains(result, name);
    }

    [TestMethod]
    public async Task Tools_WhileACallRuns_ReturnBusyError()
    {
        OpenSession(UpdateKind.Fill);
        Assert.IsTrue(CollaboratorTools.TryEnter(out _));
        try
        {
            var skip = CollaboratorTools.Skip(TestDataSetup.Api, "Overview.Description");
            var settings = CollaboratorTools.Settings(terseness: "Concise");
            var accept = await CollaboratorTools.Accept(TestDataSetup.Api, "Overview.Description");
            var close = OutlineTools.CloseOutline(TestDataSetup.Api);

            foreach (var result in new[] { skip, settings, accept, close })
                StringAssert.StartsWith(result, "Error:");
            Assert.IsNotNull(TestDataSetup.Api.CurrentModel, "close_outline must not close during a call");
        }
        finally
        {
            CollaboratorTools.Leave();
        }
    }

    // A sign-in that has not expired: the activation service takes it as Active without a Worker
    // call. The tests below stop before any request would leave the machine.
    private async Task WithFakeSignIn(Func<Task> test)
    {
        var prefsPath = Path.Combine(TestDataSetup.TempDir, $"live_prefs_{Guid.NewGuid():N}.json");
        File.WriteAllText(prefsPath, System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["StoreUserGuid"] = Guid.NewGuid().ToString(),
            ["StoreActivationJwt"] = FakeToken,
            ["StoreActivationJwtExpiry"] = DateTime.UtcNow.AddHours(6).ToString("O"),
            ["StoreActivationPath"] = "store"
        }));
        var model = Ioc.Default.GetRequiredService<PreferenceService>().Model;
        var (savedGuid, savedJwt, savedExpiry, savedPath) = (model.StoreUserGuid, model.StoreActivationJwt,
            model.StoreActivationJwtExpiry, model.StoreActivationPath);
        var priorPath = CollaboratorSignIn.PreferencesPathOverride;
        CollaboratorSignIn.PreferencesPathOverride = prefsPath;
        try
        {
            await test();
        }
        finally
        {
            CollaboratorSignIn.PreferencesPathOverride = priorPath;
            (model.StoreUserGuid, model.StoreActivationJwt, model.StoreActivationJwtExpiry,
                model.StoreActivationPath) = (savedGuid, savedJwt, savedExpiry, savedPath);
        }
    }

    [TestMethod]
    public async Task TryAgain_RunFails_ReturnsTheSessionError()
    {
        await WithFakeSignIn(async () =>
        {
            // A workflow with a required input and a session with no input elements: the runner
            // refuses before it builds a request.
            var workflow = WorkflowRegistry.All.First(w =>
                w.Mode == WorkflowMode.OneShot && w.GetIO().RequiredInputs.Count > 0);
            var session = new WorkflowSession(workflow, TestDataSetup.Api.CurrentModel);
            CollaboratorTools.UseSession(session);

            var result = await CollaboratorTools.TryAgain(TestDataSetup.Api);

            Assert.IsTrue(result.StartsWith("Error:"), result);
            StringAssert.Contains(result, "Missing required element");
            Assert.IsFalse(result.Contains(FakeToken), result);
        });
    }

    [TestMethod]
    public async Task CharacterInterview_OpensAndChatReadsTheChoiceWithoutWorker()
    {
        await WithFakeSignIn(async () =>
        {
            var added = TestDataSetup.Api.AddElement(StoryItemType.Character, _overview.ToString(), "Hero");
            Assert.IsTrue(added.IsSuccess, added.ErrorMessage);
            var character = TestDataSetup.Api.GetStoryElement(added.Payload).Payload;

            var opened = await CollaboratorTools.RunWorkflow(
                TestDataSetup.Api, "CharacterInterview", character.Uuid.ToString());

            Assert.IsFalse(opened.StartsWith("Error:"), opened);
            using (var doc = System.Text.Json.JsonDocument.Parse(opened))
            {
                Assert.IsTrue(doc.RootElement.GetProperty("interview").GetBoolean());
                Assert.IsFalse(string.IsNullOrWhiteSpace(doc.RootElement.GetProperty("choices").GetString()));
            }

            // Text the choice parser cannot read gets the list again; no request is made.
            var chat = await CollaboratorTools.Chat(TestDataSetup.Api, "zzzz qqqq");

            Assert.IsFalse(chat.StartsWith("Error:"), chat);
            StringAssert.Contains(chat, "did not catch");
        });
    }
}
