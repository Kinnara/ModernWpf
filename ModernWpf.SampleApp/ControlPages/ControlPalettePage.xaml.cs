using System.Windows;
using System.Windows.Threading;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ControlPalettePage
    {
        public ControlPalettePage()
        {
            InitializeComponent();

            ContentScrollViewer.Visibility = Visibility.Collapsed;
            Loaded += OnLoaded;
        }

        ~ControlPalettePage()
        {
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
