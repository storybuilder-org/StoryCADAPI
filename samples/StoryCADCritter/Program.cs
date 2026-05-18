using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using StoryCADCritter;
using StoryCADLib.Services.API;
using StoryCADLib.Services.IoC;

// ─────────────────────────────────────────────────────────────────────────────
// StoryCADCritter — per-element Key Questions critique walk over a StoryCAD
// outline. See issue storybuilder-org/StoryCADAPI#14 for the design intent.
//
// CLI:
//   StoryCADCritter [path-to-outline.stbx]
// With no arg, builds and critiques the "Last Lighthouse Keeper" demo outline
// in memory so the sample still works out of the box.
//
// Env vars:
//   OPENAI_API_KEY  (required)
//   OPENAI_MODEL    (optional; default: gpt-4o-mini)
//
// Exit codes:
//   0 = full success
//   1 = partial (some per-element failures; see report)
//   2 = hard failure (missing key / bad outline / no LLM access)
// ─────────────────────────────────────────────────────────────────────────────

Console.WriteLine("=== StoryCADCritter — per-element critique walk ===");
Console.WriteLine();

// 1) API key
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("ERROR: OPENAI_API_KEY environment variable is not set.");
    Console.Error.WriteLine("Set it (export OPENAI_API_KEY=sk-...) and re-run.");
    return 2;
}
var modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";
Console.WriteLine($"Model: {modelId}");

// 2) StoryCADLib bootstrap + API handle
BootStrapper.Initialise();
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

// 3) Semantic Kernel — built here in the composition root only. The
// orchestrator gets IChatCompletionService injected, so tests can stub it.
Kernel kernel;
IChatCompletionService chatService;
try
{
    kernel = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(modelId, apiKey)
        .Build();
    chatService = kernel.GetRequiredService<IChatCompletionService>();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERROR: Couldn't initialize the OpenAI chat client: {ex.Message}");
    return 2;
}

// 4) Load the critique system prompt from disk. Prompts/CritiquePrompt.md is
// copied to the output dir at build time (see .csproj).
string systemPrompt;
try
{
    systemPrompt = LoadPrompt();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERROR: Couldn't load CritiquePrompt.md: {ex.Message}");
    return 2;
}

// 5) Open or build the outline
string outlinePath;
string outlineDisplay;
bool isDemo = false;
if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
{
    outlinePath = args[0];
    if (!File.Exists(outlinePath))
    {
        Console.Error.WriteLine($"ERROR: Outline file not found: {outlinePath}");
        return 2;
    }
    Console.WriteLine($"Opening outline: {outlinePath}");
    var openResult = await api.OpenOutline(outlinePath);
    if (openResult == null || !openResult.IsSuccess)
    {
        Console.Error.WriteLine($"ERROR: Failed to open outline: {openResult?.ErrorMessage ?? "unknown error"}");
        return 2;
    }
    outlineDisplay = Path.GetFileNameWithoutExtension(outlinePath);
}
else
{
    isDemo = true;
    Console.WriteLine("No outline argument provided — building the 'Last Lighthouse Keeper' demo outline in memory.");
    try
    {
        await LighthouseKeeperFixture.BuildAsync(api);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"ERROR: Couldn't build demo outline: {ex.Message}");
        return 2;
    }
    // Use a stable display name + an in-cwd report path; we don't save the
    // .stbx since nobody asked for it.
    outlineDisplay = LighthouseKeeperFixture.Title;
    outlinePath = Path.Combine(Directory.GetCurrentDirectory(), "LastLighthouseKeeper.demo");
}

// 6) Run the critique
var orchestrator = new CritiqueOrchestrator(api, chatService, systemPrompt, kernel);
var progress = new Progress<string>(msg => Console.WriteLine($"  - {msg}"));

Console.WriteLine();
Console.WriteLine("Walking elements…");
CritiqueRunResult run;
try
{
    run = await orchestrator.RunAsync(outlinePath, progress);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERROR: Unhandled exception during critique: {ex.Message}");
    return 2;
}

// 7) Write the report
var report = CritiqueOrchestrator.RenderReport(run, outlineDisplay);
if (isDemo)
{
    report = report.Replace($"from `{outlinePath}`.", "from the built-in demo outline.");
}
try
{
    await File.WriteAllTextAsync(run.ReportPath, report);
    Console.WriteLine();
    Console.WriteLine($"Report written: {run.ReportPath}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"WARNING: Couldn't write report to {run.ReportPath}: {ex.Message}");
    Console.WriteLine();
    Console.WriteLine("--- Report (stdout fallback) ---");
    Console.WriteLine(report);
}

// 7a) Write companion files: .costs.json (run totals) and .raw.json (raw LLM
// per-element responses, before they were rendered into Markdown).
var jsonOpts = new System.Text.Json.JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = null
};

var costsPath = outlinePath + ".costs.json";
try
{
    await File.WriteAllTextAsync(costsPath, System.Text.Json.JsonSerializer.Serialize(run.Cost, jsonOpts));
    Console.WriteLine($"Costs written:  {costsPath}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"WARNING: Couldn't write costs to {costsPath}: {ex.Message}");
}

var rawPath = outlinePath + ".raw.json";
try
{
    var rawDoc = new
    {
        model = run.Cost.ModelId,
        elements = run.ElementCritiques.Select(e => new
        {
            uuid = e.Uuid,
            type = e.ElementType,
            name = e.Name,
            parsed = e.Parsed,
            rawResponse = e.RawResponse,
            error = e.ErrorMessage
        })
    };
    await File.WriteAllTextAsync(rawPath, System.Text.Json.JsonSerializer.Serialize(rawDoc, jsonOpts));
    Console.WriteLine($"Raw written:    {rawPath}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"WARNING: Couldn't write raw responses to {rawPath}: {ex.Message}");
}

// 8) Exit code
if (run.HardFailed)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine($"Hard failure: {run.HardFailureMessage}");
    return 2;
}
if (run.ShortCircuited)
{
    Console.WriteLine();
    Console.WriteLine($"Short-circuited: {run.ShortCircuitReason}");
    return 0;
}
if (run.HasPerElementErrors)
{
    Console.WriteLine();
    Console.WriteLine("Completed with per-element failures — see Errors section in the report.");
    return 1;
}

Console.WriteLine();
Console.WriteLine("Done.");
return 0;

// ─── local helpers ──────────────────────────────────────────────────────────

static string LoadPrompt()
{
    string[] candidates =
    {
        Path.Combine(AppContext.BaseDirectory, "Prompts", "CritiquePrompt.md"),
        Path.Combine(Directory.GetCurrentDirectory(), "Prompts", "CritiquePrompt.md"),
        Path.Combine(Directory.GetCurrentDirectory(), "samples", "StoryCADCritter", "Prompts", "CritiquePrompt.md")
    };
    foreach (var p in candidates)
    {
        if (File.Exists(p))
            return File.ReadAllText(p);
    }
    throw new FileNotFoundException(
        "CritiquePrompt.md not found. Looked in: " + string.Join(" | ", candidates));
}
