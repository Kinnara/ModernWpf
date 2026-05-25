using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.DateAndTime
{
    public partial class DatePickerPage : Page
    {
        public DatePickerPageViewModel ViewModel { get; }

        public DatePickerPage(DatePickerPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
