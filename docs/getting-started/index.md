# Installation

## Requirements

- .NET 10.0 SDK or later
- Windows, macOS, or Linux

## Install via NuGet

```bash
dotnet add package StoryCADLib
```

Or via the Package Manager Console in Visual Studio:

```powershell
Install-Package StoryCADLib
```

## Package Reference

Add to your `.csproj` file:

```xml
<PackageReference Include="StoryCADLib" Version="4.0.0" />
```

## Initialization

StoryCADLib uses dependency injection. Initialize the service container before using the API:

```csharp
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// Initialize in headless mode (console apps, web APIs, etc.)
BootStrapper.Initialise(headless: true);

// Get the API instance from the DI container
var api = Ioc.Default.GetRequiredService<SemanticKernelApi>();
```

### Headless Mode

The `headless: true` parameter configures StoryCADLib for use without a UI:

- Skips UI-specific service registration
- Dialog operations return default values
- File picker operations require explicit paths
- All core API operations work normally

This is the recommended mode for:
- Console applications
- ASP.NET web APIs
- Background services
- Batch processing tools
- AI agent integrations

## Next Steps

- [Quick Start Tutorial](quick-start.md) - Build your first outline
- [Hello World Sample](hello-world.md) - Minimal working example
