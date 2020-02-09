using ModernWpf.Controls;
using System.Windows.Navigation;

namespace MUXControlsTestApp
{
    public class TestFrame : ThemeAwareFrame
    {
        private object _oldContent;

        public TestFrame()
        {
            Navigating += OnNavigating;
            Navigated += OnNavigated;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            _oldContent = oldContent;
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            (Content as IPage)?.InternalOnNavigatingFrom(e);
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (_oldContent != null)
            {
                (_oldContent as IPage)?.InternalOnNavigatedFrom(e);
                _oldContent = null;
            }

            (e.Content as IPage)?.InternalOnNavigatedTo(e);
        }
    }
}
