using CommunityToolkit.Mvvm.DependencyInjection;
using StoryCADLib.Models;
using StoryCADLib.Services.API;
using StoryCADLib.Services.IoC;

int failures = 0;

Console.WriteLine("=== StoryCADLib 4.0 Headless Verification ===");
Console.WriteLine();

// Step 1: Headless initialization
Console.Write("1. Headless initialization... ");
try
{
    BootStrapper.Initialise(); // headless=true by default
    Console.WriteLine("PASS");
}
catch (Exception ex)
{
    Console.WriteLine($"FAIL: {ex.Message}");
    return 1;
}

// Step 2: DI resolution of StoryCADApi
Console.Write("2. DI resolution of StoryCADApi... ");
StoryCADApi api;
try
{
    api = Ioc.Default.GetRequiredService<StoryCADApi>();
    Console.WriteLine("PASS");
}
catch (Exception ex)
{
    Console.WriteLine($"FAIL: {ex.Message}");
    return 1;
}

// Step 3: CreateEmptyOutline
Console.Write("3. CreateEmptyOutline... ");
var createResult = await api.CreateEmptyOutline("Headless Test Story", "Test Author", "0");
if (!createResult.IsSuccess)
{
    Console.WriteLine($"FAIL: {createResult.ErrorMessage}");
    return 1;
}
Console.WriteLine($"PASS ({createResult.Payload.Count} element GUIDs returned)");

// Step 4: Element inspection
Console.Write("4. Element inspection... ");
var elementsResult = api.GetAllElements();
if (!elementsResult.IsSuccess)
{
    Console.WriteLine($"FAIL: {elementsResult.ErrorMessage}");
    return 1;
}
var elements = elementsResult.Payload;
if (elements.Count == 0)
{
    Console.WriteLine("FAIL: No elements found");
    return 1;
}
Console.WriteLine($"PASS ({elements.Count} elements)");
foreach (var el in elements)
{
    Console.WriteLine($"   - {el.Name} ({el.ElementType})");
}

// Step 5: WriteOutline
var tempPath = Path.Combine(Path.GetTempPath(), "HeadlessTest.stbx");
Console.Write($"5. WriteOutline to {tempPath}... ");
var writeResult = await api.WriteOutline(tempPath);
if (!writeResult.IsSuccess)
{
    Console.WriteLine($"FAIL: {writeResult.ErrorMessage}");
    failures++;
}
else if (!File.Exists(tempPath))
{
    Console.WriteLine("FAIL: File was not created");
    failures++;
}
else
{
    Console.WriteLine($"PASS ({new FileInfo(tempPath).Length} bytes)");
}

// Step 6: OpenOutline
Console.Write("6. OpenOutline from file... ");
var openResult = await api.OpenOutline(tempPath);
if (!openResult.IsSuccess)
{
    Console.WriteLine($"FAIL: {openResult.ErrorMessage}");
    failures++;
}
else
{
    Console.WriteLine("PASS");
}

// Step 7: Verify reopened model
Console.Write("7. Verify reopened model matches... ");
var reopenedResult = api.GetAllElements();
if (!reopenedResult.IsSuccess)
{
    Console.WriteLine($"FAIL: {reopenedResult.ErrorMessage}");
    failures++;
}
else if (reopenedResult.Payload.Count != elements.Count)
{
    Console.WriteLine($"FAIL: Expected {elements.Count} elements, got {reopenedResult.Payload.Count}");
    failures++;
}
else
{
    Console.WriteLine($"PASS ({reopenedResult.Payload.Count} elements)");
}

// Cleanup
try { File.Delete(tempPath); } catch { /* best-effort */ }

Console.WriteLine();
if (failures > 0)
{
    Console.WriteLine($"=== {failures} check(s) FAILED ===");
    return 1;
}

Console.WriteLine("=== All checks passed ===");
return 0;
