using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using StoryCADLib.Services.API;

namespace StoryCADMcp.Tools;

[McpServerToolType]
public static class BeatTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpServerTool(Name = "assign_beat")]
    [Description("Assigns a Scene or Problem element to a beat in a Problem's structure. Call save_outline after to persist.")]
    public static string AssignBeat(
        StoryCADApi api,
        [Description("GUID of the Problem element that owns the beats")] string problemGuid,
        [Description("0-based index of the beat to assign to")] int beatIndex,
        [Description("GUID of the Scene or Problem element to assign")] string elementGuid)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(problemGuid, out var problem))
            return $"Error: Invalid problem GUID: {problemGuid}";

        if (!Guid.TryParse(elementGuid, out var element))
            return $"Error: Invalid element GUID: {elementGuid}";

        var result = api.AssignElementToBeat(problem, beatIndex, element);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            problem = problemGuid,
            beatIndex,
            element = elementGuid,
            assigned = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "clear_beat")]
    [Description("Clears the element assignment from a beat, leaving the beat but removing its linked Scene/Problem. Call save_outline after to persist.")]
    public static string ClearBeat(
        StoryCADApi api,
        [Description("GUID of the Problem element that owns the beats")] string problemGuid,
        [Description("0-based index of the beat to clear")] int beatIndex)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(problemGuid, out var problem))
            return $"Error: Invalid problem GUID: {problemGuid}";

        var result = api.ClearBeatAssignment(problem, beatIndex);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            problem = problemGuid,
            beatIndex,
            cleared = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "create_beat")]
    [Description("Creates a new beat at the end of a Problem's structure. Call save_outline after to persist.")]
    public static string CreateBeat(
        StoryCADApi api,
        [Description("GUID of the Problem element")] string problemGuid,
        [Description("Title for the new beat")] string title,
        [Description("Description/notes for the new beat")] string description)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(problemGuid, out var problem))
            return $"Error: Invalid problem GUID: {problemGuid}";

        var result = api.CreateBeat(problem, title, description);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            problem = problemGuid,
            title,
            created = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "update_beat")]
    [Description("Updates a beat's title and description. Preserves any existing element assignment. Call save_outline after to persist.")]
    public static string UpdateBeat(
        StoryCADApi api,
        [Description("GUID of the Problem element that owns the beats")] string problemGuid,
        [Description("0-based index of the beat to update")] int beatIndex,
        [Description("New title for the beat")] string title,
        [Description("New description for the beat")] string description)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(problemGuid, out var problem))
            return $"Error: Invalid problem GUID: {problemGuid}";

        var result = api.UpdateBeat(problem, beatIndex, title, description);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            problem = problemGuid,
            beatIndex,
            title,
            updated = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "delete_beat")]
    [Description("Deletes a beat from a Problem's structure. Remaining beats shift down. Call save_outline after to persist.")]
    public static string DeleteBeat(
        StoryCADApi api,
        [Description("GUID of the Problem element that owns the beats")] string problemGuid,
        [Description("0-based index of the beat to delete")] int beatIndex)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(problemGuid, out var problem))
            return $"Error: Invalid problem GUID: {problemGuid}";

        var result = api.DeleteBeat(problem, beatIndex);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            problem = problemGuid,
            beatIndex,
            deleted = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "move_beat")]
    [Description("Moves a beat from one position to another within a Problem's structure. Call save_outline after to persist.")]
    public static string MoveBeat(
        StoryCADApi api,
        [Description("GUID of the Problem element that owns the beats")] string problemGuid,
        [Description("0-based index of the beat to move")] int fromIndex,
        [Description("0-based index to move the beat to")] int toIndex)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(problemGuid, out var problem))
            return $"Error: Invalid problem GUID: {problemGuid}";

        var result = api.MoveBeat(problem, fromIndex, toIndex);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            problem = problemGuid,
            fromIndex,
            toIndex,
            moved = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "save_beat_sheet")]
    [Description("Exports a Problem's beat structure to a .stbeat file. The file can be loaded into other Problems.")]
    public static string SaveBeatSheet(
        StoryCADApi api,
        [Description("GUID of the Problem element whose beats to export")] string problemGuid,
        [Description("Absolute path for the .stbeat file to create")] string filePath)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(problemGuid, out var problem))
            return $"Error: Invalid problem GUID: {problemGuid}";

        var result = api.SaveBeatSheet(problem, filePath);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            problem = problemGuid,
            filePath,
            saved = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "load_beat_sheet")]
    [Description("Loads a beat structure from a .stbeat file into a Problem, replacing its current beats. Call save_outline after to persist.")]
    public static string LoadBeatSheet(
        StoryCADApi api,
        [Description("GUID of the Problem element to load beats into")] string problemGuid,
        [Description("Absolute path to the .stbeat file to load")] string filePath)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(problemGuid, out var problem))
            return $"Error: Invalid problem GUID: {problemGuid}";

        var result = api.LoadBeatSheet(problem, filePath);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            problem = problemGuid,
            filePath,
            loaded = true
        }, JsonOptions);
    }
}
