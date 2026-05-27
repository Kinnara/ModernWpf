using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.DateAndTime
{
    public class CalendarPageViewModel : WpfGalleryPageViewModel
    {
        public CalendarPageViewModel()
            : base("Calendar", string.Empty)
        {
        }
    }

    public class DatePickerPageViewModel : WpfGalleryPageViewModel
    {
        public DatePickerPageViewModel()
            : base("DatePicker", string.Empty)
        {
        }
    }
}
