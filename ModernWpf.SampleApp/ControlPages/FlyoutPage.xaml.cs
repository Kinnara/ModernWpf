using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.Windows;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class FlyoutPage : Page
    {
        public FlyoutPage()
        {
            InitializeComponent();
        }

        private void DeleteConfirmation_Click(object sender, RoutedEventArgs e)
        {
            Flyout f = FlyoutService.GetFlyout(Control1) as Flyout;
            if (f != null)
            {
                f.Hide();
            }
        }
    }
}
