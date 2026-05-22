using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public sealed partial class TypographyPage : UserControl
    {
        public TypographyPage()
        {
            ViewModel = new TypographyPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public TypographyPageViewModel ViewModel { get; }
    }
}
