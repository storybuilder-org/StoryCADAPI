using CommunityToolkit.Mvvm.DependencyInjection;
using StoryCADLib.Models;
using StoryCADLib.Services.API;
using StoryCADLib.Services.IoC;

Console.WriteLine("=== StoryMetrics: Outline Analytics Dashboard ===");
Console.WriteLine();

// Initialize
BootStrapper.Initialise();
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

// Create a rich outline
Console.WriteLine("Building a sample outline...");
var createResult = await api.CreateEmptyOutline("Murder at Whitmore Manor", "Demo Author", "0");
if (!createResult.IsSuccess) { Console.WriteLine($"FAIL: {createResult.ErrorMessage}"); return 1; }
var overviewGuid = createResult.Payload[0].ToString();

// Add 2 Problems
var prob1 = api.AddElement(StoryItemType.Problem, overviewGuid, "Who killed Lord Whitmore?");
var prob2 = api.AddElement(StoryItemType.Problem, overviewGuid, "The family inheritance dispute");

// Add 4 Characters
var charDetective = api.AddElement(StoryItemType.Character, overviewGuid, "Detective Sarah Blake",
    new Dictionary<string, object> { { "Role", "Protagonist" }, { "Age", "38" } });
var charButler = api.AddElement(StoryItemType.Character, overviewGuid, "James the Butler",
    new Dictionary<string, object> { { "Role", "Antagonist" }, { "Age", "55" } });
var charDaughter = api.AddElement(StoryItemType.Character, overviewGuid, "Lady Catherine",
    new Dictionary<string, object> { { "Role", "Supporting" }, { "Age", "28" } });
var charDoctor = api.AddElement(StoryItemType.Character, overviewGuid, "Dr. Pemberton",
    new Dictionary<string, object> { { "Role", "Supporting" }, { "Age", "62" } });

// Add 3 Settings
var setManor = api.AddElement(StoryItemType.Setting, overviewGuid, "Whitmore Manor - Drawing Room",
    new Dictionary<string, object> { { "Locale", "English countryside" }, { "Period", "1920s" } });
var setGarden = api.AddElement(StoryItemType.Setting, overviewGuid, "The Manor Gardens",
    new Dictionary<string, object> { { "Locale", "English countryside" }, { "Season", "Autumn" } });
var setVillage = api.AddElement(StoryItemType.Setting, overviewGuid, "Barrington Village Pub",
    new Dictionary<string, object> { { "Locale", "Small English village" } });

// Add 6 Scenes with varied linkage
var scene1 = api.AddElement(StoryItemType.Scene, overviewGuid, "The Body is Found");
var scene2 = api.AddElement(StoryItemType.Scene, overviewGuid, "Interviewing the Butler");
var scene3 = api.AddElement(StoryItemType.Scene, overviewGuid, "Clue in the Garden");
var scene4 = api.AddElement(StoryItemType.Scene, overviewGuid, "Lady Catherine's Secret");
var scene5 = api.AddElement(StoryItemType.Scene, overviewGuid, "The Doctor's Testimony");
var scene6 = api.AddElement(StoryItemType.Scene, overviewGuid, "The Drawing Room Reveal");

// Link scenes to settings
api.UpdateElementProperties(scene1.Payload, new() { { "Setting", setManor.Payload.ToString() } });
api.UpdateElementProperties(scene2.Payload, new() { { "Setting", setManor.Payload.ToString() } });
api.UpdateElementProperties(scene3.Payload, new() { { "Setting", setGarden.Payload.ToString() } });
api.UpdateElementProperties(scene4.Payload, new() { { "Setting", setGarden.Payload.ToString() } });
api.UpdateElementProperties(scene5.Payload, new() { { "Setting", setVillage.Payload.ToString() } });
api.UpdateElementProperties(scene6.Payload, new() { { "Setting", setManor.Payload.ToString() } });

// Add cast to scenes (varied participation)
api.AddCastMember(scene1.Payload, charDetective.Payload); // Detective in 5 scenes
api.AddCastMember(scene1.Payload, charButler.Payload);

api.AddCastMember(scene2.Payload, charDetective.Payload);
api.AddCastMember(scene2.Payload, charButler.Payload);

api.AddCastMember(scene3.Payload, charDetective.Payload);
api.AddCastMember(scene3.Payload, charDaughter.Payload);

api.AddCastMember(scene4.Payload, charDetective.Payload);
api.AddCastMember(scene4.Payload, charDaughter.Payload);

api.AddCastMember(scene5.Payload, charDetective.Payload);
api.AddCastMember(scene5.Payload, charDoctor.Payload);

