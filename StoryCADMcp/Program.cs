using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StoryCADLib.Services.API;
using StoryCADLib.Services.IoC;

// Initialize StoryCADLib's DI container (populates Ioc.Default)
BootStrapper.Initialise(headless: true);
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
