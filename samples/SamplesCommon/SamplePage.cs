using System.Windows.Controls;
using System.Windows.Navigation;

namespace SamplesCommon
{
    public class SamplePage : Page
    {
        protected virtual void OnNavigatedTo(NavigationEventArgs e) { }
        protected virtual void OnNavigatedFrom(NavigationEventArgs e) { }
        protected virtual void OnNavigatingFrom(NavigatingCancelEventArgs e) { }

        internal void InternalOnNavigatedTo(NavigationEventArgs e) => OnNavigatedTo(e);
        internal void InternalOnNavigatedFrom(NavigationEventArgs e) => OnNavigatedFrom(e);
        internal void InternalOnNavigatingFrom(NavigatingCancelEventArgs e) => OnNavigatingFrom(e);
    }
}
