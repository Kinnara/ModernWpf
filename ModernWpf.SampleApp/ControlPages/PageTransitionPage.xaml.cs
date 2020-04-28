using SamplesCommon;
using System.Windows;
using System.Windows.Navigation;
using SamplePages = SamplesCommon.SamplePages;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class PageTransitionPage
    {
        public PageTransitionPage()
        {
            InitializeComponent();

            ContentFrame.NavigateToType(typeof(SamplePages.SamplePage1));
        }

        private void NavigateForward(object sender, RoutedEventArgs e)
        {
            var pageToNavigateTo = ContentFrame.BackStackDepth() % 2 == 1 ? typeof(SamplePages.SamplePage1) : typeof(SamplePages.SamplePage2);
            ContentFrame.NavigateToType(pageToNavigateTo);
        }

        private void NavigateBackward(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.BackStackDepth() > 0)
            {
                ContentFrame.GoBack();
            }
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            var page = (FrameworkElement)e.Content;
            page.Margin = new Thickness(-18, 0, -18, 0);
        }

        private void DefaultRB_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.ClearValue(StyleProperty);
        }

        private void DrillRB_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.Style = (Style)Resources["DrillFrameStyle"];
        }

        private void SuppressRB_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.Style = (Style)Resources["SuppressFrameStyle"];
        }

        private void SlideFromRightRB_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.Style = (Style)Resources["SlideFromRightFrameStyle"];
        }

        private void SlideFromLeftRB_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.Style = (Style)Resources["SlideFromLeftFrameStyle"];
        }
    }
}
