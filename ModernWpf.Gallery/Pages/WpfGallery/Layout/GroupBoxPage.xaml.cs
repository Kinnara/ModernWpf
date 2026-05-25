using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public partial class GroupBoxPage : Page
    {
        public GroupBoxPage(GroupBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public GroupBoxPageViewModel ViewModel { get; }
    }
}
