using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.DateAndTime
{
    public sealed partial class CalendarPage : UserControl
    {
        public CalendarPage()
        {
            ViewModel = new WpfGalleryPageViewModel("Calendar", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
