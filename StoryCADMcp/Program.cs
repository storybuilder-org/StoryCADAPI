using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StoryCADLib.Models;
using StoryCADLib.Services.API;
using StoryCADLib.Services.IoC;
using StoryCADMcp;
using StoryCollaborator.Models;

// The server keeps its own root folder, so a sign-in refresh never writes the installed app's
// Preferences.json (Collaborator #291, design section 5.5). StoryCADLib honors the variable when it runs
// headless, in Debug and Release builds. Set it before BootStrapper.Initialise, which makes AppState and
// PreferenceService; Initialise below is called with headless: true.
if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(AppState.RootDirectoryOverrideVariable)))
{
    var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StoryCADMcp");
    Directory.CreateDirectory(root);
    Environment.SetEnvironmentVariable(AppState.RootDirectoryOverrideVariable, root);
}

// Initialize StoryCADLib's DI container (populates Ioc.Default)
BootStrapper.Initialise(headless: true);

// The Worker journal counts this server's calls apart from the app's.
KernelFactory.ClientName = "mcp";

// Copy the Collaborator sign-in from the installed app. A missing or expired sign-in is not fatal:
// the outline tools still work, and each Collaborator run checks again.
var signInError = await CollaboratorSignIn.EnsureAsync();
if (signInError != null)
    Console.Error.WriteLine($"Collaborator: {signInError}");
var api = Ioc.Default.GetRequiredService<StoryCADApi>();

// Build MCP server host, bridging StoryCADLib's DI into the MCP container
var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Services.AddSingleton(api);
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
