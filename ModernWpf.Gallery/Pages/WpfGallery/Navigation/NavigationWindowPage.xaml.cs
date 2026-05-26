using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public partial class NavigationWindowPage : Page
    {
        public NavigationWindowPage(NavigationWindowPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public NavigationWindowPageViewModel ViewModel { get; }

        private void OpenNavigationWindow_Click(object sender, RoutedEventArgs e)
        {
            NavigationWindow window = new NavigationWindow()
            {
                Width = 800,
                Height = 450,
                Source = new Uri("/Pages/WpfGallery/Navigation/Page1.xaml", UriKind.Relative)
            };
            window.Show();
        }
    }
}
