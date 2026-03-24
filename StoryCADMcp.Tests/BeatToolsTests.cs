using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADMcp.Tools;

namespace StoryCADMcp.Tests;

[TestClass]
public class BeatToolsTests
{
    private List<Guid> _guids = null!;
    private string _problemGuid = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _guids = await TestDataSetup.CreateTestOutline("Beat Test Outline");
        var parentGuid = _guids[0].ToString();

        // Create a problem and apply a beat sheet for most tests
        var problemResult = WriteTools.AddElement(TestDataSetup.Api, "Problem", parentGuid, "Beat Test Problem");
        Assert.IsFalse(problemResult.StartsWith("Error:"), problemResult);
        _problemGuid = ExtractGuid(problemResult);

        var applyResult = WriteTools.ApplyBeatSheet(TestDataSetup.Api, _problemGuid, "Three Act Play");
        Assert.IsFalse(applyResult.StartsWith("Error:"), applyResult);
    }

    #region AssignBeat

    [TestMethod]
    public void AssignBeat_WithScene_ReturnsSuccess()
    {
        var parentGuid = _guids[0].ToString();
        var sceneResult = WriteTools.AddElement(TestDataSetup.Api, "Scene", parentGuid, "Assigned Scene");
        Assert.IsFalse(sceneResult.StartsWith("Error:"), sceneResult);
        var sceneGuid = ExtractGuid(sceneResult);

        var result = BeatTools.AssignBeat(TestDataSetup.Api, _problemGuid, 2, sceneGuid);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("assigned"), result);
    }

    [TestMethod]
    public void AssignBeat_WithProblem_ReturnsSuccess()
    {
        var parentGuid = _guids[0].ToString();
        var subProblemResult = WriteTools.AddElement(TestDataSetup.Api, "Problem", parentGuid, "Sub Problem");
        Assert.IsFalse(subProblemResult.StartsWith("Error:"), subProblemResult);
        var subProblemGuid = ExtractGuid(subProblemResult);

        var result = BeatTools.AssignBeat(TestDataSetup.Api, _problemGuid, 6, subProblemGuid);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("assigned"), result);
    }

    [TestMethod]
    public void AssignBeat_WithInvalidBeatIndex_ReturnsError()
    {
        var parentGuid = _guids[0].ToString();
        var sceneResult = WriteTools.AddElement(TestDataSetup.Api, "Scene", parentGuid, "Scene");
        var sceneGuid = ExtractGuid(sceneResult);

        var result = BeatTools.AssignBeat(TestDataSetup.Api, _problemGuid, 99, sceneGuid);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void AssignBeat_WithInvalidProblemGuid_ReturnsError()
    {
        var result = BeatTools.AssignBeat(TestDataSetup.Api, "not-a-guid", 0, Guid.NewGuid().ToString());

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void AssignBeat_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = BeatTools.AssignBeat(TestDataSetup.Api, Guid.NewGuid().ToString(), 0, Guid.NewGuid().ToString());

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    #endregion

    #region ClearBeat

    [TestMethod]
    public void ClearBeat_AfterAssign_ReturnsSuccess()
    {
        var parentGuid = _guids[0].ToString();
        var sceneResult = WriteTools.AddElement(TestDataSetup.Api, "Scene", parentGuid, "Clearable Scene");
        var sceneGuid = ExtractGuid(sceneResult);

        BeatTools.AssignBeat(TestDataSetup.Api, _problemGuid, 2, sceneGuid);

        var result = BeatTools.ClearBeat(TestDataSetup.Api, _problemGuid, 2);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("cleared"), result);
    }

    [TestMethod]
    public void ClearBeat_WithInvalidIndex_ReturnsError()
    {
        var result = BeatTools.ClearBeat(TestDataSetup.Api, _problemGuid, 99);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void ClearBeat_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = BeatTools.ClearBeat(TestDataSetup.Api, Guid.NewGuid().ToString(), 0);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    #endregion

    #region CreateBeat

    [TestMethod]
    public void CreateBeat_WithValidArgs_ReturnsSuccess()
    {
        var result = BeatTools.CreateBeat(TestDataSetup.Api, _problemGuid, "New Beat", "A custom beat");

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("created"), result);
        Assert.IsTrue(result.Contains("New Beat"), result);
    }

    [TestMethod]
    public void CreateBeat_WithInvalidGuid_ReturnsError()
    {
        var result = BeatTools.CreateBeat(TestDataSetup.Api, "not-a-guid", "Title", "Desc");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void CreateBeat_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = BeatTools.CreateBeat(TestDataSetup.Api, Guid.NewGuid().ToString(), "Title", "Desc");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    #endregion

    #region UpdateBeat

    [TestMethod]
    public void UpdateBeat_WithValidArgs_ReturnsSuccess()
    {
        var result = BeatTools.UpdateBeat(TestDataSetup.Api, _problemGuid, 2, "Renamed Beat", "New description");

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("updated"), result);
        Assert.IsTrue(result.Contains("Renamed Beat"), result);
    }

    [TestMethod]
    public void UpdateBeat_WithInvalidIndex_ReturnsError()
    {
        var result = BeatTools.UpdateBeat(TestDataSetup.Api, _problemGuid, 99, "Title", "Desc");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void UpdateBeat_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = BeatTools.UpdateBeat(TestDataSetup.Api, Guid.NewGuid().ToString(), 0, "Title", "Desc");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    #endregion

    #region DeleteBeat

    [TestMethod]
    public void DeleteBeat_WithValidIndex_ReturnsSuccess()
    {
        // Delete the last beat (Denouement, index 11)
        var result = BeatTools.DeleteBeat(TestDataSetup.Api, _problemGuid, 11);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("deleted"), result);
    }

    [TestMethod]
    public void DeleteBeat_WithInvalidIndex_ReturnsError()
    {
        var result = BeatTools.DeleteBeat(TestDataSetup.Api, _problemGuid, 99);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void DeleteBeat_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = BeatTools.DeleteBeat(TestDataSetup.Api, Guid.NewGuid().ToString(), 0);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    #endregion

    #region MoveBeat

    [TestMethod]
    public void MoveBeat_WithValidIndices_ReturnsSuccess()
    {
        var result = BeatTools.MoveBeat(TestDataSetup.Api, _problemGuid, 2, 4);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("moved"), result);
    }

    [TestMethod]
    public void MoveBeat_WithInvalidFromIndex_ReturnsError()
    {
        var result = BeatTools.MoveBeat(TestDataSetup.Api, _problemGuid, 99, 0);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void MoveBeat_WithInvalidToIndex_ReturnsError()
    {
        var result = BeatTools.MoveBeat(TestDataSetup.Api, _problemGuid, 0, 99);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void MoveBeat_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = BeatTools.MoveBeat(TestDataSetup.Api, Guid.NewGuid().ToString(), 0, 1);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    #endregion

    #region SaveBeatSheet

    [TestMethod]
    public void SaveBeatSheet_WithValidArgs_ReturnsSuccess()
    {
        var filePath = Path.Combine(TestDataSetup.TempDir, "test-export.stbeat");

        var result = BeatTools.SaveBeatSheet(TestDataSetup.Api, _problemGuid, filePath);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("saved"), result);
        Assert.IsTrue(File.Exists(filePath), "Exported .stbeat file should exist");
    }

    [TestMethod]
    public void SaveBeatSheet_WithInvalidGuid_ReturnsError()
    {
        var filePath = Path.Combine(TestDataSetup.TempDir, "bad.stbeat");

        var result = BeatTools.SaveBeatSheet(TestDataSetup.Api, "not-a-guid", filePath);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void SaveBeatSheet_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = BeatTools.SaveBeatSheet(TestDataSetup.Api, Guid.NewGuid().ToString(), "/tmp/test.stbeat");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    #endregion

    #region LoadBeatSheet

    [TestMethod]
    public void LoadBeatSheet_FromExportedFile_ReturnsSuccess()
    {
        // Export first
        var filePath = Path.Combine(TestDataSetup.TempDir, "roundtrip.stbeat");
        var saveResult = BeatTools.SaveBeatSheet(TestDataSetup.Api, _problemGuid, filePath);
        Assert.IsFalse(saveResult.StartsWith("Error:"), saveResult);

        // Create a second problem and load into it
        var parentGuid = _guids[0].ToString();
        var problem2Result = WriteTools.AddElement(TestDataSetup.Api, "Problem", parentGuid, "Import Target");
        Assert.IsFalse(problem2Result.StartsWith("Error:"), problem2Result);
        var problem2Guid = ExtractGuid(problem2Result);

        var result = BeatTools.LoadBeatSheet(TestDataSetup.Api, problem2Guid, filePath);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("loaded"), result);
    }

    [TestMethod]
    public void LoadBeatSheet_WithMissingFile_ReturnsError()
    {
        var result = BeatTools.LoadBeatSheet(TestDataSetup.Api, _problemGuid, "/tmp/nonexistent.stbeat");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void LoadBeatSheet_WithInvalidGuid_ReturnsError()
    {
        var result = BeatTools.LoadBeatSheet(TestDataSetup.Api, "not-a-guid", "/tmp/test.stbeat");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void LoadBeatSheet_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = BeatTools.LoadBeatSheet(TestDataSetup.Api, Guid.NewGuid().ToString(), "/tmp/test.stbeat");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    #endregion

    #region Integration

    [TestMethod]
    public void AssignBeat_ThenVerifyViaStructure_ShowsLinkedElement()
    {
        var parentGuid = _guids[0].ToString();
        var sceneResult = WriteTools.AddElement(TestDataSetup.Api, "Scene", parentGuid, "Verified Scene");
        Assert.IsFalse(sceneResult.StartsWith("Error:"), sceneResult);
        var sceneGuid = ExtractGuid(sceneResult);

        // Assign scene to beat index 2 (Narrative hook)
        var assignResult = BeatTools.AssignBeat(TestDataSetup.Api, _problemGuid, 2, sceneGuid);
        Assert.IsFalse(assignResult.StartsWith("Error:"), assignResult);

        // Verify via get_problem_structure
        var structResult = ReadTools.GetProblemStructure(TestDataSetup.Api, _problemGuid);
        Assert.IsFalse(structResult.StartsWith("Error:"), structResult);
        Assert.IsTrue(structResult.Contains(sceneGuid), "Structure should contain the assigned scene GUID");
    }

    [TestMethod]
    public void ClearBeat_ThenVerifyViaStructure_ShowsNull()
    {
        var parentGuid = _guids[0].ToString();
        var sceneResult = WriteTools.AddElement(TestDataSetup.Api, "Scene", parentGuid, "Cleared Scene");
        var sceneGuid = ExtractGuid(sceneResult);

        BeatTools.AssignBeat(TestDataSetup.Api, _problemGuid, 2, sceneGuid);
        BeatTools.ClearBeat(TestDataSetup.Api, _problemGuid, 2);

        var structResult = ReadTools.GetProblemStructure(TestDataSetup.Api, _problemGuid);
        Assert.IsFalse(structResult.StartsWith("Error:"), structResult);
        // Beat at index 2 should have null linkedElement after clearing
        Assert.IsFalse(structResult.Contains(sceneGuid), "Cleared beat should not contain the scene GUID");
    }

    #endregion

    private static string ExtractGuid(string json)
    {
        var start = json.IndexOf("\"guid\": \"") + 9;
        var end = json.IndexOf("\"", start);
        return json.Substring(start, end - start);
    }
}
