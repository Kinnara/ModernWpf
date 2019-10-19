using ModernWpf.SampleApp.SamplePages;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class CompactSizingPage : UserControl
    {
        public CompactSizingPage()
        {
            InitializeComponent();
        }

        private void Example1_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new SampleStandardSizingPage());
        }

        private void Standard_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new SampleStandardSizingPage());
        }

        private void Compact_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new SampleCompactSizingPage());
        }
    }
}
