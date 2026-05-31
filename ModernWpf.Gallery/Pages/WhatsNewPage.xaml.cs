using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages
{
    /// <summary>
    /// Interaction logic for WhatsNewPage.xaml
    /// </summary>
    public partial class WhatsNewPage : Page
    {
        public WhatsNewPageViewModel ViewModel { get; }

        public WhatsNewPage()
            : this(null)
        {
        }

        public WhatsNewPage(WhatsNewPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel ?? new WhatsNewPageViewModel(OnNavigateCard);
            DataContext = this;
        }

        public Action<string> ItemRequested { get; set; }

        private void Open_WhatsNewPageNET10(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://learn.microsoft.com/en-in/dotnet/desktop/wpf/whats-new/net100") { UseShellExecute = true });
        }

        private void Open_WhatsNewPageNET9(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://learn.microsoft.com/en-in/dotnet/desktop/wpf/whats-new/net90") { UseShellExecute = true });
        }

        private void Open_MessageBoxAPISpecNET10(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/dotnet/wpf/issues/9613") { UseShellExecute = true });
        }

        private void NavigateToMessageBoxSample(object sender, RoutedEventArgs e)
        {
            ViewModel.Navigate("MessageBox");
        }

        private void Open_UsingFluentInWPFPage(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://aka.ms/wpf-fluentdoc") { UseShellExecute = true });
        }

        private void OnNavigateCard(object parameter)
        {
            if (parameter is string uniqueId)
            {
                ItemRequested?.Invoke(uniqueId);
            }
        }
    }
}
