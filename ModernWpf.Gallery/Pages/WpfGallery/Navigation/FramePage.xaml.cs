using System.Windows;
using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public sealed partial class FramePage : UserControl
    {
        public FramePage()
        {
            ViewModel = new WpfGalleryPageViewModel("Frame", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }

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
