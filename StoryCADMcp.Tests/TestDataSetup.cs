using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADLib.Models;
using StoryCADLib.Models.Tools;
using StoryCADLib.Services;
using StoryCADLib.Services.API;
using StoryCADLib.Services.Backup;
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.Outline;
using StoryCADLib.ViewModels;

namespace StoryCADMcp.Tests;

[TestClass]
public static class TestDataSetup
{
    public static StoryCADApi Api { get; private set; } = null!;
    public static string TempDir { get; private set; } = null!;

    [AssemblyInitialize]
    public static async Task AssemblyInitAsync(TestContext context)
    {
        BootStrapper.Initialise(headless: true);

        var prefs = Ioc.Default.GetRequiredService<PreferenceService>();
        prefs.Model.AutoSave = false;
        prefs.Model.TimedBackup = false;
        prefs.Model.BackupOnOpen = false;

        Api = new StoryCADApi(
            Ioc.Default.GetRequiredService<OutlineService>(),
            Ioc.Default.GetRequiredService<ListData>(),
            Ioc.Default.GetRequiredService<ControlData>(),
            Ioc.Default.GetRequiredService<ToolsData>()
        );

        TempDir = Path.Combine(Path.GetTempPath(), "StoryCADMcpTests");
        Directory.CreateDirectory(TempDir);
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        try
        {
            var autoSaveService = Ioc.Default.GetService<AutoSaveService>();
            autoSaveService?.StopAutoSave();
            autoSaveService?.Dispose();

            var backupService = Ioc.Default.GetService<BackupService>();
            backupService?.StopTimedBackup();
        }
        catch { }

        try
        {
            if (Directory.Exists(TempDir))
                Directory.Delete(TempDir, true);
        }
        catch { }
    }

    /// <summary>
    /// Creates a fresh empty outline and sets it as CurrentModel.
    /// Returns the GUIDs of the created elements (overview, problem, character, etc.)
    /// </summary>
    public static async Task<List<Guid>> CreateTestOutline(string name = "Test Outline")
    {
        var result = await Api.CreateEmptyOutline(name, "Test Author", "0");
        Assert.IsTrue(result.IsSuccess, $"CreateEmptyOutline failed: {result.ErrorMessage}");
        return result.Payload;
    }

    /// <summary>
    /// Saves the current model to a temp file and returns the path.
    /// </summary>
    public static async Task<string> SaveToTemp(string fileName = "test.stbx")
    {
        var path = Path.Combine(TempDir, fileName);
        var result = await Api.WriteOutline(path);
        Assert.IsTrue(result.IsSuccess, $"WriteOutline failed: {result.ErrorMessage}");
        return path;
    }
}
