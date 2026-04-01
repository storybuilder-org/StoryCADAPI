using System;
using Microsoft.UI.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using StoryCAD.Services.IoC;
using StoryCAD.Models;
using StoryCAD.Services.API;

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
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<AppState>();
            services.AddSingleton<StoryCADApi>();

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Configure the IoC container
            Ioc.Default.ConfigureServices(serviceProvider);
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
