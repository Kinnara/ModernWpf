using ModernWpf.Controls;
using System.Windows.Navigation;

namespace SamplesCommon
{
    public class SampleFrame : ThemeAwareFrame
    {
        private object _oldContent;

        public SampleFrame()
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
            (Content as SamplePage)?.InternalOnNavigatingFrom(e);
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (_oldContent != null)
            {
                (_oldContent as SamplePage)?.InternalOnNavigatedFrom(e);
                _oldContent = null;
            }

            (e.Content as SamplePage)?.InternalOnNavigatedTo(e);
        }
    }
}
