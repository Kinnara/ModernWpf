using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using ModernWpf.Gallery.Testing;

namespace ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo
{
    /// <summary>
    /// Interaction logic for ToolTipPage.xaml
    /// </summary>
    public partial class ToolTipPage : Page
    {
        private readonly DispatcherTimer _visualTestToolTipCloseTimer;

        public ToolTipPageViewModel ViewModel { get; }

        public ToolTipPage(ToolTipPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            _visualTestToolTipCloseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1800)
            };
            _visualTestToolTipCloseTimer.Tick += (sender, args) =>
            {
                _visualTestToolTipCloseTimer.Stop();
                SimpleToolTip.IsOpen = false;
            };
        }

        private void ToolTipButton_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            OpenSimpleToolTip(sender as FrameworkElement);
        }

        private void ToolTipButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSimpleToolTip(sender as FrameworkElement);
        }

        private void ToolTipButton_MouseEnter(object sender, MouseEventArgs e)
        {
            OpenSimpleToolTip(sender as FrameworkElement);
        }

        private void ToolTipButton_MouseMove(object sender, MouseEventArgs e)
        {
            OpenSimpleToolTip(sender as FrameworkElement);
        }

        private void OpenSimpleToolTip(FrameworkElement placementTarget)
        {
            if (!GalleryDiagnostics.IsEnabled)
            {
                return;
            }

            if (placementTarget == null)
            {
                return;
            }

            SimpleToolTip.PlacementTarget = placementTarget;
            SimpleToolTip.Placement = PlacementMode.Bottom;
            SimpleToolTip.VerticalOffset = 4;
            SimpleToolTip.IsOpen = true;
            _visualTestToolTipCloseTimer.Stop();
            _visualTestToolTipCloseTimer.Start();
        }
    }
}
