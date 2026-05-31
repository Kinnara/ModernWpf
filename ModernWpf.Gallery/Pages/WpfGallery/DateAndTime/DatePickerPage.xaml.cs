using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.DateAndTime
{
    /// <summary>
    /// Interaction logic for DatePickerPage.xaml
    /// </summary>
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
