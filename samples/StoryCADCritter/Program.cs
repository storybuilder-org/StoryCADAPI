using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using StoryCADLib.Models;
using StoryCADLib.Services.API;
using StoryCADLib.Services.IoC;

Console.WriteLine("=== StoryCADCritter: AI-Scored Story Evaluation ===");
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

// Step 3: Create a fleshed-out outline
Console.WriteLine("Building story outline for critique...");
var createResult = await api.CreateEmptyOutline("The Last Lighthouse Keeper", "Demo Author", "0");
if (!createResult.IsSuccess) { Console.WriteLine($"FAIL: {createResult.ErrorMessage}"); return 1; }
var overviewGuid = createResult.Payload[0].ToString();

// Characters
var charElara = api.AddElement(StoryItemType.Character, overviewGuid, "Elara Marsh",
    new Dictionary<string, object>
    {
        { "Role", "Protagonist" },
        { "Age", "34" },
        { "Flaw", "Cannot trust anyone since her father's betrayal" },
        { "BackStory", "Grew up in the lighthouse, left for the city, returned after her father's death" }
    });
var charSilas = api.AddElement(StoryItemType.Character, overviewGuid, "Silas Croft",
    new Dictionary<string, object>
    {
        { "Role", "Antagonist" },
        { "Age", "50" },
        { "Flaw", "Greed disguised as civic duty" }
    });
var charMarina = api.AddElement(StoryItemType.Character, overviewGuid, "Marina Torres",
    new Dictionary<string, object>
    {
        { "Role", "Supporting" },
        { "Age", "29" }
    });

// Problems
var prob1 = api.AddElement(StoryItemType.Problem, overviewGuid, "Save the Lighthouse");
api.UpdateElementProperties(prob1.Payload, new()
{
    { "Protagonist", charElara.Payload.ToString() },
    { "Antagonist", charSilas.Payload.ToString() },
    { "ProtGoal", "Preserve the lighthouse and her father's legacy" },
    { "ProtMotive", "Guilt over leaving and her father dying alone" },
    { "AntagGoal", "Demolish the lighthouse for a resort development" },
    { "AntagMotive", "Profit and political ambition" },
    { "Theme", "Legacy vs. Progress" },
    { "Premise", "A woman must confront her past to save her father's lighthouse from demolition" }
});

var prob2 = api.AddElement(StoryItemType.Problem, overviewGuid, "Trust Again");
api.UpdateElementProperties(prob2.Payload, new()
{
    { "Protagonist", charElara.Payload.ToString() },
    { "ProtGoal", "Learn to rely on others" },
    { "ProtMotive", "Loneliness and the realization she cannot fight alone" }
});

// Settings
var setLighthouse = api.AddElement(StoryItemType.Setting, overviewGuid, "Marsh Point Lighthouse",
    new Dictionary<string, object>
    {
        { "Locale", "Remote coastal headland" },
        { "Season", "Late autumn" },
        { "Period", "Present day" }
    });
var setTownHall = api.AddElement(StoryItemType.Setting, overviewGuid, "Harborview Town Hall",
    new Dictionary<string, object> { { "Locale", "Small fishing town" } });
var setCafe = api.AddElement(StoryItemType.Setting, overviewGuid, "The Anchor Cafe",
    new Dictionary<string, object> { { "Locale", "Small fishing town waterfront" } });

// Scenes
var scene1 = api.AddElement(StoryItemType.Scene, overviewGuid, "Homecoming");
api.UpdateElementProperties(scene1.Payload, new()
{
    { "Setting", setLighthouse.Payload.ToString() },
    { "Protagonist", charElara.Payload.ToString() },
    { "Outcome", "Elara discovers the demolition notice" }
});
api.AddCastMember(scene1.Payload, charElara.Payload);

var scene2 = api.AddElement(StoryItemType.Scene, overviewGuid, "The Town Meeting");
api.UpdateElementProperties(scene2.Payload, new()
{
    { "Setting", setTownHall.Payload.ToString() },
    { "Protagonist", charElara.Payload.ToString() },
    { "Antagonist", charSilas.Payload.ToString() },
    { "Outcome", "Silas reveals the council vote is in two weeks" }
});
api.AddCastMember(scene2.Payload, charElara.Payload);
api.AddCastMember(scene2.Payload, charSilas.Payload);

var scene3 = api.AddElement(StoryItemType.Scene, overviewGuid, "An Unlikely Ally");
api.UpdateElementProperties(scene3.Payload, new()
{
    { "Setting", setCafe.Payload.ToString() },
    { "Protagonist", charElara.Payload.ToString() },
    { "Outcome", "Marina offers to help with the historical preservation filing" }
});
api.AddCastMember(scene3.Payload, charElara.Payload);
api.AddCastMember(scene3.Payload, charMarina.Payload);

