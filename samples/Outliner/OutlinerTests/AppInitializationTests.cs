using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADLib.Models;
using StoryCADLib.Services.API;

namespace OutlinerTests
{
    /// <summary>
    /// Tests for application initialization and dependency injection setup.
    /// </summary>
    [TestClass]
    public class AppInitializationTests
    {
        [TestMethod]
        public void DependencyInjection_IsProperlyConfigured()
        {
            // Assert - Verify IoC container is configured
            Assert.IsNotNull(Ioc.Default, "IoC container should be initialized");
        }

        [TestMethod]
        public void AppState_CanBeResolvedFromDI()
        {
            // Act
            var appState = Ioc.Default.GetService<AppState>();

            // Assert
            Assert.IsNotNull(appState, "AppState should be registered in DI container");
            Assert.IsTrue(appState.Headless, "AppState should be configured for headless testing");
        }

        [TestMethod]
        public void AppState_IsSingleton()
        {
            // Act
            var appState1 = Ioc.Default.GetService<AppState>();
            var appState2 = Ioc.Default.GetService<AppState>();

            // Assert
            Assert.IsNotNull(appState1);
            Assert.IsNotNull(appState2);
            Assert.AreSame(appState1, appState2, "AppState should be registered as singleton");
        }

        [TestMethod]
        public void StoryCADApi_CanBeResolvedFromDI()
        {
            // Act
            var api = Ioc.Default.GetService<StoryCADApi>();

            // Assert
            Assert.IsNotNull(api, "StoryCADApi should be registered in DI container");
        }

        [TestMethod]
        public void StoryCADApi_IsSingleton()
        {
            // Act
            var api1 = Ioc.Default.GetService<StoryCADApi>();
            var api2 = Ioc.Default.GetService<StoryCADApi>();

            // Assert
            Assert.IsNotNull(api1);
            Assert.IsNotNull(api2);
            Assert.AreSame(api1, api2, "StoryCADApi should be registered as singleton");
        }

        [TestMethod]
        public void GetRequiredService_AppState_DoesNotThrow()
        {
            // Act & Assert - Should not throw
            var appState = Ioc.Default.GetRequiredService<AppState>();
            Assert.IsNotNull(appState);
        }

        [TestMethod]
        public void GetRequiredService_StoryCADApi_DoesNotThrow()
        {
            // Act & Assert - Should not throw
            var api = Ioc.Default.GetRequiredService<StoryCADApi>();
            Assert.IsNotNull(api);
        }

        [TestMethod]
        public void TestDirectories_AreCreated()
        {
            // Assert
            Assert.IsTrue(System.IO.Directory.Exists(App.InputDir),
                $"Test input directory should exist at {App.InputDir}");
            Assert.IsTrue(System.IO.Directory.Exists(App.ResultsDir),
                $"Test results directory should exist at {App.ResultsDir}");
        }
    }
}