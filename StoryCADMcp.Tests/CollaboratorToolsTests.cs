using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADLib.Models;
using StoryCADLib.Services;
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
        }
    }
}
