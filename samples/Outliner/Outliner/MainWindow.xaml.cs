using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Outliner.Services;

namespace Outliner
{
    /// <summary>
    /// Hosts the NavigationView shell and routes selection to the appropriate page
    /// (Single today, Batch and Settings in following commits). Initial page is
    /// chosen from OutlinerPreferences.StartupMode.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            var prefs = Ioc.Default.GetService<OutlinerPreferences>() ?? new OutlinerPreferences();
            var startupTag = string.IsNullOrWhiteSpace(prefs.StartupMode) ? "Single" : prefs.StartupMode;

            // Select the menu item matching the user's preferred startup mode;
            // fall back to the first item if it isn't present yet.
            foreach (var item in NavView.MenuItems)
            {
                if (item is NavigationViewItem nvi && nvi.Tag is string tag && tag == startupTag)
                {
                    NavView.SelectedItem = nvi;
                    return;
                }
            }
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer is not NavigationViewItem item)
                return;

            switch (item.Tag as string)
            {
                case "Single":
                    ContentFrame.Navigate(typeof(ContentPage));
                    break;
            }
        }
    }
}
