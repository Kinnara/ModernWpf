using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.DateAndTime
{
    public partial class CalendarPage : Page
    {
        public CalendarPageViewModel ViewModel { get; }

        public CalendarPage(CalendarPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
