using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADLib.Models;
using StoryCADMcp.Tools;

namespace StoryCADMcp.Tests;

[TestClass]
public class ReadToolsTests
{
    private List<Guid> _guids = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _guids = await TestDataSetup.CreateTestOutline("Read Test Outline");
    }

    [TestMethod]
    public void ListElements_WithNoFilter_ReturnsAllElements()
    {
        var result = ReadTools.ListElements(TestDataSetup.Api);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("guid"));
        Assert.IsTrue(result.Contains("name"));
        Assert.IsTrue(result.Contains("type"));
    }

    [TestMethod]
    public void ListElements_WithTypeFilter_ReturnsFilteredElements()
    {
        var result = ReadTools.ListElements(TestDataSetup.Api, "StoryOverview");

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("StoryOverview"));
    }

    [TestMethod]
    public void ListElements_ByDefault_OmitsDescription()
    {
        var result = ReadTools.ListElements(TestDataSetup.Api);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsFalse(result.Contains("description"),
            $"Default listing should stay compact (no description field): {result}");
    }

    [TestMethod]
    public void ListElements_WithIncludeDescription_AddsDescriptionField()
    {
        var result = ReadTools.ListElements(TestDataSetup.Api, includeDescription: true);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("description"),
            $"includeDescription:true should add the description field: {result}");
    }

    [TestMethod]
    public void ListElements_WithIncludeDescription_StripsRtf()
    {
        var result = ReadTools.ListElements(TestDataSetup.Api, includeDescription: true);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsFalse(result.Contains("\\rtf"),
            $"Descriptions should be RTF-stripped, not raw control words: {result}");
    }

    [TestMethod]
    public void ListElements_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = ReadTools.ListElements(TestDataSetup.Api);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void GetElement_WithValidGuid_ReturnsElementDetail()
    {
        var guid = _guids[0].ToString();

        var result = ReadTools.GetElement(TestDataSetup.Api, guid);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.AreNotEqual("null", result);
    }

    [TestMethod]
    public void GetElement_WithInvalidGuid_ReturnsError()
    {
        var result = ReadTools.GetElement(TestDataSetup.Api, "not-a-guid");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void GetElement_WithNonexistentGuid_ReturnsError()
    {
        var result = ReadTools.GetElement(TestDataSetup.Api, Guid.NewGuid().ToString());

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void SearchText_WithMatchingText_ReturnsResults()
    {
        // The outline name "Read Test Outline" should be searchable
        var result = ReadTools.SearchText(TestDataSetup.Api, "Read Test");

        Assert.IsFalse(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void SearchText_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = ReadTools.SearchText(TestDataSetup.Api, "anything");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public void GetStructure_WithOpenOutline_ReturnsTree()
    {
        var result = ReadTools.GetStructure(TestDataSetup.Api);

        Assert.IsFalse(result.StartsWith("Error:"), result);
        Assert.IsTrue(result.Contains("guid"));
        Assert.IsTrue(result.Contains("children"));
    }

    [TestMethod]
    public void GetStructure_WithNoOutline_ReturnsError()
    {
        TestDataSetup.Api.SetCurrentModel(null!);

        var result = ReadTools.GetStructure(TestDataSetup.Api);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }
}
