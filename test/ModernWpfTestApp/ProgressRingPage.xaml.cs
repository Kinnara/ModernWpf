// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MUXControlsTestApp
{
    [TopLevelTestPage(Name = "ProgressRing")]
    public sealed partial class ProgressRingPage : TestPage
    {
        public ProgressRingPage()
        {
            this.InitializeComponent();
            Loaded += ProgressRingPage_Loaded;

            //NavigateToCustomLottieSourcePage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(ProgressRingCustomLottieSourcePage), 0); };
            NavigateToStoryboardAnimationPage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(ProgressRingStoryboardAnimationPage), 0); };
        }

        private void ProgressRingPage_Loaded(object sender, RoutedEventArgs e)
        {
            var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(TestProgressRing, 0);

            var commonStatesGroup = (VisualStateGroup)VisualStateManager.GetVisualStateGroups(layoutRoot)[0];
            commonStatesGroup.CurrentStateChanged += this.ProgressRingPage_CurrentStateChanged;
            VisualStateText.Text = commonStatesGroup.CurrentState.Name;
            foreach (var state in commonStatesGroup.States.Cast<VisualState>().Where(s => s.Storyboard != null))
            {
                // Change the animation to 0 duration to avoid timing issues in the test.
                state.Storyboard.Children[0].Duration = new Duration(TimeSpan.FromSeconds(0));
            }

            //var animatedVisualPlayer = (ModernWpf.Controls.AnimatedVisualPlayer)VisualTreeHelper.GetChild(layoutRoot, 0);

            //IsPlayingText.Text = animatedVisualPlayer.IsPlaying.ToString();
            OpacityText.Text = layoutRoot.Opacity.ToString();

            Loaded -= ProgressRingPage_Loaded;
        }

        private void ProgressRingPage_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            VisualStateText.Text = e.NewState.Name;

            var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(TestProgressRing, 0);
            //var animatedVisualPlayer = (ModernWpf.Controls.AnimatedVisualPlayer)VisualTreeHelper.GetChild(layoutRoot, 0);
            //IsPlayingText.Text = animatedVisualPlayer.IsPlaying.ToString();
            OpacityText.Text = layoutRoot.Opacity.ToString();
        }

        public void UpdateWidth_Click(object sender, RoutedEventArgs e)
        {
            TestProgressRing.Width = String.IsNullOrEmpty(WidthInput.Text) ? Double.Parse(WidthInput.PlaceholderText) : Double.Parse(WidthInput.Text);
            TestProgressRing.Height = String.IsNullOrEmpty(WidthInput.Text) ? Double.Parse(WidthInput.PlaceholderText) : Double.Parse(WidthInput.Text);
        }
    }
}
