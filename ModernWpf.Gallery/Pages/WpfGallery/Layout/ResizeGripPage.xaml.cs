using System.Windows;
using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class ResizeGripPage : UserControl
    {
        public ResizeGripPage()
        {
            ViewModel = new WpfGalleryPageViewModel("ResizeGrip", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }

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
