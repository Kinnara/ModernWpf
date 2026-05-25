using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.DateAndTime
{
    public partial class CalendarPage : Page
    {
        public CalendarPage(CalendarPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public CalendarPageViewModel ViewModel { get; }
    }
}
