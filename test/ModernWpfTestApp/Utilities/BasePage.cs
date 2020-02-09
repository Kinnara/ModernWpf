using System.Windows.Navigation;

namespace MUXControlsTestApp
{
    internal interface IPage
    {
        void InternalOnNavigatedTo(NavigationEventArgs e);
        void InternalOnNavigatedFrom(NavigationEventArgs e);
        void InternalOnNavigatingFrom(NavigatingCancelEventArgs e);
    }

    public class BasePage : System.Windows.Controls.Page, IPage
    {
        public NavigationService Frame => NavigationService;

        protected virtual void OnNavigatedTo(NavigationEventArgs e) { }
        protected virtual void OnNavigatedFrom(NavigationEventArgs e) { }
        protected virtual void OnNavigatingFrom(NavigatingCancelEventArgs e) { }

        void IPage.InternalOnNavigatedTo(NavigationEventArgs e) => OnNavigatedTo(e);
        void IPage.InternalOnNavigatedFrom(NavigationEventArgs e) => OnNavigatedFrom(e);
        void IPage.InternalOnNavigatingFrom(NavigatingCancelEventArgs e) => OnNavigatingFrom(e);
    }
}
