using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCAD.Models;
using StoryCAD.Services.API;
using System;
using System.IO;

namespace OutlinerTests
{
    /// <summary>
    /// Global test setup and cleanup for all tests in the assembly.
    /// </summary>
    [TestClass]
    public static class TestSetup
    {
        /// <summary>
        /// Runs once before any tests in the assembly.
        /// Sets up IoC container and configures test environment.
        /// </summary>
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            // Initialize DI container for tests
            var services = new ServiceCollection();

            // Register services required for testing
            services.AddSingleton<AppState>();
            services.AddSingleton<StoryCADApi>();

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Configure the IoC container
            Ioc.Default.ConfigureServices(serviceProvider);

            // Set up AppState for testing
            var appState = Ioc.Default.GetRequiredService<AppState>();
            appState.Headless = true;

            // Create test directories
            Directory.CreateDirectory(App.InputDir);
            Directory.CreateDirectory(App.ResultsDir);
        }

        /// <summary>
        /// Runs once after all tests in the assembly complete.
        /// Cleans up resources to ensure clean shutdown.
        /// </summary>
        [AssemblyCleanup]
        public static void Cleanup()
        {
            // Clean up test files
            try
            {
                if (Directory.Exists(App.ResultsDir))
                {
                    Directory.Delete(App.ResultsDir, true);
                }
            }
            catch { }
        }
    }
}