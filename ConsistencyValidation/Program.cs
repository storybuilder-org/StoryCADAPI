using CommunityToolkit.Mvvm.DependencyInjection;
using StoryCADLib.Models;
using StoryCADLib.Services.API;
using StoryCADLib.Services.IoC;

Console.WriteLine("=== ConsistencyValidation: Outline Quality Report ===");
Console.WriteLine();

// Initialize
BootStrapper.Initialise();
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

// Build an outline with intentional issues
Console.WriteLine("Building outline with intentional issues...");
var createResult = await api.CreateEmptyOutline("The Broken Outline", "Demo Author", "0");
if (!createResult.IsSuccess) { Console.WriteLine($"FAIL: {createResult.ErrorMessage}"); return 1; }
var overviewGuid = createResult.Payload[0].ToString();

// Characters — one will be an orphan (never used anywhere)
var charHero = api.AddElement(StoryItemType.Character, overviewGuid, "Marcus Vane");
var charVillain = api.AddElement(StoryItemType.Character, overviewGuid, "Dr. Nyx");
var charOrphan = api.AddElement(StoryItemType.Character, overviewGuid, "Unused Bystander"); // orphan

// Settings — one will be unused
var setCity = api.AddElement(StoryItemType.Setting, overviewGuid, "Neo-Tokyo Undercity");
var setLab = api.AddElement(StoryItemType.Setting, overviewGuid, "Abandoned Laboratory");
var setUnused = api.AddElement(StoryItemType.Setting, overviewGuid, "Mountain Retreat"); // unused

// Problems — one without protagonist
var prob1 = api.AddElement(StoryItemType.Problem, overviewGuid, "Stop the Virus");
api.UpdateElementProperties(prob1.Payload, new()
{
    { "Protagonist", charHero.Payload.ToString() },
    { "Antagonist", charVillain.Payload.ToString() }
});
var probNoProtag = api.AddElement(StoryItemType.Problem, overviewGuid, "Fix the Economy"); // no protagonist

// Scenes — with various issues
// Scene 1: Good scene (has setting, cast, conflict, outcome)
var sceneGood = api.AddElement(StoryItemType.Scene, overviewGuid, "Ambush in the Undercity");
api.UpdateElementProperties(sceneGood.Payload, new()
{
    { "Setting", setCity.Payload.ToString() },
    { "Protagonist", charHero.Payload.ToString() },
    { "Antagonist", charVillain.Payload.ToString() },
    { "Outcome", "Hero escapes but is wounded" }
});
api.AddCastMember(sceneGood.Payload, charHero.Payload);
api.AddCastMember(sceneGood.Payload, charVillain.Payload);

// Scene 2: No conflict (no protagonist/antagonist)
var sceneNoConflict = api.AddElement(StoryItemType.Scene, overviewGuid, "Morning Coffee");
api.UpdateElementProperties(sceneNoConflict.Payload, new()
{
    { "Setting", setCity.Payload.ToString() },
    { "Outcome", "Nothing happens" }
});
api.AddCastMember(sceneNoConflict.Payload, charHero.Payload);

// Scene 3: No outcome
var sceneNoOutcome = api.AddElement(StoryItemType.Scene, overviewGuid, "Lab Break-In");
api.UpdateElementProperties(sceneNoOutcome.Payload, new()
{
    { "Setting", setLab.Payload.ToString() },
    { "Protagonist", charHero.Payload.ToString() },
    { "Antagonist", charVillain.Payload.ToString() }
});
api.AddCastMember(sceneNoOutcome.Payload, charHero.Payload);

// Scene 4: No setting
var sceneNoSetting = api.AddElement(StoryItemType.Scene, overviewGuid, "The Phone Call");
api.UpdateElementProperties(sceneNoSetting.Payload, new()
{
    { "Protagonist", charHero.Payload.ToString() }
});
api.AddCastMember(sceneNoSetting.Payload, charHero.Payload);

Console.WriteLine("Outline built.");
Console.WriteLine();

// ========================
// Validation Checks
// ========================
var issueCount = 0;

Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║       CONSISTENCY VALIDATION REPORT               ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
Console.WriteLine();

