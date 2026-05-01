using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADLib.Models;
using StoryCADMcp.Tools;

namespace StoryCADMcp.Tests;

[TestClass]
public class CollectionEntryToolsTests
{
    private List<Guid> _guids = null!;
    private string _storyWorldGuid = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _guids = await TestDataSetup.CreateTestOutline("Collection Entry Test Outline");
        _storyWorldGuid = EnsureStoryWorld(_guids[0].ToString());
    }

    /// <summary>
    /// Returns the GUID of the StoryWorld element for the current outline.
    /// If no StoryWorld exists yet, one is created under the supplied parent.
    /// </summary>
    private static string EnsureStoryWorld(string parentGuid)
    {
        var existing = TestDataSetup.Api.GetStoryWorld();
        if (existing.IsSuccess && existing.Payload != null)
            return existing.Payload.Uuid.ToString();

        var addResult = WriteTools.AddElement(TestDataSetup.Api, "StoryWorld", parentGuid, "Test World");
        Assert.IsFalse(addResult.StartsWith("Error:"), addResult);
        return ExtractGuid(addResult);
    }

    // ---------- AddCollectionEntry ----------

    [TestMethod]
    public void AddCollectionEntry_WithValidEntry_ReturnsIndex()
    {
        var entryJson = "{\"Name\":\"Earth\",\"Geography\":\"Varied\"}";

        var result = WriteTools.AddCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "PhysicalWorlds", entryJson);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("index"), result);
        Assert.IsTrue(result.Contains("PhysicalWorlds"), result);
    }

    [TestMethod]
    public void AddCollectionEntry_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = WriteTools.AddCollectionEntry(
            TestDataSetup.Api, Guid.NewGuid().ToString(), "PhysicalWorlds", "{}");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void AddCollectionEntry_WithInvalidGuid_ReturnsError()
    {
        var result = WriteTools.AddCollectionEntry(
            TestDataSetup.Api, "not-a-guid", "PhysicalWorlds", "{}");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void AddCollectionEntry_WithInvalidJson_ReturnsError()
    {
        var result = WriteTools.AddCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "PhysicalWorlds", "{not-json");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void AddCollectionEntry_WithBadPropertyName_ReturnsError()
    {
        var result = WriteTools.AddCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "NotARealProperty", "{}");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    // ---------- UpdateCollectionEntry ----------

    [TestMethod]
    public void UpdateCollectionEntry_WithValidEntry_ReturnsSuccess()
    {
        // Seed a single entry first.
        var addResult = WriteTools.AddCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "PhysicalWorlds",
            "{\"Name\":\"Original\"}");
        Assert.IsFalse(addResult.StartsWith("Error:"), addResult);

        var result = WriteTools.UpdateCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "PhysicalWorlds", 0,
            "{\"Name\":\"Updated\",\"Geography\":\"Plains\"}");

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("updated"), result);
    }

    [TestMethod]
    public void UpdateCollectionEntry_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = WriteTools.UpdateCollectionEntry(
            TestDataSetup.Api, Guid.NewGuid().ToString(), "PhysicalWorlds", 0, "{}");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void UpdateCollectionEntry_WithInvalidGuid_ReturnsError()
    {
        var result = WriteTools.UpdateCollectionEntry(
            TestDataSetup.Api, "not-a-guid", "PhysicalWorlds", 0, "{}");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void UpdateCollectionEntry_WithInvalidJson_ReturnsError()
    {
        var result = WriteTools.UpdateCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "PhysicalWorlds", 0, "{not-json");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void UpdateCollectionEntry_WithBadPropertyName_ReturnsError()
    {
        var result = WriteTools.UpdateCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "NotARealProperty", 0, "{}");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void UpdateCollectionEntry_WithOutOfRangeIndex_ReturnsError()
    {
        // Seed one entry; indices 0 valid, 5 not.
        WriteTools.AddCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "PhysicalWorlds", "{\"Name\":\"X\"}");

        var result = WriteTools.UpdateCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "PhysicalWorlds", 5,
            "{\"Name\":\"Y\"}");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    // ---------- RemoveCollectionEntry ----------

    [TestMethod]
    public void RemoveCollectionEntry_WithValidIndex_ReturnsSuccess()
    {
        // Seed an entry to remove.
        WriteTools.AddCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "PhysicalWorlds", "{\"Name\":\"Doomed\"}");

        var result = WriteTools.RemoveCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "PhysicalWorlds", 0);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("removed"), result);
    }

    [TestMethod]
    public void RemoveCollectionEntry_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = WriteTools.RemoveCollectionEntry(
            TestDataSetup.Api, Guid.NewGuid().ToString(), "PhysicalWorlds", 0);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void RemoveCollectionEntry_WithInvalidGuid_ReturnsError()
    {
        var result = WriteTools.RemoveCollectionEntry(
            TestDataSetup.Api, "not-a-guid", "PhysicalWorlds", 0);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void RemoveCollectionEntry_WithBadPropertyName_ReturnsError()
    {
        var result = WriteTools.RemoveCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "NotARealProperty", 0);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void RemoveCollectionEntry_WithOutOfRangeIndex_ReturnsError()
    {
        // Empty list — any index is out of range.
        var result = WriteTools.RemoveCollectionEntry(
            TestDataSetup.Api, _storyWorldGuid, "PhysicalWorlds", 99);

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
