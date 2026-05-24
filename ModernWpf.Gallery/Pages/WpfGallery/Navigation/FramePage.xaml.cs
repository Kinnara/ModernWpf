using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public sealed partial class FramePage : Page
    {
        public FramePage(FramePageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public FramePageViewModel ViewModel { get; }

        private void OpenFrameWindow_Click(object sender, RoutedEventArgs e)
        {
            FrameWindow window = new FrameWindow();
            window.Show();
        }
    }
}
