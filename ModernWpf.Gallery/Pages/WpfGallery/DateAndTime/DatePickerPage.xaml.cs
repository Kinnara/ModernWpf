using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.DateAndTime
{
    public sealed partial class DatePickerPage : UserControl
    {
        public DatePickerPage()
        {
            ViewModel = new DatePickerPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public DatePickerPageViewModel ViewModel { get; }
    }
}
