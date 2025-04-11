# StoryCAD API Samples
This repo contains samples on how to use the StoryCAD API


### /StoryCADChat/
This sample is a simple console API to interact with StoryCAD in natural language via a Large Langauge Model.


### /ProseToOutline/
This sample is a application that turns an text file or document into a StoryCAD Outline File (.STBX) using a Large Language Model


## Quick Start

### Installing StoryCAD Package
The StoryCADLib API can be installed via [NuGet](https://www.nuget.org/packages/StoryCADLib)

### Sample Usage

```cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StoryCAD.Services.API;

namespace StoryCADConsoleSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create a new instance of the SemanticKernelApi (assuming dependency injection or manual instantiation)
            var api = new SemanticKernelApi();
            
            // Create an empty outline
            var outlineResult = await api.CreateEmptyOutline("The Great Adventure", "Jane Doe", "0");
            if (!outlineResult.IsSuccess)
            {
                Console.WriteLine("Failed to create outline: " + outlineResult.ErrorMessage);
                return;
            }
            Console.WriteLine("Outline created successfully.");
            
            // Suppose later we update a story element's property:
            Guid someElementGuid = outlineResult.Payload[0]; // Get a GUID from the created outline
            var updateResult = api.UpdateElementProperty(someElementGuid, "Name", "New Outline Name");
            if (!updateResult.IsSuccess)
            {
                Console.WriteLine("Failed to update element: " + updateResult.ErrorMessage);
            }
            else
            {
                Console.WriteLine("Element updated successfully.");
            }
            
            // Write the outline to disk
            var writeResult = await api.WriteOutline("C:\\Outlines\\MyStoryOutline.stbx");
            if (!writeResult.IsSuccess)
            {
                Console.WriteLine("Failed to write outline: " + writeResult.ErrorMessage);
            }
            else
            {
                Console.WriteLine("Outline written successfully.");
            }
        }
    }
}
```
