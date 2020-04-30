using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MUXControlsTestApp
{
    public sealed partial class ProgressRingStoryboardAnimationPage : TestPage
    {
        public ProgressRingStoryboardAnimationPage()
        {
            this.InitializeComponent();
            Loaded += ProgressRingStoryboardAnimationPage_Loaded;

            NavigateToMainPage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(ProgressRingPage), 0); };
        }

        private void ProgressRingStoryboardAnimationPage_Loaded(object sender, RoutedEventArgs e)
        {
            var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(TestStoryboardAnimationProgressRing, 0);

            var commonStatesGroup = (VisualStateGroup)VisualStateManager.GetVisualStateGroups(layoutRoot)[1];
            commonStatesGroup.CurrentStateChanged += this.ProgressRingStoryboardAnimationPage_CurrentStateChanged;
            VisualStateText.Text = commonStatesGroup.CurrentState.Name;

            Loaded -= ProgressRingStoryboardAnimationPage_Loaded;
        }

        private void ProgressRingStoryboardAnimationPage_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            VisualStateText.Text = e.NewState.Name;
        }

        public void UpdateWidth_Click(object sender, RoutedEventArgs e)
        {
            TestStoryboardAnimationProgressRing.Width = String.IsNullOrEmpty(WidthInput.Text) ? Double.Parse(WidthInput.PlaceholderText) : Double.Parse(WidthInput.Text);
            TestStoryboardAnimationProgressRing.Height = String.IsNullOrEmpty(WidthInput.Text) ? Double.Parse(WidthInput.PlaceholderText) : Double.Parse(WidthInput.Text);
        }
    }
}
