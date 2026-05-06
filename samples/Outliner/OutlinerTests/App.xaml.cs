using System;
using System.IO;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using StoryCADLib.Models;

namespace OutlinerTests;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public static string InputDir  = Path.Combine(FindOutlinerProjectDir(), "OutlinerInput");
    public static string OutputDir = Path.Combine(FindOutlinerProjectDir(), "OutlinerOutput");

    /// <summary>
    /// Resolves the sibling Outliner project's directory, where the
    /// OutlinerInput / OutlinerOutput / OutlinerStories folders live.
    /// </summary>
    private static string FindOutlinerProjectDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "OutlinerTests.csproj")))
            dir = dir.Parent;
        if (dir == null)
            throw new InvalidOperationException(
                $"OutlinerTests.csproj not found walking up from {AppContext.BaseDirectory}");
        return Path.GetFullPath(Path.Combine(dir.FullName, "..", "Outliner"));
    }

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        // Initialize test environment
        TestSetup.Initialize(null);

        // Create test directories if they don't exist
        Directory.CreateDirectory(InputDir);
        Directory.CreateDirectory(OutputDir);

        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Run MSTest unit tests
        Microsoft.VisualStudio.TestPlatform.TestExecutor.UnitTestClient.Run(Environment.CommandLine);
    }
}