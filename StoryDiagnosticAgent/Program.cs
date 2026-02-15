using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using StoryCADLib.Models;
using StoryCADLib.Services.API;
using StoryCADLib.Services.IoC;

Console.WriteLine("=== StoryDiagnosticAgent: AI-Powered Story Diagnosis ===");
Console.WriteLine();

// Step 1: Read API key from environment (fail fast if missing)
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new InvalidOperationException(
        "OPENAI_API_KEY environment variable is required. Set it before running.");
var modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";

Console.WriteLine($"Using model: {modelId}");
Console.WriteLine();

// Step 2: Initialize StoryCADLib and Semantic Kernel
BootStrapper.Initialise();
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(modelId, apiKey)
    .Build();
var chatService = kernel.GetRequiredService<IChatCompletionService>();

// Step 3: Create an outline with structural issues
Console.WriteLine("Building story outline with structural issues...");
var createResult = await api.CreateEmptyOutline("The Forgotten Kingdom", "Demo Author", "0");
if (!createResult.IsSuccess) { Console.WriteLine($"FAIL: {createResult.ErrorMessage}"); return 1; }
var overviewGuid = createResult.Payload[0].ToString();

// Characters
var charPrince = api.AddElement(StoryItemType.Character, overviewGuid, "Prince Alaric",
    new Dictionary<string, object> { { "Role", "Protagonist" }, { "Flaw", "" } }); // no flaw = passive
var charSorceress = api.AddElement(StoryItemType.Character, overviewGuid, "Sorceress Morwen",
    new Dictionary<string, object> { { "Role", "Antagonist" } });
var charMentor = api.AddElement(StoryItemType.Character, overviewGuid, "Old Sage Thandril",
    new Dictionary<string, object> { { "Role", "Supporting" } });

// Problem — protagonist has no stated goal or motive
var problem = api.AddElement(StoryItemType.Problem, overviewGuid, "The Curse on the Kingdom");
api.UpdateElementProperties(problem.Payload, new()
{
    { "Protagonist", charPrince.Payload.ToString() },
    { "Antagonist", charSorceress.Payload.ToString() },
    { "ProtGoal", "" },   // empty goal = passive protagonist
    { "ProtMotive", "" },  // empty motive
    { "Outcome", "" }      // no resolution
});

// Setting
var setCastle = api.AddElement(StoryItemType.Setting, overviewGuid, "Ruined Castle");

// Scenes — pacing issue: all action at start, nothing at end
var scene1 = api.AddElement(StoryItemType.Scene, overviewGuid, "The Curse Strikes");
api.UpdateElementProperties(scene1.Payload, new()
{
    { "Setting", setCastle.Payload.ToString() },
    { "Protagonist", charPrince.Payload.ToString() },
    { "Antagonist", charSorceress.Payload.ToString() },
    { "Outcome", "The kingdom falls under a dark curse" }
});

var scene2 = api.AddElement(StoryItemType.Scene, overviewGuid, "Meeting the Sage");
api.UpdateElementProperties(scene2.Payload, new()
{
    { "Setting", setCastle.Payload.ToString() },
    { "Outcome", "Thandril explains the curse" }
});
api.AddCastMember(scene2.Payload, charPrince.Payload);
api.AddCastMember(scene2.Payload, charMentor.Payload);

var scene3 = api.AddElement(StoryItemType.Scene, overviewGuid, "Wandering the Forest");
api.UpdateElementProperties(scene3.Payload, new()
{
    { "Outcome", "Nothing happens" }  // filler scene, no conflict
});
api.AddCastMember(scene3.Payload, charPrince.Payload);

var scene4 = api.AddElement(StoryItemType.Scene, overviewGuid, "Another Day Passes");
api.UpdateElementProperties(scene4.Payload, new()
{
    { "Outcome", "Still wandering" }  // another filler
});
api.AddCastMember(scene4.Payload, charPrince.Payload);

Console.WriteLine("Outline built.");
Console.WriteLine();

// Step 4: Serialize all elements for LLM context
Console.WriteLine("Serializing outline for analysis...");
var allElements = api.GetAllElements();
var serializedElements = new List<string>();
foreach (var el in allElements.Payload)
{
    var detail = api.GetElement(el.Uuid);
    if (detail.IsSuccess)
        serializedElements.Add($"[{el.ElementType}] {el.Name}:\n{detail.Payload}");
}
var outlineData = string.Join("\n\n", serializedElements);

// Step 5: Build the diagnostic prompt
var systemPrompt = """
    You are a story structure analyst familiar with StoryCAD concepts.
    StoryCAD is a fiction outlining tool where stories are composed of:
    - Problems: The central conflicts with Protagonist, Antagonist, goals, and motives
    - Characters: People in the story with roles, flaws, and arcs
    - Settings: Locations where scenes take place
    - Scenes: Individual story beats with conflict, cast, and outcomes

    Analyze the provided story outline and diagnose structural issues.
    Focus on these areas:
    1. PACING: Are scenes well-distributed? Is there a sagging middle or rushed ending?
    2. REVERSALS: Does the story have turning points, or is it too linear?
    3. PASSIVE PROTAGONIST: Does the protagonist have clear goals and motives? Do they drive the action?
    4. PLOT HOLES: Are there unresolved threads, missing connections, or logical gaps?

    Be specific — reference element names. Provide actionable recommendations.
    Keep the report concise (under 500 words).
    """;

var userPrompt = $"""
    Please analyze this story outline and provide a diagnostic report:

    {outlineData}
    """;

// Step 6: Send to LLM
Console.WriteLine("Sending to LLM for diagnosis...");
Console.WriteLine();

var history = new ChatHistory();
history.AddSystemMessage(systemPrompt);
history.AddUserMessage(userPrompt);

var response = await chatService.GetChatMessageContentAsync(history);

// Step 7: Print the diagnostic report
Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║       STORY DIAGNOSTIC REPORT                     ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine(response.Content);
Console.WriteLine();
Console.WriteLine("=== StoryDiagnosticAgent complete! ===");
return 0;
