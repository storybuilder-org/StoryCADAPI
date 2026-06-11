using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using StoryCADLib.Services.IoC;

namespace StoryCADCritter;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }
    public static IntPtr MainWindowHandle { get; private set; }

    public App()
    {
        // Register user preferences before BootStrapper builds the provider, so
        // view models can resolve them via Ioc.Default.
        var prefsService = new PreferencesService();
        BootStrapper.Services.AddSingleton(prefsService);
        BootStrapper.Services.AddSingleton(prefsService.Load());

        BootStrapper.Initialise();
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window
        {
            Title = "StoryCAD Critter"
        };

#if WINDOWS
        MainWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
#endif

        MainWindow.Content = new MainPage();
        MainWindow.Activate();
    }
}
