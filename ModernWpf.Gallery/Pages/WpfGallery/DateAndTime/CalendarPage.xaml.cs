using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.DateAndTime
{
    public sealed partial class CalendarPage : UserControl
    {
        public CalendarPage()
        {
            ViewModel = new CalendarPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public CalendarPageViewModel ViewModel { get; }
    }
}
