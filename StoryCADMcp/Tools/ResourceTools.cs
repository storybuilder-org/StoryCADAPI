using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using StoryCADLib.Services.API;

namespace StoryCADMcp.Tools;

[McpServerToolType]
public static class ResourceTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpServerTool(Name = "get_key_questions")]
    [Description("Gets key questions for developing story elements. Without a type, lists element types that have questions. With a type (e.g. 'Character', 'Problem'), returns the questions for that type.")]
    public static string GetKeyQuestions(
        StoryCADApi api,
        [Description("Optional element type (e.g. 'Character', 'Problem'). Omit to list available types.")] string? elementType = null)
    {
        if (elementType == null)
        {
            var result = api.GetKeyQuestionElements();
            if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";
            return JsonSerializer.Serialize(result.Payload, JsonOptions);
        }

        var questionsResult = api.GetKeyQuestions(elementType);
        if (!questionsResult.IsSuccess) return $"Error: {questionsResult.ErrorMessage}";

        var questions = questionsResult.Payload.Select(q => new { topic = q.Topic, question = q.Question });
        return JsonSerializer.Serialize(questions, JsonOptions);
    }

    [McpServerTool(Name = "get_master_plots")]
    [Description("Gets master plot patterns. Without a name, lists all available plot names. With a name, returns the plot's notes and scene breakdown.")]
    public static string GetMasterPlots(
        StoryCADApi api,
        [Description("Optional plot name. Omit to list all available master plots.")] string? plotName = null)
    {
        if (plotName == null)
        {
            var result = api.GetMasterPlotNames();
            if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";
            return JsonSerializer.Serialize(result.Payload, JsonOptions);
        }

        var notesResult = api.GetMasterPlotNotes(plotName);
        if (!notesResult.IsSuccess) return $"Error: {notesResult.ErrorMessage}";

        var scenesResult = api.GetMasterPlotScenes(plotName);
        if (!scenesResult.IsSuccess) return $"Error: {scenesResult.ErrorMessage}";

        var output = new
        {
            name = plotName,
            notes = notesResult.Payload,
            scenes = scenesResult.Payload.Select(s => new { title = s.SceneTitle, notes = s.Notes })
        };
        return JsonSerializer.Serialize(output, JsonOptions);
    }

    [McpServerTool(Name = "get_beat_sheets")]
    [Description("Gets beat sheet templates for structuring narrative. Without a name, lists all available sheet names. With a name, returns the sheet's description and beats.")]
    public static string GetBeatSheets(
        StoryCADApi api,
        [Description("Optional beat sheet name. Omit to list all available beat sheets.")] string? sheetName = null)
    {
        if (sheetName == null)
        {
            var result = api.GetBeatSheetNames();
            if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";
            return JsonSerializer.Serialize(result.Payload, JsonOptions);
        }

        var sheetResult = api.GetBeatSheet(sheetName);
        if (!sheetResult.IsSuccess) return $"Error: {sheetResult.ErrorMessage}";

        var (description, beats) = sheetResult.Payload;
        var output = new
        {
            name = sheetName,
            description,
            beats = beats.Select(b => new { title = b.BeatName, notes = b.BeatNotes })
        };
        return JsonSerializer.Serialize(output, JsonOptions);
    }

    [McpServerTool(Name = "get_conflict_categories")]
    [Description("Gets conflict types for story problems. No args = list categories. With category = list subcategories. With both = get conflict examples.")]
    public static string GetConflictCategories(
        StoryCADApi api,
        [Description("Optional conflict category (e.g. 'Person vs. Person')")] string? category = null,
        [Description("Optional subcategory within the category")] string? subcategory = null)
    {
        if (category == null)
        {
            var result = api.GetConflictCategories();
            if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";
            return JsonSerializer.Serialize(result.Payload, JsonOptions);
        }

        if (subcategory == null)
        {
            var result = api.GetConflictSubcategories(category);
            if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";
            return JsonSerializer.Serialize(result.Payload, JsonOptions);
        }

        var examplesResult = api.GetConflictExamples(category, subcategory);
        if (!examplesResult.IsSuccess) return $"Error: {examplesResult.ErrorMessage}";
        return JsonSerializer.Serialize(examplesResult.Payload, JsonOptions);
    }

    [McpServerTool(Name = "get_stock_scenes")]
    [Description("Gets stock scene ideas for common story situations. Without a category, lists all categories. With a category, returns the scene ideas in that category.")]
    public static string GetStockScenes(
        StoryCADApi api,
        [Description("Optional category name. Omit to list all categories.")] string? category = null)
    {
        if (category == null)
        {
            var result = api.GetStockSceneCategories();
            if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";
            return JsonSerializer.Serialize(result.Payload, JsonOptions);
        }

        var scenesResult = api.GetStockScenes(category);
        if (!scenesResult.IsSuccess) return $"Error: {scenesResult.ErrorMessage}";
        return JsonSerializer.Serialize(scenesResult.Payload, JsonOptions);
    }

    [McpServerTool(Name = "get_examples")]
    [Description("Gets valid example values for a story element property (e.g. Role, Archetype, Tone, ScenePurpose). Useful for knowing what values are acceptable for update_property.")]
    public static string GetExamples(
        StoryCADApi api,
        [Description("Property name (e.g. 'Role', 'Archetype', 'Tone', 'ScenePurpose')")] string propertyName)
    {
        var result = api.GetExamples(propertyName);
        if (!result.IsSuccess) return $"Error: {result.ErrorMessage}";
        return JsonSerializer.Serialize(result.Payload, JsonOptions);
    }
}
