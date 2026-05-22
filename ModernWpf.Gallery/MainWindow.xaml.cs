using System.Windows;
using ModernWpf.Gallery.Pages;

namespace ModernWpf.Gallery
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            GalleryAutomation.SetHeadingLevel(TitleText, GalleryAutomationHeadingLevel.Level1);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            RootPage.GoBack();
        }

        internal void SetBackButtonVisible(bool canGoBack)
        {
            BackButton.IsEnabled = canGoBack;
        }

        internal void NavigateTo(string uniqueId)
        {
            RootPage.NavigateTo(uniqueId);
        }
    }
}
