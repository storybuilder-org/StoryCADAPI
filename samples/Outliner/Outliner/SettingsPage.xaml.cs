using Microsoft.UI.Xaml.Controls;

namespace Outliner
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPageViewModel PageVm { get; }

        public SettingsPage()
        {
            PageVm = new SettingsPageViewModel();
            this.InitializeComponent();
            DataContext = PageVm;
            // Fire-and-forget: populates the model picker from /v1/models in
            // the background; the priced-models seed is shown until it returns.
            _ = PageVm.LoadModelsAsync();
        }
    }
}
