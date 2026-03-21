using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADMcp.Tools;

namespace StoryCADMcp.Tests;

[TestClass]
public class OutlineToolsTests
{
    [TestInitialize]
    public void Setup()
    {
        // Clear any previous state
        OutlineTools.CloseOutline(TestDataSetup.Api);
    }

    [TestMethod]
    public async Task OpenOutline_WithValidFile_ReturnsSuccess()
    {
        await TestDataSetup.CreateTestOutline();
        var path = await TestDataSetup.SaveToTemp("open_test.stbx");
        OutlineTools.CloseOutline(TestDataSetup.Api);

        var result = await OutlineTools.OpenOutline(TestDataSetup.Api, path);

        Assert.IsTrue(result.Contains("Outline opened"), result);
        Assert.IsTrue(result.Contains("open_test.stbx"), result);
        Assert.IsNotNull(TestDataSetup.Api.CurrentModel);
    }

    [TestMethod]
    public async Task OpenOutline_WithInvalidPath_ReturnsError()
    {
        var result = await OutlineTools.OpenOutline(TestDataSetup.Api, "/nonexistent/path.stbx");

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public async Task SaveOutline_WithOpenOutline_SavesSuccessfully()
    {
        await TestDataSetup.CreateTestOutline();
        var path = await TestDataSetup.SaveToTemp("save_test.stbx");
        OutlineTools.CloseOutline(TestDataSetup.Api);

        await OutlineTools.OpenOutline(TestDataSetup.Api, path);
        var result = await OutlineTools.SaveOutline(TestDataSetup.Api);

        Assert.IsTrue(result.Contains("Outline saved"), result);
    }

    [TestMethod]
    public async Task SaveOutline_WithExplicitPath_SavesToNewLocation()
    {
        await TestDataSetup.CreateTestOutline();
        var originalPath = await TestDataSetup.SaveToTemp("save_original.stbx");
        OutlineTools.CloseOutline(TestDataSetup.Api);

        await OutlineTools.OpenOutline(TestDataSetup.Api, originalPath);
        var newPath = Path.Combine(TestDataSetup.TempDir, "save_copy.stbx");
        var result = await OutlineTools.SaveOutline(TestDataSetup.Api, newPath);

        Assert.IsTrue(result.Contains("save_copy.stbx"), result);
        Assert.IsTrue(File.Exists(newPath));
    }

    [TestMethod]
    public async Task SaveOutline_WithNoOutlineOpen_ReturnsError()
    {
        var result = await OutlineTools.SaveOutline(TestDataSetup.Api);

        Assert.IsTrue(result.StartsWith("Error:"), result);
    }

    [TestMethod]
    public async Task CloseOutline_WithOpenOutline_ClearsModel()
    {
        await TestDataSetup.CreateTestOutline();

        var result = OutlineTools.CloseOutline(TestDataSetup.Api);

        Assert.AreEqual("Outline closed.", result);
        Assert.IsNull(TestDataSetup.Api.CurrentModel);
    }

    [TestMethod]
    public void CloseOutline_WithNoOutlineOpen_ReturnsMessage()
    {
        var result = OutlineTools.CloseOutline(TestDataSetup.Api);

        Assert.AreEqual("No outline is currently open.", result);
    }

    [TestMethod]
    public async Task OpenSaveClose_RoundTrip_PreservesData()
    {
        // Create and save an outline
        var guids = await TestDataSetup.CreateTestOutline("Round Trip Test");
        var path = await TestDataSetup.SaveToTemp("roundtrip.stbx");
        OutlineTools.CloseOutline(TestDataSetup.Api);

        // Reopen and verify
        await OutlineTools.OpenOutline(TestDataSetup.Api, path);
        Assert.IsNotNull(TestDataSetup.Api.CurrentModel);

        var elements = TestDataSetup.Api.GetAllElements();
        Assert.IsTrue(elements.IsSuccess);
        Assert.IsTrue(elements.Payload.Count > 0);
    }
}
