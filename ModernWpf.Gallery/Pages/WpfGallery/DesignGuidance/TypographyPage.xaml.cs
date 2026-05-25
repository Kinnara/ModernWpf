using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public partial class TypographyPage : Page
    {
        public TypographyPage(TypographyPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
        }

        public TypographyPageViewModel ViewModel { get; }
    }
}
