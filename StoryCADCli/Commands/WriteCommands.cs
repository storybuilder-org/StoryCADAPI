using StoryCADLib.Models;
using StoryCADLib.Services.API;

namespace StoryCADCli.Commands;

public static class WriteCommands
{
    public static async Task<int> Add(StoryCADApi api, string outlinePath, string[] args)
    {
        if (args.Length < 3)
        {
            Program.WriteError("Usage: add <type> <parent-guid> <name>");
            return 1;
        }

        if (!Enum.TryParse<StoryItemType>(args[0], true, out var elementType))
        {
            Program.WriteError($"Invalid element type: {args[0]}. Valid: Problem, Character, Setting, Scene, Folder, Section, Web, Notes");
            return 1;
        }

        var parentGuid = args[1];
        var name = args[2];

        var result = api.AddElement(elementType, parentGuid, name);
        if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }

        var saveResult = await api.WriteOutline(outlinePath);
        if (!saveResult.IsSuccess) { Program.WriteError($"Element added but save failed: {saveResult.ErrorMessage}"); return 1; }

        Program.WriteJson(new { guid = result.Payload.ToString(), name, type = elementType.ToString() });
        return 0;
    }

    public static async Task<int> Update(StoryCADApi api, string outlinePath, string[] args)
    {
        if (args.Length < 3)
        {
            Program.WriteError("Usage: update <guid> <property> <value>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var guid))
        {
            Program.WriteError($"Invalid GUID: {args[0]}");
            return 1;
        }

        var propertyName = args[1];
        var value = string.Join(" ", args.Skip(2));

        var result = api.UpdateElementProperty(guid, propertyName, value);
        if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }

        var saveResult = await api.WriteOutline(outlinePath);
        if (!saveResult.IsSuccess) { Program.WriteError($"Property updated but save failed: {saveResult.ErrorMessage}"); return 1; }

        Program.WriteJson(new { guid = guid.ToString(), property = propertyName, updated = true });
        return 0;
    }

    public static async Task<int> Delete(StoryCADApi api, string outlinePath, string[] args)
    {
        if (args.Length < 1)
        {
            Program.WriteError("Usage: delete <guid>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var guid))
        {
            Program.WriteError($"Invalid GUID: {args[0]}");
            return 1;
        }

        var result = await api.DeleteElement(guid);
        if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }

        var saveResult = await api.WriteOutline(outlinePath);
        if (!saveResult.IsSuccess) { Program.WriteError($"Element deleted but save failed: {saveResult.ErrorMessage}"); return 1; }

        Program.WriteJson(new { guid = guid.ToString(), deleted = true });
        return 0;
    }

    public static async Task<int> LinkCast(StoryCADApi api, string outlinePath, string[] args)
    {
        if (args.Length < 2)
        {
            Program.WriteError("Usage: link-cast <scene-guid> <character-guid>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var sceneGuid))
        {
            Program.WriteError($"Invalid scene GUID: {args[0]}");
            return 1;
        }

        if (!Guid.TryParse(args[1], out var charGuid))
        {
            Program.WriteError($"Invalid character GUID: {args[1]}");
            return 1;
        }

        var result = api.AddCastMember(sceneGuid, charGuid);
        if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }

        var saveResult = await api.WriteOutline(outlinePath);
        if (!saveResult.IsSuccess) { Program.WriteError($"Cast linked but save failed: {saveResult.ErrorMessage}"); return 1; }

        Program.WriteJson(new { scene = sceneGuid.ToString(), character = charGuid.ToString(), linked = true });
        return 0;
    }

    public static async Task<int> AddRelationship(StoryCADApi api, string outlinePath, string[] args)
    {
        if (args.Length < 3)
        {
            Program.WriteError("Usage: add-relationship <source-guid> <target-guid> <description>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var sourceGuid))
        {
            Program.WriteError($"Invalid source GUID: {args[0]}");
            return 1;
        }

        if (!Guid.TryParse(args[1], out var targetGuid))
        {
            Program.WriteError($"Invalid target GUID: {args[1]}");
            return 1;
        }

        var description = string.Join(" ", args.Skip(2));

        var result = api.AddRelationship(sourceGuid, targetGuid, description, mirror: true);
        if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }

        var saveResult = await api.WriteOutline(outlinePath);
        if (!saveResult.IsSuccess) { Program.WriteError($"Relationship added but save failed: {saveResult.ErrorMessage}"); return 1; }

        Program.WriteJson(new { source = sourceGuid.ToString(), target = targetGuid.ToString(), description, linked = true });
        return 0;
    }

    public static async Task<int> ApplyBeats(StoryCADApi api, string outlinePath, string[] args)
    {
        if (args.Length < 2)
        {
            Program.WriteError("Usage: apply-beats <problem-guid> <beat-sheet-name>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var problemGuid))
        {
            Program.WriteError($"Invalid GUID: {args[0]}");
            return 1;
        }

        var beatSheetName = string.Join(" ", args.Skip(1));

        var result = api.ApplyBeatSheetToProblem(problemGuid, beatSheetName);
        if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }

        var saveResult = await api.WriteOutline(outlinePath);
        if (!saveResult.IsSuccess) { Program.WriteError($"Beats applied but save failed: {saveResult.ErrorMessage}"); return 1; }

        Program.WriteJson(new { problem = problemGuid.ToString(), beatSheet = beatSheetName, applied = true });
        return 0;
    }
}
