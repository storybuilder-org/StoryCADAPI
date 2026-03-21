using StoryCADLib.Services.API;

namespace StoryCADCli.Commands;

public static class ResourceCommands
{
    public static int Execute(StoryCADApi api, string command, string[] args)
    {
        return command switch
        {
            "key-questions" => KeyQuestions(api, args),
            "master-plots" => MasterPlots(api, args),
            "beat-sheets" => BeatSheets(api, args),
            "conflict-categories" => ConflictCategories(api, args),
            "stock-scenes" => StockScenes(api, args),
            "examples" => Examples(api, args),
            _ => UnknownResource(command)
        };
    }

    private static int KeyQuestions(StoryCADApi api, string[] args)
    {
        if (args.Length == 0)
        {
            // List element types that have key questions
            var result = api.GetKeyQuestionElements();
            if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }
            Program.WriteJson(result.Payload);
            return 0;
        }

        // Get questions for a specific element type
        var questionsResult = api.GetKeyQuestions(args[0]);
        if (!questionsResult.IsSuccess) { Program.WriteError(questionsResult.ErrorMessage); return 1; }

        var questions = questionsResult.Payload.Select(q => new { topic = q.Topic, question = q.Question });
        Program.WriteJson(questions);
        return 0;
    }

    private static int MasterPlots(StoryCADApi api, string[] args)
    {
        if (args.Length == 0)
        {
            // List all master plot names
            var result = api.GetMasterPlotNames();
            if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }
            Program.WriteJson(result.Payload);
            return 0;
        }

        var plotName = string.Join(" ", args);

        // Get notes and scenes for the named plot
        var notesResult = api.GetMasterPlotNotes(plotName);
        if (!notesResult.IsSuccess) { Program.WriteError(notesResult.ErrorMessage); return 1; }

        var scenesResult = api.GetMasterPlotScenes(plotName);
        if (!scenesResult.IsSuccess) { Program.WriteError(scenesResult.ErrorMessage); return 1; }

        var output = new
        {
            name = plotName,
            notes = notesResult.Payload,
            scenes = scenesResult.Payload.Select(s => new { title = s.SceneTitle, notes = s.Notes })
        };
        Program.WriteJson(output);
        return 0;
    }

    private static int BeatSheets(StoryCADApi api, string[] args)
    {
        if (args.Length == 0)
        {
            // List all beat sheet names
            var result = api.GetBeatSheetNames();
            if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }
            Program.WriteJson(result.Payload);
            return 0;
        }

        var sheetName = string.Join(" ", args);
        var sheetResult = api.GetBeatSheet(sheetName);
        if (!sheetResult.IsSuccess) { Program.WriteError(sheetResult.ErrorMessage); return 1; }

        var (description, beats) = sheetResult.Payload;
        var output = new
        {
            name = sheetName,
            description,
            beats = beats.Select(b => new { title = b.BeatName, notes = b.BeatNotes })
        };
        Program.WriteJson(output);
        return 0;
    }

    private static int ConflictCategories(StoryCADApi api, string[] args)
    {
        if (args.Length == 0)
        {
            // List all conflict categories
            var result = api.GetConflictCategories();
            if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }
            Program.WriteJson(result.Payload);
            return 0;
        }

        if (args.Length == 1)
        {
            // List subcategories for a category
            var result = api.GetConflictSubcategories(args[0]);
            if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }
            Program.WriteJson(result.Payload);
            return 0;
        }

        // Get examples for category + subcategory
        var examplesResult = api.GetConflictExamples(args[0], string.Join(" ", args.Skip(1)));
        if (!examplesResult.IsSuccess) { Program.WriteError(examplesResult.ErrorMessage); return 1; }
        Program.WriteJson(examplesResult.Payload);
        return 0;
    }

    private static int StockScenes(StoryCADApi api, string[] args)
    {
        if (args.Length == 0)
        {
            // List all stock scene categories
            var result = api.GetStockSceneCategories();
            if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }
            Program.WriteJson(result.Payload);
            return 0;
        }

        var category = string.Join(" ", args);
        var scenesResult = api.GetStockScenes(category);
        if (!scenesResult.IsSuccess) { Program.WriteError(scenesResult.ErrorMessage); return 1; }
        Program.WriteJson(scenesResult.Payload);
        return 0;
    }

    private static int Examples(StoryCADApi api, string[] args)
    {
        if (args.Length == 0)
        {
            Program.WriteError("Usage: examples <property-name> (e.g., Role, Archetype, Tone, ScenePurpose)");
            return 1;
        }

        var result = api.GetExamples(args[0]);
        if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }
        Program.WriteJson(result.Payload);
        return 0;
    }

    private static int UnknownResource(string command)
    {
        Program.WriteError($"Unknown resource command: {command}");
        return 1;
    }
}
