using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using StoryCADLib.Services.API;
using StoryCADLib.Services.IoC;

// Set Model and Key from environment variables
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable is required. Set it before running.");
var modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";
var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, apiKey);

// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
BootStrapper.Initialise(headless: true);
var api = CommunityToolkit.Mvvm.DependencyInjection.Ioc.Default.GetRequiredService<StoryCADApi>();

// Add the StoryCAD SK plugin
kernel.ImportPluginFromObject(api, "StoryCAD");

// Enable planning — Auto() lets the LLM call any registered function without user approval.
// Fine for a sample app, but production code should add filters for destructive operations.
OpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Create a history store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();
    if (userInput == null) break;
    if (userInput.Trim().Length == 0) continue;

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAiPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (true);