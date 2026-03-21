using System.Text.Json;
using CommunityToolkit.Mvvm.DependencyInjection;
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using StoryCADCli.Commands;

namespace StoryCADCli;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: storycad-cli <outline-path> <command> [args...]");
            Console.Error.WriteLine("Commands: list, get, search, structure, add, update, delete,");
            Console.Error.WriteLine("          link-cast, add-relationship, key-questions, master-plots,");
            Console.Error.WriteLine("          beat-sheets, conflict-categories, stock-scenes,");
            Console.Error.WriteLine("          apply-beats, problem-structure, examples");
            return 1;
        }

        var outlinePath = args[0];
        var command = args[1].ToLowerInvariant();
        var commandArgs = args.Skip(2).ToArray();

        try
        {
            BootStrapper.Initialise(headless: true);
            var api = Ioc.Default.GetRequiredService<StoryCADApi>();

            // Resource commands don't need an outline open
            if (IsResourceCommand(command))
            {
                return ResourceCommands.Execute(api, command, commandArgs);
            }

            // All other commands need the outline loaded
            var openResult = await api.OpenOutline(outlinePath);
            if (!openResult.IsSuccess)
            {
                WriteError(openResult.ErrorMessage);
                return 1;
            }

            // Dispatch to command handlers
            return command switch
            {
                "list" => ReadCommands.List(api, commandArgs),
                "get" => ReadCommands.Get(api, commandArgs),
                "search" => ReadCommands.Search(api, commandArgs),
                "structure" => ReadCommands.Structure(api),
                "add" => await WriteCommands.Add(api, outlinePath, commandArgs),
                "update" => await WriteCommands.Update(api, outlinePath, commandArgs),
                "delete" => await WriteCommands.Delete(api, outlinePath, commandArgs),
                "link-cast" => await WriteCommands.LinkCast(api, outlinePath, commandArgs),
                "add-relationship" => await WriteCommands.AddRelationship(api, outlinePath, commandArgs),
                "apply-beats" => await WriteCommands.ApplyBeats(api, outlinePath, commandArgs),
                "problem-structure" => ReadCommands.ProblemStructure(api, commandArgs),
                _ => HandleUnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            WriteError($"Unexpected error: {ex.Message}");
            return 1;
        }
    }

    static bool IsResourceCommand(string command) => command is
        "key-questions" or "master-plots" or "beat-sheets" or
        "conflict-categories" or "stock-scenes" or "examples";

    static int HandleUnknownCommand(string command)
    {
        WriteError($"Unknown command: {command}");
        return 1;
    }

    public static void WriteJson(object payload)
    {
        var result = new { success = true, data = payload };
        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
    }

    public static void WriteError(string message)
    {
        var result = new { success = false, error = message };
        Console.Error.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
    }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
