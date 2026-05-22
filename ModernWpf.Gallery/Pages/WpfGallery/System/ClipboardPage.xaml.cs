using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ModernWpf.Gallery.Pages.WpfGallery.SystemPages
{
    public sealed partial class ClipboardPage : UserControl
    {
        public ClipboardPage()
        {
            ViewModel = new ClipboardPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public ClipboardPageViewModel ViewModel { get; }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            var text = CopyTextBox.Text;
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
                ViewModel.CopyStatus = "Copied \"" + text + "\" to clipboard!";
            }
            else
            {
                ViewModel.CopyStatus = "Nothing to copy - text box is empty.";
            }
        }

        private void PasteFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PastedText = Clipboard.ContainsText() ? Clipboard.GetText() : "(No text in clipboard)";
        }

        private void ClearClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.Clear();
            ViewModel.ClearStatus = "Clipboard cleared!";
            ViewModel.PastedText = string.Empty;
        }

        private void CheckFormats_Click(object sender, RoutedEventArgs e)
        {
            var formats = new StringBuilder();
            formats.AppendLine("Clipboard contains:");
            formats.AppendLine("  - Text: " + Clipboard.ContainsText());
            formats.AppendLine("  - Image: " + Clipboard.ContainsImage());
            formats.AppendLine("  - File Drop List: " + Clipboard.ContainsFileDropList());
            formats.AppendLine("  - Audio: " + Clipboard.ContainsAudio());
            ViewModel.FormatsInfo = formats.ToString();
        }

        private void CopyImageToClipboard_Click(object sender, RoutedEventArgs e)
        {
            var bitmapSource = SourceImage.Source as BitmapSource;
            if (bitmapSource != null)
            {
                Clipboard.SetImage(bitmapSource);
                ViewModel.CopyImageStatus = "Image copied to clipboard!";
            }
            else
            {
                ViewModel.CopyImageStatus = "Failed to copy image.";
            }
        }

        private void PasteImageFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                PastedImage.Source = image;
                PastedImage.Visibility = Visibility.Visible;
                ViewModel.PasteImageStatus = "Image pasted! Size: " + image.PixelWidth + "x" + image.PixelHeight;
            }
            else
            {
                PastedImage.Source = null;
                PastedImage.Visibility = Visibility.Hidden;
                ViewModel.PasteImageStatus = "No image in clipboard.";
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });

            e.Handled = true;
        }
    }
}
