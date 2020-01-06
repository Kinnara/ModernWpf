using SamplesCommon;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class PageTransitionsPage
    {
        public PageTransitionsPage()
        {
            InitializeComponent();

            frame.Navigate(SamplePageSources.SamplePage1);
        }

        private void NavigateForward(object sender, RoutedEventArgs e)
        {
            var pageToNavigateTo = frame.BackStack.Cast<JournalEntry>().Count() % 2 == 1 ? SamplePageSources.SamplePage1 : SamplePageSources.SamplePage2;
            frame.Navigate(pageToNavigateTo);
        }

        private void NavigateBackward(object sender, RoutedEventArgs e)
        {
            frame.GoBack();
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            var page = (FrameworkElement)e.Content;
            page.Margin = new Thickness(-18, 0, -18, 0);
        }

        private void DefaultRB_Checked(object sender, RoutedEventArgs e)
        {
            frame.ClearValue(StyleProperty);
        }

        private void DrillRB_Checked(object sender, RoutedEventArgs e)
        {
            frame.Style = (Style)Resources["DrillFrameStyle"];
        }

        private void SuppressRB_Checked(object sender, RoutedEventArgs e)
        {
            frame.Style = (Style)Resources["SuppressFrameStyle"];
        }

        private void SlideFromRightRB_Checked(object sender, RoutedEventArgs e)
        {
            frame.Style = (Style)Resources["SlideFromRightFrameStyle"];
        }

        private void SlideFromLeftRB_Checked(object sender, RoutedEventArgs e)
        {
            frame.Style = (Style)Resources["SlideFromLeftFrameStyle"];
        }
    }
}