var scene4 = api.AddElement(StoryItemType.Scene, overviewGuid, "Father's Secret");
api.UpdateElementProperties(scene4.Payload, new()
{
    { "Setting", setLighthouse.Payload.ToString() },
    { "Protagonist", charElara.Payload.ToString() },
    { "Outcome", "Elara finds her father's journal revealing why he stayed" }
});
api.AddCastMember(scene4.Payload, charElara.Payload);

var scene5 = api.AddElement(StoryItemType.Scene, overviewGuid, "The Council Vote");
api.UpdateElementProperties(scene5.Payload, new()
{
    { "Setting", setTownHall.Payload.ToString() },
    { "Protagonist", charElara.Payload.ToString() },
    { "Antagonist", charSilas.Payload.ToString() },
    { "Outcome", "The vote is tied; Elara must make a personal appeal" }
});
api.AddCastMember(scene5.Payload, charElara.Payload);
api.AddCastMember(scene5.Payload, charSilas.Payload);
api.AddCastMember(scene5.Payload, charMarina.Payload);

Console.WriteLine("Outline built.");
Console.WriteLine();

// Step 4: Serialize elements and gather key questions
Console.WriteLine("Serializing outline and gathering key questions...");
var allElements = api.GetAllElements();
var serializedElements = new List<string>();
foreach (var el in allElements.Payload)
{
    var detail = api.GetElement(el.Uuid);
    if (detail.IsSuccess)
        serializedElements.Add($"[{el.ElementType}] {el.Name}:\n{detail.Payload}");
}
var outlineData = string.Join("\n\n", serializedElements);

// Gather key questions for context
var keyQuestionTypes = api.GetKeyQuestionElements();
var keyQuestionsText = "";
if (keyQuestionTypes.IsSuccess)
{
    var sections = new List<string>();
    foreach (var elementType in keyQuestionTypes.Payload)
    {
        var questions = api.GetKeyQuestions(elementType);
        if (questions.IsSuccess)
        {
            var qs = questions.Payload.Select(q => $"  - [{q.Topic}] {q.Question}");
            sections.Add($"{elementType}:\n{string.Join("\n", qs)}");
        }
    }
    keyQuestionsText = string.Join("\n\n", sections);
}

// Step 5: Build the critique prompt with scoring rubric
var systemPrompt = $"""
    You are an expert story editor evaluating a fiction outline using StoryCAD.

    StoryCAD organizes stories into Problems, Characters, Settings, and Scenes.
    Each element has structured fields (goals, motives, flaws, outcomes, etc.).

    Here are the key questions that fiction writers should consider for each element type:
    {keyQuestionsText}

    Evaluate the outline against these 5 craft criteria. Score each 1-5:
    1 = Major issues, 2 = Significant gaps, 3 = Adequate, 4 = Good, 5 = Excellent

    CRITERIA:
    1. PREMISE: Is the story premise clear, compelling, and well-articulated?
    2. CHARACTER ARCS: Do characters have clear flaws, goals, and growth potential?
    3. SCENE STRUCTURE: Do scenes have conflict, stakes, and meaningful outcomes?
    4. CONFLICT: Is the central conflict well-defined with worthy opposition?
    5. THEME: Is there a coherent theme that connects the story elements?

    FORMAT YOUR RESPONSE EXACTLY LIKE THIS:

    ## Scored Critique

    | Criterion | Score | Assessment |
    |-----------|-------|------------|
    | Premise | X/5 | [brief assessment] |
    | Character Arcs | X/5 | [brief assessment] |
    | Scene Structure | X/5 | [brief assessment] |
    | Conflict | X/5 | [brief assessment] |
    | Theme | X/5 | [brief assessment] |

    **Overall: XX/25**

    ## Recommendations
    [3-5 specific, actionable recommendations referencing element names]
    """;

var userPrompt = $"""
    Please evaluate this story outline:

    {outlineData}
    """;

// Step 6: Send to LLM
Console.WriteLine("Sending to LLM for critique...");
Console.WriteLine();

var history = new ChatHistory();
history.AddSystemMessage(systemPrompt);
history.AddUserMessage(userPrompt);

var response = await chatService.GetChatMessageContentAsync(history);

// Step 7: Print the critique report
Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║       AUTOMATED STORY CRITIQUE                    ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine(response.Content);
Console.WriteLine();
Console.WriteLine("=== StoryCADCritter complete! ===");
return 0;
