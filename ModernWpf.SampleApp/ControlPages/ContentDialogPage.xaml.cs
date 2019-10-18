using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// Interaction logic for ContentDialogPage.xaml
    /// </summary>
    public partial class ContentDialogPage : UserControl
    {
        private ContentDialog _dialog = new TestContentDialog { Title = "Good :)" };
        private bool _dialogShowing;

        public ContentDialogPage()
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
            this.InvertTheme();
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
    }
}
