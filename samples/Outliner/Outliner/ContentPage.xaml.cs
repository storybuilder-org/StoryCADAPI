using Microsoft.UI.Xaml.Controls;
// ContentPageViewModel is now in the Outliner namespace (same project)

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Outliner
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContentPage : Page
    {
        public ContentPageViewModel PageVm { get; }

        public ContentPage()
        {
            PageVm = new ContentPageViewModel
            {
                WindowHandle = App.MWindowHandle
            };
            this.InitializeComponent();
            DataContext = PageVm;
        }
    }
}
