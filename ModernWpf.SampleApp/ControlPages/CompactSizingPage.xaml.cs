using ModernWpf.Media.Animation;
using SamplesCommon.SamplePages;
using System.Windows;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class CompactSizingPage
    {
        public CompactSizingPage()
        {
            InitializeComponent();
        }

        private void Example1_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(SampleStandardSizingPage), null, new SuppressNavigationTransitionInfo());
        }

        private void Standard_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(SampleStandardSizingPage), null, new SuppressNavigationTransitionInfo());
        }

        private void Compact_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(SampleCompactSizingPage), null, new SuppressNavigationTransitionInfo());
        }
    }
}
