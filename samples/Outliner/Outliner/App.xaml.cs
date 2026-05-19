using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.UI.Xaml;
using Outliner.Services;
using StoryCADLib.Services.IoC;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Outliner
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            UnhandledException += App_UnhandledException;

            // Initialize dependency injection
            InitializeDependencyInjection();
        }

        private void InitializeDependencyInjection()
        {
            // Register Semantic Kernel services before BootStrapper builds the provider
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");

            // Persistent user preferences (mode, last folders, model, etc.)
            var prefsService = new PreferencesService();
            var prefs = prefsService.Load();

            var modelId = !string.IsNullOrWhiteSpace(prefs.SelectedModelId)
                ? prefs.SelectedModelId
                : Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-5.4-mini-2026-03-17";

            var kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(modelId, apiKey)
                .Build();

            BootStrapper.Services.AddSingleton(kernel);
            BootStrapper.Services.AddSingleton(kernel.GetRequiredService<IChatCompletionService>());

            BootStrapper.Services.AddSingleton(prefsService);
            BootStrapper.Services.AddSingleton(prefs);

            BootStrapper.Services.AddSingleton(new ModelCatalogService(apiKey));

            BootStrapper.Initialise(headless: false);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MWindow = new MainWindow();
            MWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(MWindow);
            MWindow.Activate();

        }

        public void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            string x = e.Message;
            // Log or handle the exception
            //e.Handled = true;
        }

        public static Window? MWindow;

        // Add this property to the App class
        public static IntPtr MWindowHandle { get; set; }

    }
}
