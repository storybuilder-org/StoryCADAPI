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

            // Navigate to the user's preferred startup page and reflect that
            // selection in the nav pane.
            foreach (var item in NavView.MenuItems)
            {
                if (item is NavigationViewItem nvi && nvi.Tag is string tag && tag == startupTag)
                {
                    NavView.SelectedItem = nvi;
                    NavigateToTag(tag);
                    return;
                }
            }

            if (NavView.MenuItems.Count > 0 && NavView.MenuItems[0] is NavigationViewItem first)
            {
                NavView.SelectedItem = first;
                NavigateToTag(first.Tag as string);
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer is NavigationViewItem item)
                NavigateToTag(item.Tag as string);
        }

        private void NavigateToTag(string? tag)
        {
            switch (tag)
            {
                case "Single":
                    ContentFrame.Navigate(typeof(ContentPage));
                    break;
                case "Batch":
                    ContentFrame.Navigate(typeof(BatchPage));
                    break;
                case "Settings":
                    ContentFrame.Navigate(typeof(SettingsPage));
                    break;
            }
        }
    }
}
