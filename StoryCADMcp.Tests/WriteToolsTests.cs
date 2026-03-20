using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADLib.Models;
using StoryCADMcp.Tools;

namespace StoryCADMcp.Tests;

[TestClass]
public class WriteToolsTests
{
    private List<Guid> _guids = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _guids = await TestDataSetup.CreateTestOutline("Write Test Outline");
    }

    [TestMethod]
    public void AddElement_WithValidArgs_ReturnsNewGuid()
    {
        // First GUID from CreateEmptyOutline is the overview — use it as parent
        var parentGuid = _guids[0].ToString();

        var result = WriteTools.AddElement(TestDataSetup.Api, "Character", parentGuid, "Test Character");

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("guid"));
        Assert.IsTrue(result.Contains("Test Character"));
        Assert.IsTrue(result.Contains("Character"));
    }

    [TestMethod]
    public void AddElement_WithInvalidType_ReturnsError()
    {
        var parentGuid = _guids[0].ToString();

        var result = WriteTools.AddElement(TestDataSetup.Api, "InvalidType", parentGuid, "Test");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void AddElement_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = WriteTools.AddElement(TestDataSetup.Api, "Character", Guid.NewGuid().ToString(), "Test");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void UpdateProperty_WithValidArgs_ReturnsSuccess()
    {
        var parentGuid = _guids[0].ToString();
        var addResult = WriteTools.AddElement(TestDataSetup.Api, "Character", parentGuid, "Updatable Character");
        Assert.IsFalse(addResult.StartsWith("Error:"), addResult);

        // Extract GUID from the add result
        var guid = ExtractGuid(addResult);

        var result = WriteTools.UpdateProperty(TestDataSetup.Api, guid, "Name", "Updated Character");

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("updated"), result);
    }

    [TestMethod]
    public void UpdateProperty_WithInvalidGuid_ReturnsError()
    {
        var result = WriteTools.UpdateProperty(TestDataSetup.Api, "not-a-guid", "Name", "Test");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public async Task DeleteElement_WithValidGuid_ReturnsSuccess()
    {
        var parentGuid = _guids[0].ToString();
        var addResult = WriteTools.AddElement(TestDataSetup.Api, "Notes", parentGuid, "To Be Deleted");
        Assert.IsFalse(addResult.StartsWith("Error:"), addResult);

        var guid = ExtractGuid(addResult);

        var result = await WriteTools.DeleteElement(TestDataSetup.Api, guid);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("deleted"), result);
    }

    [TestMethod]
    public async Task DeleteElement_WithInvalidGuid_ReturnsError()
    {
        var result = await WriteTools.DeleteElement(TestDataSetup.Api, "not-a-guid");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void LinkCast_WithValidGuids_ReturnsSuccess()
    {
        var parentGuid = _guids[0].ToString();

        // Create a scene and a character
        var sceneResult = WriteTools.AddElement(TestDataSetup.Api, "Scene", parentGuid, "Test Scene");
        Assert.IsFalse(sceneResult.StartsWith("Error:"), sceneResult);
        var sceneGuid = ExtractGuid(sceneResult);

        var charResult = WriteTools.AddElement(TestDataSetup.Api, "Character", parentGuid, "Test Char");
        Assert.IsFalse(charResult.StartsWith("Error:"), charResult);
        var charGuid = ExtractGuid(charResult);

        var result = WriteTools.LinkCast(TestDataSetup.Api, sceneGuid, charGuid);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("linked"), result);
    }

    [TestMethod]
    public void AddRelationship_WithValidGuids_ReturnsSuccess()
    {
        var parentGuid = _guids[0].ToString();

        var char1Result = WriteTools.AddElement(TestDataSetup.Api, "Character", parentGuid, "Alice");
        Assert.IsFalse(char1Result.StartsWith("Error:"), char1Result);
        var char1Guid = ExtractGuid(char1Result);

        var char2Result = WriteTools.AddElement(TestDataSetup.Api, "Character", parentGuid, "Bob");
        Assert.IsFalse(char2Result.StartsWith("Error:"), char2Result);
        var char2Guid = ExtractGuid(char2Result);

        var result = WriteTools.AddRelationship(TestDataSetup.Api, char1Guid, char2Guid, "siblings");

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("linked"), result);
        Assert.IsTrue(result.Contains("siblings"), result);
    }

    [TestMethod]
    public void ApplyBeatSheet_WithValidProblem_ReturnsSuccess()
    {
        var parentGuid = _guids[0].ToString();

        var problemResult = WriteTools.AddElement(TestDataSetup.Api, "Problem", parentGuid, "Test Problem");
        Assert.IsFalse(problemResult.StartsWith("Error:"), problemResult);
        var problemGuid = ExtractGuid(problemResult);

        // Get an actual beat sheet name from the resource tools
        var sheetNames = ResourceTools.GetBeatSheets(TestDataSetup.Api);
        Assert.IsFalse(sheetNames.StartsWith("Error:"), sheetNames);

        // Use "Three Act Play" which is a standard beat sheet
        var result = WriteTools.ApplyBeatSheet(TestDataSetup.Api, problemGuid, "Three Act Play");

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("applied"), result);
    }

    [TestMethod]
    public void MoveElement_WithValidGuids_ReturnsSuccess()
    {
        var parentGuid = _guids[0].ToString();

        // Create two folders and a scene under the first
        var folder1Result = WriteTools.AddElement(TestDataSetup.Api, "Folder", parentGuid, "Source Folder");
        Assert.IsFalse(folder1Result.StartsWith("Error:"), folder1Result);
        var folder1Guid = ExtractGuid(folder1Result);

        var folder2Result = WriteTools.AddElement(TestDataSetup.Api, "Folder", parentGuid, "Dest Folder");
        Assert.IsFalse(folder2Result.StartsWith("Error:"), folder2Result);
        var folder2Guid = ExtractGuid(folder2Result);

        var sceneResult = WriteTools.AddElement(TestDataSetup.Api, "Scene", folder1Guid, "Movable Scene");
        Assert.IsFalse(sceneResult.StartsWith("Error:"), sceneResult);
        var sceneGuid = ExtractGuid(sceneResult);

        // Move scene from folder1 to folder2
        var result = WriteTools.MoveElement(TestDataSetup.Api, sceneGuid, folder2Guid);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("moved"), result);
    }

    [TestMethod]
    public void MoveElement_WithInvalidGuid_ReturnsError()
    {
        var result = WriteTools.MoveElement(TestDataSetup.Api, "not-a-guid", _guids[0].ToString());

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void MoveElement_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = WriteTools.MoveElement(TestDataSetup.Api, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    /// <summary>
    /// Extracts the GUID value from a JSON result string like {"guid": "xxx", ...}
    /// </summary>
    private static string ExtractGuid(string json)
    {
        var start = json.IndexOf("\"guid\": \"") + 9;
        var end = json.IndexOf("\"", start);
        return json.Substring(start, end - start);
    }
}
