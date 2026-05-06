using Microsoft.UI.Xaml.Controls;

namespace Outliner
{
    public sealed partial class BatchPage : Page
    {
        public BatchPageViewModel PageVm { get; }

        public BatchPage()
        {
            PageVm = new BatchPageViewModel();
            this.InitializeComponent();
            DataContext = PageVm;
        }
    }
}
