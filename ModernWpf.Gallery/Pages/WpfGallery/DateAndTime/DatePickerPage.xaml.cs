using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.DateAndTime
{
    public sealed partial class DatePickerPage : UserControl
    {
        public DatePickerPage()
        {
            ViewModel = new WpfGalleryPageViewModel("DatePicker", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
