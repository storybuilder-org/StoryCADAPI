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
        }
    }
}
