using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using StoryCADLib.Models;
using StoryCADLib.Services.API;
using StoryCADLib.ViewModels;

namespace StoryCADMcp.Tools;

[McpServerToolType]
public static class ReadTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpServerTool(Name = "list_elements")]
    [Description("Lists all story elements in the open outline. Optionally filter by type: Problem, Character, Setting, Scene, Folder, Section, Web, Notes, StoryOverview, StoryWorld.")]
    public static string ListElements(
        StoryCADApi api,
        [Description("Optional element type filter (e.g. 'Character', 'Scene'). Omit to list all.")] string? type = null)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (type != null && Enum.TryParse<StoryItemType>(type, true, out var typeFilter))
        {
            var result = api.GetElementsByType(typeFilter);
            if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

            var elements = result.Payload.Select(e => new
            {
                guid = e.Uuid.ToString(),
                name = e.Name,
                type = e.ElementType.ToString()
            });
            return JsonSerializer.Serialize(elements, JsonOptions);
        }

        var allResult = api.GetAllElements();
        if (!allResult.IsSuccess) return $"Error: {allResult.ErrorMessage}";

        var allElements = allResult.Payload.Select(e => new
        {
            guid = e.Uuid.ToString(),
            name = e.Name,
            type = e.ElementType.ToString()
        });
        return JsonSerializer.Serialize(allElements, JsonOptions);
    }

    [McpServerTool(Name = "get_element")]
    [Description("Gets full detail for a single story element by its GUID, including all properties and fields.")]
    public static string GetElement(
        StoryCADApi api,
        [Description("The GUID of the element to retrieve")] string guid)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(guid, out var parsedGuid))
            return $"Error: Invalid GUID: {guid}";

        var result = api.GetElement(parsedGuid);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        // GetElement returns serialized JSON as its payload
        return result.Payload?.ToString() ?? "null";
    }

    [McpServerTool(Name = "search_text")]
    [Description("Searches all text fields in the open outline for the given text. Returns matching elements and the fields that contain the text.")]
    public static string SearchText(
        StoryCADApi api,
        [Description("The text to search for across all element fields")] string text)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        var result = api.SearchForText(text);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        return JsonSerializer.Serialize(result.Payload, JsonOptions);
    }

    [McpServerTool(Name = "get_structure")]
    [Description("Returns the explorer tree hierarchy of the open outline, showing parent-child relationships between all elements.")]
    public static string GetStructure(StoryCADApi api)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        var tree = BuildTree(api.CurrentModel.ExplorerView);
        return JsonSerializer.Serialize(tree, JsonOptions);
    }

    [McpServerTool(Name = "get_problem_structure")]
    [Description("Returns the beat structure for a Problem element, showing its title, description, and all beats with their linked elements.")]
    public static string GetProblemStructure(
        StoryCADApi api,
        [Description("The GUID of the Problem element")] string guid)
    {
        if (api.CurrentModel == null)
            return "Error: No outline is currently open. Call open_outline first.";

        if (!Guid.TryParse(guid, out var parsedGuid))
            return $"Error: Invalid GUID: {guid}";

        var result = api.GetProblemStructure(parsedGuid);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";

        var (title, description, beats) = result.Payload;
        var output = new
        {
            title,
            description,
            beats = beats.Select((b, i) => new
            {
                index = i,
                beatTitle = b.BeatTitle,
                beatDescription = b.BeatDescription,
                linkedElement = b.LinkedElement?.ToString()
            })
        };
        return JsonSerializer.Serialize(output, JsonOptions);
    }

    private static object BuildTree(ObservableCollection<StoryNodeItem> nodes)
    {
        return nodes.Select(node => new
        {
            guid = node.Uuid.ToString(),
            name = node.Name,
            type = node.Type.ToString(),
            children = node.Children.Count > 0
                ? BuildTree(node.Children)
                : null
        });
    }
}
