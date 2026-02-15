using CommunityToolkit.Mvvm.DependencyInjection;
using StoryCADLib.Models;
using StoryCADLib.Services.API;
using StoryCADLib.Services.IoC;

Console.WriteLine("=== StoryGraphBasics: Create, Populate, Link, Save & Reload ===");
Console.WriteLine();

// Step 1: Initialize StoryCADLib in headless mode
Console.Write("1. Initializing StoryCADLib... ");
BootStrapper.Initialise();
var api = Ioc.Default.GetRequiredService<StoryCADApi>();
Console.WriteLine("OK");

// Step 2: Create an empty outline
Console.Write("2. Creating outline 'The Clockwork Conspiracy'... ");
var createResult = await api.CreateEmptyOutline("The Clockwork Conspiracy", "Demo Author", "0");
if (!createResult.IsSuccess) { Console.WriteLine($"FAIL: {createResult.ErrorMessage}"); return 1; }
Console.WriteLine($"OK ({createResult.Payload.Count} initial elements)");

// Get the overview GUID (first element returned, parent for new elements)
var overviewGuid = createResult.Payload[0].ToString();

// Step 3: Add a Problem
Console.Write("3. Adding Problem: 'The Missing Gears'... ");
var problemResult = api.AddElement(StoryItemType.Problem, overviewGuid, "The Missing Gears");
if (!problemResult.IsSuccess) { Console.WriteLine($"FAIL: {problemResult.ErrorMessage}"); return 1; }
var problemGuid = problemResult.Payload;
Console.WriteLine($"OK (GUID: {problemGuid})");

// Step 4: Add two Characters
Console.Write("4. Adding Character: 'Inspector Ada Thornton'... ");
var char1Result = api.AddElement(StoryItemType.Character, overviewGuid, "Inspector Ada Thornton");
if (!char1Result.IsSuccess) { Console.WriteLine($"FAIL: {char1Result.ErrorMessage}"); return 1; }
var char1Guid = char1Result.Payload;
Console.WriteLine($"OK (GUID: {char1Guid})");

Console.Write("   Adding Character: 'Victor Blackwell'... ");
var char2Result = api.AddElement(StoryItemType.Character, overviewGuid, "Victor Blackwell");
if (!char2Result.IsSuccess) { Console.WriteLine($"FAIL: {char2Result.ErrorMessage}"); return 1; }
var char2Guid = char2Result.Payload;
Console.WriteLine($"OK (GUID: {char2Guid})");

// Step 5: Add a Setting
Console.Write("5. Adding Setting: 'The Grand Clocktower'... ");
var settingResult = api.AddElement(StoryItemType.Setting, overviewGuid, "The Grand Clocktower");
if (!settingResult.IsSuccess) { Console.WriteLine($"FAIL: {settingResult.ErrorMessage}"); return 1; }
var settingGuid = settingResult.Payload;
Console.WriteLine($"OK (GUID: {settingGuid})");

// Step 6: Add two Scenes
Console.Write("6. Adding Scene: 'The Discovery'... ");
var scene1Result = api.AddElement(StoryItemType.Scene, overviewGuid, "The Discovery");
if (!scene1Result.IsSuccess) { Console.WriteLine($"FAIL: {scene1Result.ErrorMessage}"); return 1; }
var scene1Guid = scene1Result.Payload;
Console.WriteLine($"OK (GUID: {scene1Guid})");

Console.Write("   Adding Scene: 'The Confrontation'... ");
var scene2Result = api.AddElement(StoryItemType.Scene, overviewGuid, "The Confrontation");
if (!scene2Result.IsSuccess) { Console.WriteLine($"FAIL: {scene2Result.ErrorMessage}"); return 1; }
var scene2Guid = scene2Result.Payload;
Console.WriteLine($"OK (GUID: {scene2Guid})");

// Step 7: Link Protagonist and Antagonist on Problem
Console.Write("7. Linking Protagonist/Antagonist on Problem... ");
var protResult = api.UpdateElementProperties(problemGuid, new Dictionary<string, object>
{
    { "Protagonist", char1Guid.ToString() },
    { "Antagonist", char2Guid.ToString() }
});
if (!protResult.IsSuccess) { Console.WriteLine($"FAIL: {protResult.ErrorMessage}"); return 1; }
Console.WriteLine("OK");

