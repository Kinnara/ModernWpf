using ModernWpf.Controls;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ContentDialogTestContent : UserControl
    {
        private readonly ContentDialog _dialog = new TestContentDialog();

        public ContentDialogTestContent()
        {
            InitializeComponent();

            var dialog = ParentedDialog;
            dialog.Opened += Dialog_Opened;
            dialog.Closing += Dialog_Closing;
            dialog.Closed += Dialog_Closed;
            dialog.PrimaryButtonClick += Dialog_PrimaryButtonClick;
            dialog.SecondaryButtonClick += Dialog_SecondaryButtonClick;
            dialog.CloseButtonClick += Dialog_CloseButtonClick;
        }

        private void Dialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            WriteCallerName();
        }

        private void Dialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            WriteCallerName();
        }

        private void Dialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            WriteCallerName();
        }

        private void Dialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            WriteCallerName();
            args.Cancel = true;
            this.ToggleTheme();
        }

        private void Dialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            WriteCallerName();
        }

        private void Dialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            WriteCallerName();
        }

        private void WriteCallerName([CallerMemberName] string name = "")
        {
            //Debug.WriteLine(name);
        }

        private void ShowDialog(object sender, RoutedEventArgs e)
        {
            _dialog.Title = ((Button)sender).Content;
            _dialog.ShowAsync(ContentDialogPlacement.Popup);
        }

        private void ShowParentedDialogPopup(object sender, RoutedEventArgs e)
        {
            ParentedDialog.Title = ((Button)sender).Content;
            ParentedDialog.ShowAsync(ContentDialogPlacement.Popup);
        }

        private void ShowParentedDialogInPlace(object sender, RoutedEventArgs e)
        {
            ParentedDialog.Title = ((Button)sender).Content;
            ParentedDialog.ShowAsync(ContentDialogPlacement.InPlace);
        }

        private void OpenNewWindow(object sender, RoutedEventArgs e)
        {
            var window = new ContentDialogTestWindow();
            ThemeManager.SetRequestedTheme(window, ThemeManager.GetActualTheme(this));
            window.Show();
        }
    }
}
