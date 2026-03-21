using System.ComponentModel;
using ModelContextProtocol.Server;
using StoryCADLib.Services.API;

namespace StoryCADMcp.Tools;

[McpServerToolType]
public static class OutlineTools
{
    private static string? _openedPath;

    [McpServerTool(Name = "open_outline")]
    [Description("Opens a StoryCAD outline file (.stbx) and loads it into memory. Must be called before using any read or write tools that operate on outline elements.")]
    public static async Task<string> OpenOutline(
        StoryCADApi api,
        [Description("Absolute path to the .stbx outline file")] string path)
    {
        var result = await api.OpenOutline(path);
        if (!result.IsSuccess)
            return $"Error: {result.ErrorMessage}";

        _openedPath = path;

        var elemCount = api.GetAllElements();
        var count = elemCount.IsSuccess ? elemCount.Payload.Count : 0;
        return $"Outline opened: {path} ({count} elements)";
    }

    [McpServerTool(Name = "save_outline")]
    [Description("Saves the currently open outline to disk. Call after making changes with write tools. If no path is provided, saves to the path it was opened from.")]
    public static async Task<string> SaveOutline(
        StoryCADApi api,
        [Description("Optional: path to save to. Omit to save to the original location.")] string? path = null)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open.";

        var savePath = path ?? _openedPath;
        if (string.IsNullOrEmpty(savePath))
            return "Error: No save path available. Provide a path explicitly.";

        var result = await api.WriteOutline(savePath);
        if (!result.IsSuccess)
            return $"Error: {result.ErrorMessage}";

        return $"Outline saved to: {savePath}";
    }

    [McpServerTool(Name = "close_outline")]
    [Description("Closes the currently open outline, clearing it from memory. Call save_outline first if you have unsaved changes.")]
    public static string CloseOutline(StoryCADApi api)
    {
        if (api.CurrentModel == null)
            return "No outline is currently open.";

        api.SetCurrentModel(null!);
        _openedPath = null;
        return "Outline closed.";
    }
}
