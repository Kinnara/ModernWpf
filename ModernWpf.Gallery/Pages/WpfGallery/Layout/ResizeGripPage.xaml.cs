using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public partial class ResizeGripPage : Page
    {
        public ResizeGripPage(ResizeGripPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public ResizeGripPageViewModel ViewModel { get; }

        private void OpenResizeGripWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                Width = 500,
                Height = 300,
                ResizeMode = ResizeMode.CanResizeWithGrip,
                Content = new TextBlock
                {
                    Text = "ResizeGrip is present at the bottom right corner of the window",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16
                }
            };
            window.Show();
        }
    }
}
