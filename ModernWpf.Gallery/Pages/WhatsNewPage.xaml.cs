using System;
using System.Diagnostics;
using System.Windows;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class WhatsNewPage
    {
        public WhatsNewPage()
        {
            ViewModel = new WhatsNewPageViewModel(OnNavigateCard);
            InitializeComponent();
            GalleryAutomation.SetHeadingLevel(TitleLabel, GalleryAutomationHeadingLevel.Level1);
            GalleryAutomation.SetHeadingLevel(DescriptionLabel, GalleryAutomationHeadingLevel.Level2);
            DataContext = this;
        }

        public Action<string> ItemRequested { get; set; }
        public WhatsNewPageViewModel ViewModel { get; }

        private void OnMessageBoxSampleClick(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigateCommand.Execute("MessageBox");
        }

        private void OnMessageBoxSpecClick(object sender, RoutedEventArgs e)
        {
            OpenUri("https://github.com/dotnet/wpf/issues/9542");
        }

        private void OnWhatsNewNet10Click(object sender, RoutedEventArgs e)
        {
            OpenUri("https://learn.microsoft.com/dotnet/desktop/wpf/whats-new/net100");
        }

        private void OnWhatsNewNet9Click(object sender, RoutedEventArgs e)
        {
            OpenUri("https://learn.microsoft.com/dotnet/desktop/wpf/whats-new/net90");
        }

        private void OnUsingFluentInWpfClick(object sender, RoutedEventArgs e)
        {
            OpenUri("https://learn.microsoft.com/dotnet/desktop/wpf/whats-new/net90#using-fluent-theme-in-wpf-in-net-9");
        }

        private static void OpenUri(string uri)
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
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
