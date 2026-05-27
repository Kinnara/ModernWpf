using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.DateAndTime
{
    public partial class CalendarPageViewModel : WpfGalleryPageViewModel
    {
        public CalendarPageViewModel()
            : base("Calendar", string.Empty)
        {
        }
    }

    public partial class DatePickerPageViewModel : WpfGalleryPageViewModel
    {
        public DatePickerPageViewModel()
            : base("DatePicker", string.Empty)
        {
        }
    }
}
