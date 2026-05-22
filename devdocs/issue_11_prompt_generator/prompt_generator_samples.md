# Fiction Prompt Generator Integration Notes

## Overview

This guide outlines how to integrate a fiction writing prompt generator into StoryCAD (a FOSS outliner) and how to host or call an LLM server for prompt creation using C#.

---

## Integrating Prompts into StoryCAD

StoryCAD is structured around outlines with nodes such as *character*, *problem*, *scene*, and *conflict*.  
To add a **Prompt** element type:

1. Define a `Prompt` node with fields like:
   - `Genre`
   - `Focus` (Character, Problem, Scene)
   - `RawPromptText`

2. Expand the API:
   - Introduce a service method such as `CreatePromptNode(OutlineElement element, string promptText)`
   - This lets developers attach a generated prompt directly to any node.

3. Add a UI option (“Generate Prompt…”) that:
   - Serializes the current outline node.
   - Sends the information to a local or cloud LLM service.
   - Inserts the prompt as a new StoryCAD child node.

---

## Using Cloud LLMs From C#

For cloud-based models (e.g., OpenAI, Azure OpenAI):

```csharp
var client = new OpenAIClient(apiKey);
var prompt = await client.Chat.GetChatCompletionsAsync(new ChatCompletionsOptions
{
    Messages =
    {
        new ChatMessage("system", "You are an assistant that creates short creative writing prompts."),
        new ChatMessage("user", context)
    },
    Model = "gpt-4o-mini"
});
return prompt.Value.Choices.Message.Content;