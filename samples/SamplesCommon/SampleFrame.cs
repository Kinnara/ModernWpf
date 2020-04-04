using ModernWpf.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace SamplesCommon
{
    public class SampleFrame : TransitionFrame
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
            bool firstNavigation = _oldContent == null;

            if (_oldContent != null)
            {
                (_oldContent as SamplePage)?.InternalOnNavigatedFrom(e);
                _oldContent = null;
            }

            (e.Content as SamplePage)?.InternalOnNavigatedTo(e);

            if (!firstNavigation && e.Content is UIElement element)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    element.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }, DispatcherPriority.Loaded);
            }
        }
    }
}
