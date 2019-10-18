using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// Interaction logic for ProgressThreadedPage.xaml
    /// </summary>
    public partial class ProgressThreadedPage : UserControl
    {
        public ProgressThreadedPage()
        {
            InitializeComponent();
        }

        private void BlockUIThread(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(4000);
        }
    }
}
