using Microsoft.UI.Xaml.Controls;

namespace StoryCADCritter;

public sealed partial class MainPage : Page
{
    public MainPageViewModel Vm { get; }

    public MainPage()
    {
        Vm = new MainPageViewModel();
        InitializeComponent();
        ConcurrencyBox.Value = Vm.MaxConcurrency;
    }

    private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LogScrollViewer.UpdateLayout();
        LogScrollViewer.ChangeView(null, LogScrollViewer.ScrollableHeight, null);
    }

    private void ConcurrencyBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        // NumberBox.Value is double and is NaN while the field is being edited;
        // only commit real numbers to the int-typed preference.
        if (!double.IsNaN(args.NewValue))
            Vm.MaxConcurrency = (int)args.NewValue;
    }
}
