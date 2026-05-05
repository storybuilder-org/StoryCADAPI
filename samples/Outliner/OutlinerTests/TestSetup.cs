using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADLib.Services.IoC;
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
            BootStrapper.Initialise(headless: true);

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