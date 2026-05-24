using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ModernWpf.Gallery.Testing;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public sealed partial class ColorPage : Page
    {
        public ColorPage(ColorsPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
        }

        public ColorsPageViewModel ViewModel { get; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PageSelector.SelectedItem = ResolveInitialSubpage();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var section = WpfGalleryColorSectionFactory.Create(PageSelector.SelectedIndex);
            section.SetResourceReference(TextElement.FontSizeProperty, "BodyTextBlockFontSize");
            ColorSubpageNavigationFrame.Navigate(section);
        }

        private object ResolveInitialSubpage()
        {
            if (GalleryDiagnostics.IsEnabled && !string.IsNullOrWhiteSpace(GalleryDiagnostics.ColorSubpage))
            {
                foreach (var item in PageSelector.Items)
                {
                    if (string.Equals(item as string, GalleryDiagnostics.ColorSubpage, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return item;
                    }
                }
            }

            return PageSelector.Items[0];
        }
    }
}
