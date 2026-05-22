using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class GroupBoxPage : UserControl
    {
        public GroupBoxPage()
        {
            ViewModel = new GroupBoxPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public GroupBoxPageViewModel ViewModel { get; }
    }
}
