using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.DateAndTime
{
    public partial class DatePickerPage : Page
    {
        public DatePickerPage(DatePickerPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public DatePickerPageViewModel ViewModel { get; }
    }
}
