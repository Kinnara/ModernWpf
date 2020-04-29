using ModernWpf.Media.Animation;
using System.Windows;
using System.Windows.Controls;
using Page = ModernWpf.Controls.Page;
using SamplePages = SamplesCommon.SamplePages;

namespace ModernWpf.SampleApp.ControlPages
{
    public sealed partial class PageTransitionPage : Page
    {
        private NavigationTransitionInfo _transitionInfo = null;

        public PageTransitionPage()
        {
            InitializeComponent();

            ContentFrame.Navigate(typeof(SamplePages.SamplePage1));
        }

        private void ForwardButton1_Click(object sender, RoutedEventArgs e)
        {

            var pageToNavigateTo = ContentFrame.BackStackDepth % 2 == 1 ? typeof(SamplePages.SamplePage1) : typeof(SamplePages.SamplePage2);

            if (_transitionInfo == null)
            {
                // Default behavior, no transition set or used.
                ContentFrame.Navigate(pageToNavigateTo, null);
            }
            else
            {
                // Explicit transition info used.
                ContentFrame.Navigate(pageToNavigateTo, null, _transitionInfo);
            }
        }

        private void BackwardButton1_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.BackStackDepth > 0)
            {
                ContentFrame.GoBack();
            }
        }

        private void TransitionRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var senderTransitionString = (sender as RadioButton).Content.ToString();
            if (senderTransitionString != "Default")
            {
                if (senderTransitionString == "Entrance")
                {
                    _transitionInfo = new EntranceNavigationTransitionInfo();
                }
                else if (senderTransitionString == "DrillIn")
                {
                    _transitionInfo = new DrillInNavigationTransitionInfo();
                }
                else if (senderTransitionString == "Suppress")
                {
                    _transitionInfo = new SuppressNavigationTransitionInfo();
                }
                else if (senderTransitionString == "Slide from Right")
                {
                    _transitionInfo = new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight };
                }
                else if (senderTransitionString == "Slide from Left")
                {
                    _transitionInfo = new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft };
                }
            }
            else
            {
                _transitionInfo = null;
            }
        }
    }
}
