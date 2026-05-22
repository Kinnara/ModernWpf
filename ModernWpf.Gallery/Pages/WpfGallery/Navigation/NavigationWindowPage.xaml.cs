using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public sealed partial class NavigationWindowPage : UserControl
    {
        public NavigationWindowPage()
        {
            ViewModel = new WpfGalleryPageViewModel("Navigation Window", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }

        private void OpenNavigationWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new NavigationWindow
            {
                Width = 800,
                Height = 450,
                Source = new Uri("/Pages/WpfGallery/Navigation/Page1.xaml", UriKind.Relative)
            };
            var owner = Window.GetWindow(this);
            if (owner != null)
            {
                window.Owner = owner;
            }

            window.Show();
        }
    }
}
