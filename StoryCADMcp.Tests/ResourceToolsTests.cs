using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADMcp.Tools;

namespace StoryCADMcp.Tests;

/// <summary>
/// Resource tools don't require an open outline — they query static reference data.
/// </summary>
[TestClass]
public class ResourceToolsTests
{
    [TestMethod]
    public void GetKeyQuestions_WithNoType_ListsElementTypes()
    {
        var result = ResourceTools.GetKeyQuestions(TestDataSetup.Api);

        Assert.IsFalse(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void GetKeyQuestions_WithCharacterType_ReturnsQuestions()
    {
        var result = ResourceTools.GetKeyQuestions(TestDataSetup.Api, "Character");

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("topic") || result.Contains("question"), result);
    }

    [TestMethod]
    public void GetMasterPlots_WithNoName_ListsPlotNames()
    {
        var result = ResourceTools.GetMasterPlots(TestDataSetup.Api);

        Assert.IsFalse(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void GetMasterPlots_WithValidName_ReturnsPlotDetail()
    {
        // Get actual list of names from the API
        var listResult = ResourceTools.GetMasterPlots(TestDataSetup.Api);
        Assert.IsFalse(listResult.StartsWith("Error:"), listResult);
        var names = JsonSerializer.Deserialize<List<string>>(listResult);
        Assert.IsTrue(names != null && names.Count > 0, "No master plot names returned");

        var result = ResourceTools.GetMasterPlots(TestDataSetup.Api, names[0]);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("notes") || result.Contains("scenes"), result);
    }

    [TestMethod]
    public void GetBeatSheets_WithNoName_ListsSheetNames()
    {
        var result = ResourceTools.GetBeatSheets(TestDataSetup.Api);

        Assert.IsFalse(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void GetBeatSheets_WithValidName_ReturnsBeats()
    {
        var result = ResourceTools.GetBeatSheets(TestDataSetup.Api, "Three Act Play");

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("beats"), result);
    }

    [TestMethod]
    public void GetConflictCategories_WithNoArgs_ListsCategories()
    {
        var result = ResourceTools.GetConflictCategories(TestDataSetup.Api);

        Assert.IsFalse(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void GetConflictCategories_WithCategory_ListsSubcategories()
    {
        // Get actual list of categories from the API
        var listResult = ResourceTools.GetConflictCategories(TestDataSetup.Api);
        Assert.IsFalse(listResult.StartsWith("Error:"), listResult);
        var categories = JsonSerializer.Deserialize<List<string>>(listResult);
        Assert.IsTrue(categories != null && categories.Count > 0, "No conflict categories returned");

        var result = ResourceTools.GetConflictCategories(TestDataSetup.Api, categories[0]);

        Assert.IsFalse(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void GetStockScenes_WithNoCategory_ListsCategories()
    {
        var result = ResourceTools.GetStockScenes(TestDataSetup.Api);

        Assert.IsFalse(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void GetExamples_WithValidProperty_ReturnsValues()
    {
        var result = ResourceTools.GetExamples(TestDataSetup.Api, "Role");

        Assert.IsFalse(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void GetExamples_WithInvalidProperty_ReturnsError()
    {
        var result = ResourceTools.GetExamples(TestDataSetup.Api, "NonexistentProperty");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }
}