api.AddCastMember(scene6.Payload, charDetective.Payload);
api.AddCastMember(scene6.Payload, charButler.Payload);
api.AddCastMember(scene6.Payload, charDaughter.Payload);
api.AddCastMember(scene6.Payload, charDoctor.Payload);

// Link problems to protagonist/antagonist
api.UpdateElementProperties(prob1.Payload, new()
{
    { "Protagonist", charDetective.Payload.ToString() },
    { "Antagonist", charButler.Payload.ToString() }
});
api.UpdateElementProperties(prob2.Payload, new()
{
    { "Protagonist", charDaughter.Payload.ToString() }
});

Console.WriteLine("Outline built successfully.");
Console.WriteLine();

// ========================
// Analytics Dashboard
// ========================
Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║       STORY METRICS DASHBOARD            ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();

// 1. Element counts by type
Console.WriteLine("┌─── Element Counts ───────────────────────┐");
var types = new[] { StoryItemType.Problem, StoryItemType.Character, StoryItemType.Setting, StoryItemType.Scene };
var total = 0;
foreach (var type in types)
{
    var result = api.GetElementsByType(type);
    if (result.IsSuccess)
    {
        Console.WriteLine($"│  {type,-12} : {result.Payload.Count,3}                     │");
        total += result.Payload.Count;
    }
}
Console.WriteLine($"│  {"TOTAL",-12} : {total,3}                     │");
Console.WriteLine("└───────────────────────────────────────────┘");
Console.WriteLine();

// 2. Character appearances (using SearchForReferences)
Console.WriteLine("┌─── Character Appearances ────────────────┐");
var characters = api.GetElementsByType(StoryItemType.Character);
if (characters.IsSuccess)
{
    foreach (var ch in characters.Payload)
    {
        var refs = api.SearchForReferences(ch.Uuid);
        var refCount = refs.IsSuccess ? refs.Payload.Count : 0;
        Console.WriteLine($"│  {ch.Name,-28} : {refCount,3} refs │");
    }
}
Console.WriteLine("└───────────────────────────────────────────┘");
Console.WriteLine();

// 3. Setting usage
Console.WriteLine("┌─── Setting Usage ────────────────────────┐");
var settings = api.GetElementsByType(StoryItemType.Setting);
if (settings.IsSuccess)
{
    foreach (var s in settings.Payload)
    {
        var refs = api.SearchForReferences(s.Uuid);
        var refCount = refs.IsSuccess ? refs.Payload.Count : 0;
        Console.WriteLine($"│  {s.Name,-28} : {refCount,3} refs │");
    }
}
Console.WriteLine("└───────────────────────────────────────────┘");
Console.WriteLine();

// 4. Scenes per Problem (by checking protagonist references)
Console.WriteLine("┌─── Problem Scope ────────────────────────┐");
var problems = api.GetElementsByType(StoryItemType.Problem);
if (problems.IsSuccess)
{
    foreach (var p in problems.Payload)
    {
        var detail = api.GetStoryElement(p.Uuid);
        if (detail.IsSuccess && detail.Payload is ProblemModel pm)
        {
            var hasProtag = pm.Protagonist != Guid.Empty ? "Yes" : "No";
            var hasAntag = pm.Antagonist != Guid.Empty ? "Yes" : "No";
            Console.WriteLine($"│  {p.Name,-28}           │");
            Console.WriteLine($"│    Protagonist: {hasProtag,-5}  Antagonist: {hasAntag,-5}    │");
        }
    }
}
Console.WriteLine("└───────────────────────────────────────────┘");
Console.WriteLine();

// 5. Summary statistics
var allElements = api.GetAllElements();
var scenes = api.GetElementsByType(StoryItemType.Scene);
Console.WriteLine("┌─── Summary ──────────────────────────────┐");
Console.WriteLine($"│  Total story elements  : {allElements.Payload.Count,3}              │");
Console.WriteLine($"│  Story element types   : {types.Length,3}              │");
Console.WriteLine($"│  Scenes                : {(scenes.IsSuccess ? scenes.Payload.Count : 0),3}              │");
Console.WriteLine($"│  Settings              : {(settings.IsSuccess ? settings.Payload.Count : 0),3}              │");
Console.WriteLine($"│  Characters            : {(characters.IsSuccess ? characters.Payload.Count : 0),3}              │");
Console.WriteLine($"│  Problems              : {(problems.IsSuccess ? problems.Payload.Count : 0),3}              │");
Console.WriteLine("└───────────────────────────────────────────┘");

Console.WriteLine();
Console.WriteLine("=== StoryMetrics complete! ===");
return 0;
