using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public sealed partial class TypographyPage : Page
    {
        public TypographyPage(TypographyPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public TypographyPageViewModel ViewModel { get; }
    }
}
