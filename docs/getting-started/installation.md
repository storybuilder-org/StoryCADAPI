---
layout: default
title: "Installation"
parent: "Getting Started"
nav_order: 1
---

# Installation

StoryCADLib is a .NET library, so this page covers the .NET / NuGet install. The Python examples elsewhere in these docs use the `storycad` module — a thin Python wrapper around this same library.

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

<div class="code-tabs" markdown="1">

```csharp
using StoryCADLib.Services.IoC;
using StoryCADLib.Services.API;
using CommunityToolkit.Mvvm.DependencyInjection;

// Initialize in headless mode (console apps, web APIs, etc.)
BootStrapper.Initialise(headless: true);

// Get the API instance from the DI container
var api = Ioc.Default.GetRequiredService<StoryCADApi>();
```

```python
from storycad import StoryCAD

# Initialize in headless mode (console apps, web APIs, etc.)
# The sc object is the API directly — there is no separate DI container step.
sc = StoryCAD(headless=True)
```

</div>

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

- [Quick Start Tutorial](quick-start.md) - Build your first outline (Hello World)
