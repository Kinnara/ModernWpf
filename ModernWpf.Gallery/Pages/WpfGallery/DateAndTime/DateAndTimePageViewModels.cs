using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.DateAndTime
{
    public sealed class CalendarPageViewModel : WpfGalleryPageViewModel
    {
        public CalendarPageViewModel()
            : base("Calendar", string.Empty)
        {
        }
    }

    public sealed class DatePickerPageViewModel : WpfGalleryPageViewModel
    {
        public DatePickerPageViewModel()
            : base("DatePicker", string.Empty)
        {
        }
    }
}
