using System.Windows;
using System.Windows.Threading;

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// Interaction logic for ControlPalettePage.xaml
    /// </summary>
    public partial class ControlPalettePage
    {
        public ControlPalettePage()
        {
            InitializeComponent();

            ContentScrollViewer.Visibility = Visibility.Collapsed;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                ContentScrollViewer.Visibility = Visibility.Visible;
            }, DispatcherPriority.ContextIdle);
        }
    }
}