// Step 8: Link Setting and Cast on Scenes
Console.Write("8. Linking Setting on Scene 1... ");
var setScene1 = api.UpdateElementProperties(scene1Guid, new Dictionary<string, object>
{
    { "Setting", settingGuid.ToString() }
});
if (!setScene1.IsSuccess) { Console.WriteLine($"FAIL: {setScene1.ErrorMessage}"); return 1; }
Console.WriteLine("OK");

Console.Write("   Adding cast to Scene 1... ");
var cast1a = api.AddCastMember(scene1Guid, char1Guid);
var cast1b = api.AddCastMember(scene1Guid, char2Guid);
if (!cast1a.IsSuccess || !cast1b.IsSuccess) { Console.WriteLine("FAIL"); return 1; }
Console.WriteLine("OK");

Console.Write("   Linking Setting on Scene 2... ");
var setScene2 = api.UpdateElementProperties(scene2Guid, new Dictionary<string, object>
{
    { "Setting", settingGuid.ToString() }
});
if (!setScene2.IsSuccess) { Console.WriteLine($"FAIL: {setScene2.ErrorMessage}"); return 1; }
Console.WriteLine("OK");

Console.Write("   Adding cast to Scene 2... ");
var cast2a = api.AddCastMember(scene2Guid, char1Guid);
var cast2b = api.AddCastMember(scene2Guid, char2Guid);
if (!cast2a.IsSuccess || !cast2b.IsSuccess) { Console.WriteLine("FAIL"); return 1; }
Console.WriteLine("OK");

// Step 9: Add a relationship between characters
Console.Write("9. Adding relationship (Ada <-> Victor: 'Rivals')... ");
var relResult = api.AddRelationship(char1Guid, char2Guid, "Rivals", mirror: true);
if (!relResult.IsSuccess) { Console.WriteLine($"FAIL: {relResult.ErrorMessage}"); return 1; }
Console.WriteLine("OK");

// Step 10: Query the outline
Console.WriteLine();
Console.WriteLine("--- Querying the Outline ---");

var allElements = api.GetAllElements();
Console.WriteLine($"Total elements: {allElements.Payload.Count}");

foreach (var type in new[] { StoryItemType.Problem, StoryItemType.Character, StoryItemType.Setting, StoryItemType.Scene })
{
    var byType = api.GetElementsByType(type);
    if (byType.IsSuccess)
    {
        Console.WriteLine($"  {type}: {byType.Payload.Count}");
        foreach (var el in byType.Payload)
            Console.WriteLine($"    - {el.Name} ({el.Uuid})");
    }
}

// Step 11: Retrieve a single element by GUID
Console.WriteLine();
Console.Write("Retrieving Problem by GUID... ");
var getResult = api.GetElement(problemGuid);
if (getResult.IsSuccess)
    Console.WriteLine($"OK - Got: {getResult.Payload}");
else
    Console.WriteLine($"FAIL: {getResult.ErrorMessage}");

// Step 12: Save to temp file
var tempPath = Path.Combine(Path.GetTempPath(), "StoryGraphBasics.stbx");
Console.WriteLine();
Console.Write($"10. Saving to {tempPath}... ");
var writeResult = await api.WriteOutline(tempPath);
if (!writeResult.IsSuccess) { Console.WriteLine($"FAIL: {writeResult.ErrorMessage}"); return 1; }
Console.WriteLine($"OK ({new FileInfo(tempPath).Length} bytes)");

// Step 13: Reopen and verify
var originalCount = allElements.Payload.Count;
Console.Write("11. Reopening saved file... ");
var openResult = await api.OpenOutline(tempPath);
if (!openResult.IsSuccess) { Console.WriteLine($"FAIL: {openResult.ErrorMessage}"); return 1; }
Console.WriteLine("OK");

Console.Write("12. Verifying element count matches... ");
var reopened = api.GetAllElements();
if (reopened.Payload.Count == originalCount)
    Console.WriteLine($"OK ({reopened.Payload.Count} elements)");
else
    Console.WriteLine($"MISMATCH: expected {originalCount}, got {reopened.Payload.Count}");

// Cleanup
try { File.Delete(tempPath); } catch { }

Console.WriteLine();
Console.WriteLine("=== StoryGraphBasics complete! ===");
return 0;
