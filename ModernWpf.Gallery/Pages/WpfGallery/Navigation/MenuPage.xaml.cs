using System.Windows;
using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public sealed partial class MenuPage : UserControl
    {
        public MenuPage()
        {
            ViewModel = new WpfGalleryPageViewModel("Menu", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            if (e.OriginalSource is MenuItem originalMenuItem && originalMenuItem == menuItem)
            {
                StatusMenuItem.Visibility = Visibility.Visible;
                StatusMenuItem.Text = menuItem.Tag != null ? "You pressed " + menuItem.Tag : "You pressed " + menuItem.Header;
            }

            var parentMenuItem = menuItem.Parent as MenuItem;
            if (parentMenuItem != null)
            {
                parentMenuItem.Focus();
            }
            else
            {
                menuItem.Focus();
            }
        }
    }
}
