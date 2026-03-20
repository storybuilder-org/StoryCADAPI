using StoryCADLib.Models;
using StoryCADLib.Services.API;

namespace StoryCADCli.Commands;

public static class ReadCommands
{
    public static int List(StoryCADApi api, string[] args)
    {
        // Optional --type filter
        StoryItemType? typeFilter = null;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--type" && Enum.TryParse<StoryItemType>(args[i + 1], true, out var parsed))
            {
                typeFilter = parsed;
            }
        }

        if (typeFilter.HasValue)
        {
            var result = api.GetElementsByType(typeFilter.Value);
            if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }

            var elements = result.Payload.Select(e => new
            {
                guid = e.Uuid.ToString(),
                name = e.Name,
                type = e.ElementType.ToString()
            });
            Program.WriteJson(elements);
        }
        else
        {
            var result = api.GetAllElements();
            if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }

            var elements = result.Payload.Select(e => new
            {
                guid = e.Uuid.ToString(),
                name = e.Name,
                type = e.ElementType.ToString()
            });
            Program.WriteJson(elements);
        }

        return 0;
    }

    public static int Get(StoryCADApi api, string[] args)
    {
        if (args.Length < 1)
        {
            Program.WriteError("Usage: get <guid>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var guid))
        {
            Program.WriteError($"Invalid GUID: {args[0]}");
            return 1;
        }

        // Use GetElement which returns the serialized element
        var result = api.GetElement(guid);
        if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }

        // GetElement already returns the serialized JSON string as payload
        // Write it directly wrapped in our success envelope
        Console.WriteLine($"{{\"success\":true,\"data\":{result.Payload}}}");
        return 0;
    }

    public static int Search(StoryCADApi api, string[] args)
    {
        if (args.Length < 1)
        {
            Program.WriteError("Usage: search <text>");
            return 1;
        }

        var searchText = string.Join(" ", args);
        var result = api.SearchForText(searchText);
        if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }

        Program.WriteJson(result.Payload);
        return 0;
    }

    public static int Structure(StoryCADApi api)
    {
        var model = api.CurrentModel;
        if (model == null)
        {
            Program.WriteError("No model loaded");
            return 1;
        }

        var tree = BuildTree(api, model.ExplorerView);
        Program.WriteJson(tree);
        return 0;
    }

    public static int ProblemStructure(StoryCADApi api, string[] args)
    {
        if (args.Length < 1)
        {
            Program.WriteError("Usage: problem-structure <problem-guid>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var guid))
        {
            Program.WriteError($"Invalid GUID: {args[0]}");
            return 1;
        }

        var result = api.GetProblemStructure(guid);
        if (!result.IsSuccess) { Program.WriteError(result.ErrorMessage); return 1; }

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
        Program.WriteJson(output);
        return 0;
    }

    private static object BuildTree(StoryCADApi api,
        System.Collections.ObjectModel.ObservableCollection<StoryCADLib.ViewModels.StoryNodeItem> nodes)
    {
        return nodes.Select(node => new
        {
            guid = node.Uuid.ToString(),
            name = node.Name,
            type = node.Type.ToString(),
            children = node.Children.Count > 0
                ? BuildTree(api, node.Children)
                : null
        });
    }
}
