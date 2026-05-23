using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public sealed partial class ColorPage : Page
    {
        public ColorPage(ColorsPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public ColorsPageViewModel ViewModel { get; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PageSelector.SelectedItem = PageSelector.Items[0];
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorSubpageNavigationFrame.Navigate(WpfGalleryColorSectionFactory.Create(PageSelector.SelectedIndex));
        }
    }
}
