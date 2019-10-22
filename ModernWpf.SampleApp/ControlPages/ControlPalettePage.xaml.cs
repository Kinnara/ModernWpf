using System;
using System.Collections.Generic;
using System.Linq;
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
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ContentScrollViewer.Visibility = Visibility.Visible;
            }), DispatcherPriority.ContextIdle);
        }
    }
}
