using System.Windows;
using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public sealed partial class ColorPage : UserControl
    {
        public ColorPage()
        {
            ViewModel = new WpfGalleryPageViewModel("Colors", "Guide showing how to use colors in your app");
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PageSelector.SelectedIndex = 0;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorSubpageNavigationFrame.Content = WpfGalleryExampleFactory.CreateColorSection(PageSelector.SelectedIndex);
        }
    }
}
