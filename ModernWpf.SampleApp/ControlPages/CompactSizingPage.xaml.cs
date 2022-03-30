using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using ModernWpf.SampleApp.SamplePages;
using System.Threading.Tasks;
using System.Windows;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class CompactSizingPage : Page
    {
        public CompactSizingPage()
        {
            InitializeComponent();
        }

        private void Example1_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(SampleStandardSizingPage), null, new SuppressNavigationTransitionInfo());
        }

        private async void Standard_Checked(object sender, RoutedEventArgs e)
        {
            if (ContentFrame == null) { return; }

            var oldPage = ContentFrame.Content as SampleCompactSizingPage;

            ContentFrame.Navigate(typeof(SampleStandardSizingPage), null, new SuppressNavigationTransitionInfo());
            await Task.Delay(10);

            if (oldPage != null)
            {
                var page = ContentFrame.Content as SampleStandardSizingPage;
                page?.CopyState(oldPage);
            }
        }

        private async void Compact_Checked(object sender, RoutedEventArgs e)
        {
            if (ContentFrame == null) { return; }

            var oldPage = ContentFrame.Content as SampleStandardSizingPage;

            ContentFrame.Navigate(typeof(SampleCompactSizingPage), null, new SuppressNavigationTransitionInfo());
            await Task.Delay(10);

            if (oldPage != null)
            {
                var page = ContentFrame.Content as SampleCompactSizingPage;
                page?.CopyState(oldPage);
            }
        }
    }
}
