using ModernWpf.Controls;
using System.Windows.Navigation;

namespace ModernWpfTestApp
{
    public class TestFrame : ThemeAwareFrame
    {
        public TestFrame()
        {
            Navigating += OnNavigating;
            Navigated += OnNavigated;
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            (e.Content as TestPage)?.OnNavigatingFrom(e);
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            (e.Content as TestPage)?.OnNavigatedTo(e);
        }
    }
}
