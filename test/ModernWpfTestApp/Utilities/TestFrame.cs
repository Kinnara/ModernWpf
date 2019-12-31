using ModernWpf.Controls;
using System.Windows.Navigation;

namespace ModernWpfTestApp
{
    public class TestFrame : ThemeAwareFrame
    {
        public TestFrame()
        {
            Navigating += OnNavigating;
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            (e.Content as TestPage)?.OnNavigatingFrom(e);
        }
    }
}
