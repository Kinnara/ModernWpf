using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public sealed partial class ColorPage : UserControl
    {
        public ColorPage()
        {
            ViewModel = new ColorsPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public ColorsPageViewModel ViewModel { get; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PageSelector.SelectedIndex = 0;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorSubpageNavigationFrame.Content = WpfGalleryColorSectionFactory.Create(PageSelector.SelectedIndex);
        }
    }
}