// Check 1: Scenes without conflict (no Protagonist AND no Antagonist)
Console.WriteLine("── Check 1: Scenes without conflict ──");
var scenes = api.GetElementsByType(StoryItemType.Scene);
if (scenes.IsSuccess)
{
    foreach (var s in scenes.Payload)
    {
        var el = api.GetStoryElement(s.Uuid);
        if (el.IsSuccess && el.Payload is SceneModel scene)
        {
            if (scene.Protagonist == Guid.Empty && scene.Antagonist == Guid.Empty)
            {
                Console.WriteLine($"  WARNING: Scene '{scene.Name}' has no protagonist or antagonist");
                issueCount++;
            }
        }
    }
}
Console.WriteLine();

// Check 2: Scenes without outcome
Console.WriteLine("── Check 2: Scenes without outcome ──");
if (scenes.IsSuccess)
{
    foreach (var s in scenes.Payload)
    {
        var el = api.GetStoryElement(s.Uuid);
        if (el.IsSuccess && el.Payload is SceneModel scene)
        {
            if (string.IsNullOrWhiteSpace(scene.Outcome))
            {
                Console.WriteLine($"  WARNING: Scene '{scene.Name}' has no outcome");
                issueCount++;
            }
        }
    }
}
Console.WriteLine();

// Check 3: Problems without protagonist
Console.WriteLine("── Check 3: Problems without protagonist ──");
var problems = api.GetElementsByType(StoryItemType.Problem);
if (problems.IsSuccess)
{
    foreach (var p in problems.Payload)
    {
        var el = api.GetStoryElement(p.Uuid);
        if (el.IsSuccess && el.Payload is ProblemModel problem)
        {
            if (problem.Protagonist == Guid.Empty)
            {
                Console.WriteLine($"  WARNING: Problem '{problem.Name}' has no protagonist assigned");
                issueCount++;
            }
        }
    }
}
Console.WriteLine();

// Check 4: Orphan characters (never referenced by any scene or problem)
Console.WriteLine("── Check 4: Orphan characters ──");
var characters = api.GetElementsByType(StoryItemType.Character);
if (characters.IsSuccess)
{
    foreach (var ch in characters.Payload)
    {
        var refs = api.SearchForReferences(ch.Uuid);
        if (refs.IsSuccess && refs.Payload.Count == 0)
        {
            Console.WriteLine($"  WARNING: Character '{ch.Name}' is never referenced by any element");
            issueCount++;
        }
    }
}
Console.WriteLine();

// Check 5: Unused settings (never referenced)
Console.WriteLine("── Check 5: Unused settings ──");
var settingsList = api.GetElementsByType(StoryItemType.Setting);
if (settingsList.IsSuccess)
{
    foreach (var s in settingsList.Payload)
    {
        var refs = api.SearchForReferences(s.Uuid);
        if (refs.IsSuccess && refs.Payload.Count == 0)
        {
            Console.WriteLine($"  WARNING: Setting '{s.Name}' is never used in any scene");
            issueCount++;
        }
    }
}
Console.WriteLine();

// Check 6: Scenes without setting
Console.WriteLine("── Check 6: Scenes without setting ──");
if (scenes.IsSuccess)
{
    foreach (var s in scenes.Payload)
    {
        var el = api.GetStoryElement(s.Uuid);
        if (el.IsSuccess && el.Payload is SceneModel scene)
        {
            if (scene.Setting == Guid.Empty)
            {
                Console.WriteLine($"  WARNING: Scene '{scene.Name}' has no setting assigned");
                issueCount++;
            }
        }
    }
}
Console.WriteLine();

// Summary
Console.WriteLine("══════════════════════════════════════════════════");
if (issueCount == 0)
    Console.WriteLine("  All checks passed. No issues found.");
else
    Console.WriteLine($"  Total issues found: {issueCount}");
Console.WriteLine("══════════════════════════════════════════════════");

Console.WriteLine();
Console.WriteLine("=== ConsistencyValidation complete! ===");
return 0;
