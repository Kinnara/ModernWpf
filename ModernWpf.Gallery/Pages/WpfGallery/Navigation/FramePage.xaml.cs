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
            var window = new FrameWindow();
            var owner = Window.GetWindow(this);
            if (owner != null)
            {
                window.Owner = owner;
            }

            window.Show();
        }
    }
}
