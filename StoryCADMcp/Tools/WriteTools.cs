using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using StoryCADLib.Models;
using StoryCADLib.Services.API;

namespace StoryCADMcp.Tools;

[McpServerToolType]
public static class WriteTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpServerTool(Name = "add_element")]
    [Description("Adds a new story element as a child of the specified parent. Valid types: Problem, Character, Setting, Scene, Folder, Section, Web, Notes. Call save_outline after to persist.")]
    public static string AddElement(
        StoryCADApi api,
        [Description("Element type: Problem, Character, Setting, Scene, Folder, Section, Web, or Notes")] string type,
        [Description("GUID of the parent element under which to add this element")] string parentGuid,
        [Description("Name for the new element")] string name)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Enum.TryParse<StoryItemType>(type, true, out var elementType))
            return $"Error: Invalid element type: {type}. Valid: Problem, Character, Setting, Scene, Folder, Section, Web, Notes";

        var result = api.AddElement(elementType, parentGuid, name);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            guid = result.Payload.ToString(),
            name,
            type = elementType.ToString()
        }, JsonOptions);
    }

    [McpServerTool(Name = "update_property")]
    [Description("Updates a single property on a story element. Call save_outline after to persist.")]
    public static string UpdateProperty(
        StoryCADApi api,
        [Description("GUID of the element to update")] string guid,
        [Description("Property name to update (e.g. Name, Description, Role, Archetype)")] string property,
        [Description("New value for the property")] string value)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(guid, out var parsedGuid))
            return $"Error: Invalid GUID: {guid}";

        var result = api.UpdateElementProperty(parsedGuid, property, value);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            guid,
            property,
            updated = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "delete_element")]
    [Description("Moves a story element to the trash. Call save_outline after to persist.")]
    public static async Task<string> DeleteElement(
        StoryCADApi api,
        [Description("GUID of the element to delete")] string guid)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(guid, out var parsedGuid))
            return $"Error: Invalid GUID: {guid}";

        var result = await api.DeleteElement(parsedGuid);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new { guid, deleted = true }, JsonOptions);
    }

    [McpServerTool(Name = "link_cast")]
    [Description("Adds a character to a scene's cast list. Call save_outline after to persist.")]
    public static string LinkCast(
        StoryCADApi api,
        [Description("GUID of the scene")] string sceneGuid,
        [Description("GUID of the character to add to the scene")] string characterGuid)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(sceneGuid, out var scene))
            return $"Error: Invalid scene GUID: {sceneGuid}";

        if (!Guid.TryParse(characterGuid, out var character))
            return $"Error: Invalid character GUID: {characterGuid}";

        var result = api.AddCastMember(scene, character);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            scene = sceneGuid,
            character = characterGuid,
            linked = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "add_relationship")]
    [Description("Creates a relationship between two characters (bidirectional). Call save_outline after to persist.")]
    public static string AddRelationship(
        StoryCADApi api,
        [Description("GUID of the first character")] string sourceGuid,
        [Description("GUID of the second character")] string targetGuid,
        [Description("Description of the relationship (e.g. 'sister', 'rival')")] string description)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(sourceGuid, out var source))
            return $"Error: Invalid source GUID: {sourceGuid}";

        if (!Guid.TryParse(targetGuid, out var target))
            return $"Error: Invalid target GUID: {targetGuid}";

        var result = api.AddRelationship(source, target, description, mirror: true);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            source = sourceGuid,
            target = targetGuid,
            description,
            linked = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "apply_beat_sheet")]
    [Description("Applies a named beat sheet template to a Problem element, creating its narrative structure. Call save_outline after to persist.")]
    public static string ApplyBeatSheet(
        StoryCADApi api,
        [Description("GUID of the Problem element")] string problemGuid,
        [Description("Name of the beat sheet to apply (use get_beat_sheets to see available names)")] string beatSheetName)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(problemGuid, out var problem))
            return $"Error: Invalid GUID: {problemGuid}";

        var result = api.ApplyBeatSheetToProblem(problem, beatSheetName);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            problem = problemGuid,
            beatSheet = beatSheetName,
            applied = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "add_collection_entry")]
    [Description("Appends an entry to a list-typed property on a story element (e.g. PhysicalWorlds, Cultures, Species, Governments, Religions on a StoryWorld). The entry is supplied as a JSON object string and is converted to the collection's element type by the API. Returns the index of the newly added entry. Call save_outline after to persist.")]
    public static string AddCollectionEntry(
        StoryCADApi api,
        [Description("GUID of the element that owns the collection")] string elementGuid,
        [Description("Name of the list-typed property (e.g. PhysicalWorlds, Cultures)")] string propertyName,
        [Description("JSON object representing the entry (e.g. {\"Name\":\"Earth\",\"Geography\":\"Varied\"})")] string entryJson)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(elementGuid, out var parsedGuid))
            return $"Error: Invalid element GUID: {elementGuid}";

        JsonElement entry;
        try
        {
            entry = JsonSerializer.Deserialize<JsonElement>(entryJson);
        }
        catch (JsonException ex)
        {
            return $"Error: Invalid JSON for entry: {ex.Message}";
        }

        var result = api.AddCollectionEntry(parsedGuid, propertyName, entry);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            elementGuid,
            propertyName,
            index = result.Payload
        }, JsonOptions);
    }

    [McpServerTool(Name = "update_collection_entry")]
    [Description("Replaces an existing entry at the given index in a list-typed property on a story element. The entry is supplied as a JSON object string and is converted to the collection's element type by the API. Call save_outline after to persist.")]
    public static string UpdateCollectionEntry(
        StoryCADApi api,
        [Description("GUID of the element that owns the collection")] string elementGuid,
        [Description("Name of the list-typed property (e.g. PhysicalWorlds, Cultures)")] string propertyName,
        [Description("Zero-based index of the entry to replace")] int index,
        [Description("JSON object representing the replacement entry")] string entryJson)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(elementGuid, out var parsedGuid))
            return $"Error: Invalid element GUID: {elementGuid}";

        JsonElement entry;
        try
        {
            entry = JsonSerializer.Deserialize<JsonElement>(entryJson);
        }
        catch (JsonException ex)
        {
            return $"Error: Invalid JSON for entry: {ex.Message}";
        }

        var result = api.UpdateCollectionEntry(parsedGuid, propertyName, index, entry);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            elementGuid,
            propertyName,
            index,
            updated = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "remove_collection_entry")]
    [Description("Removes the entry at the given index from a list-typed property on a story element. Call save_outline after to persist.")]
    public static string RemoveCollectionEntry(
        StoryCADApi api,
        [Description("GUID of the element that owns the collection")] string elementGuid,
        [Description("Name of the list-typed property (e.g. PhysicalWorlds, Cultures)")] string propertyName,
        [Description("Zero-based index of the entry to remove")] int index)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(elementGuid, out var parsedGuid))
            return $"Error: Invalid element GUID: {elementGuid}";

        var result = api.RemoveCollectionEntry(parsedGuid, propertyName, index);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            elementGuid,
            propertyName,
            index,
            removed = true
        }, JsonOptions);
    }

    [McpServerTool(Name = "move_element")]
    [Description("Moves a story element to a new parent in the outline tree. Call save_outline after to persist.")]
    public static string MoveElement(
        StoryCADApi api,
        [Description("GUID of the element to move")] string guid,
        [Description("GUID of the new parent element")] string newParentGuid,
        [Description("Optional 0-based position in the new parent's children; omit to append.")] int? index = null)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(guid, out var elementGuid))
            return $"Error: Invalid element GUID: {guid}";

        if (!Guid.TryParse(newParentGuid, out var parentGuid))
            return $"Error: Invalid parent GUID: {newParentGuid}";

        var result = api.MoveElement(elementGuid, parentGuid, index);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            guid,
            newParent = newParentGuid,
            index,
            moved = true
        }, JsonOptions);
    }
}
