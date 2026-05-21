using Microsoft.UI.Xaml.Controls;

namespace StoryCADCritter;

public sealed partial class MainPage : Page
{
    public MainPageViewModel Vm { get; }

    public MainPage()
    {
        Vm = new MainPageViewModel();
        InitializeComponent();
    }

    private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LogScrollViewer.UpdateLayout();
        LogScrollViewer.ChangeView(null, LogScrollViewer.ScrollableHeight, null);
    }
}
