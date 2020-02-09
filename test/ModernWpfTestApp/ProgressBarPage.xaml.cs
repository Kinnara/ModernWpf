// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf;
using ModernWpf.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MUXControlsTestApp
{
    [TopLevelTestPage(Name = "ProgressBar")]
    public sealed partial class ProgressBarPage : TestPage
    {
        public ProgressBarPage()
        {
            InitializeComponent();
            Loaded += ProgressBarPage_Loaded;
        }

        private void ProgressBarPage_Loaded(object sender, RoutedEventArgs e)
        {
            var layoutRoot = (Grid)VisualTreeHelper.GetChild(TestProgressBar, 0);

            ((VisualStateGroup)VisualStateManager.GetVisualStateGroups(layoutRoot)[0]).CurrentStateChanged += this.ProgressBarPage_CurrentStateChanged;
            VisualStateText.Text = ((VisualStateGroup)VisualStateManager.GetVisualStateGroups(layoutRoot)[0]).CurrentState.Name;

            var progressBarRoot = VisualTreeHelper.GetChild(layoutRoot, 0);
            var clip = VisualTreeHelper.GetChild(progressBarRoot, 0);
            var stackPanel = VisualTreeHelper.GetChild(clip, 0);
            var indicator = (Rectangle)VisualTreeHelper.GetChild(stackPanel, 0);

            indicator.SizeChanged += this.Indicator_SizeChanged;
            IndicatorWidthText.Text = indicator.ActualWidth.ToString();

            Loaded -= ProgressBarPage_Loaded;
        }

        private void Indicator_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            IndicatorWidthText.Text = ((Rectangle)sender).ActualWidth.ToString();
        }

        private void ProgressBarPage_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            VisualStateText.Text = e.NewState.Name;
        }

        public void UpdateMinMax_Click(object sender, RoutedEventArgs e)
        {
            TestProgressBar.Maximum = string.IsNullOrEmpty(MaximumInput.Text) ? double.Parse(ControlHelper.GetPlaceholderText(MaximumInput)) : double.Parse(MaximumInput.Text);
            TestProgressBar.Minimum = string.IsNullOrEmpty(MinimumInput.Text) ? double.Parse(ControlHelper.GetPlaceholderText(MinimumInput)) : double.Parse(MinimumInput.Text);
        }

        public void UpdateWidth_Click(object sender, RoutedEventArgs e)
        {
            TestProgressBar.Width = string.IsNullOrEmpty(WidthInput.Text) ? double.Parse(ControlHelper.GetPlaceholderText(WidthInput)) : double.Parse(WidthInput.Text);
        }

        public void UpdateValue_Click(object sender, RoutedEventArgs e)
        {
            TestProgressBar.Value = string.IsNullOrEmpty(ValueInput.Text) ? double.Parse(ControlHelper.GetPlaceholderText(ValueInput)) : double.Parse(ValueInput.Text);
        }

        public void ChangeValue_Click(object sender, RoutedEventArgs e)
        {
            if (TestProgressBar.Value + 1 > TestProgressBar.Maximum)
            {
                TestProgressBar.Value = (int)(TestProgressBar.Minimum + 0.5);
            }
            else
            {
                TestProgressBar.Value += 1;
            }
        }

        public void UpdatePadding_Click(object sender, RoutedEventArgs e)
        {
            double paddingLeft = string.IsNullOrEmpty(PaddingLeftInput.Text) ? double.Parse(ControlHelper.GetPlaceholderText(PaddingLeftInput)) : double.Parse(PaddingLeftInput.Text);
            double paddingRight = string.IsNullOrEmpty(PaddingRightInput.Text) ? double.Parse(ControlHelper.GetPlaceholderText(PaddingRightInput)) : double.Parse(PaddingRightInput.Text);

            TestProgressBar.Padding = new Thickness(paddingLeft, 0, paddingRight, 0);
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            var tm = ThemeManager.Current;
            if (tm.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                tm.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                tm.ApplicationTheme = ApplicationTheme.Dark;
            }
        }
    }
}
