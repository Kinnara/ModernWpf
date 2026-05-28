using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public partial class IconographyPage : Page
    {
        static IconographyPage()
        {
            CommandManager.RegisterClassCommandBinding(typeof(IconographyPage), new CommandBinding(ApplicationCommands.Copy, CopyContent));
        }

        public IconographyPage(IconographyPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
        }

        public IconographyPageViewModel ViewModel { get; }

        private static void CopyContent(object sender, ExecutedRoutedEventArgs e)
        {
            var text = e.Parameter as string;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error copying to clipboard: " + ex.Message);
            }
        }

        private void IconsSearchBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UpdateSearchPlaceholder();
        }

        private void IconsSearchBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UpdateSearchPlaceholder();
        }

        private void IconsSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSearchPlaceholder();
        }

        private void UpdateSearchPlaceholder()
        {
            IconsSearchBoxPlaceholder.Visibility = IconsSearchBox.Text.Length > 0
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void Open_SegoeFontDownloadPage(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://learn.microsoft.com/windows/apps/design/downloads/#fonts") { UseShellExecute = true });
        }

        private void Open_IconDesignGuidelinesPage(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://learn.microsoft.com/windows/apps/design/style/segoe-fluent-icons-font#layering-and-mirroring") { UseShellExecute = true });
        }
    }
}
