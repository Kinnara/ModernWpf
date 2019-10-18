using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// Interaction logic for HyperlinkButtonPage.xaml
    /// </summary>
    public partial class HyperlinkButtonPage : Page
    {
        public HyperlinkButtonPage()
        {
            InitializeComponent();
        }

        private void GoToHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("ControlPages/ListBoxPage.xaml", UriKind.Relative));
        }
    }
}
