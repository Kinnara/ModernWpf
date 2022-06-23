using ModernWpf.Controls.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls
{
    public class ScrollViewerEx : ScrollViewer
    {
        private double LastLocation = 0;

        public ScrollViewerEx()
        {
            Loaded += (s, e) => UpdateVisualState(false);
        }

        #region AutoHideScrollBars

        public static readonly DependencyProperty AutoHideScrollBarsProperty =
            ScrollViewerHelper.AutoHideScrollBarsProperty
            .AddOwner(
                typeof(ScrollViewerEx),
                new PropertyMetadata(true, OnAutoHideScrollBarsChanged));

        public bool AutoHideScrollBars
        {
            get => (bool)GetValue(AutoHideScrollBarsProperty);
            set => SetValue(AutoHideScrollBarsProperty, value);
        }

        private static void OnAutoHideScrollBarsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewerEx sv)
            {
                sv.UpdateVisualState();
            }
        }

        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (Style == null && ReadLocalValue(StyleProperty) == DependencyProperty.UnsetValue)
            {
                SetResourceReference(StyleProperty, typeof(ScrollViewer));
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            double WheelChange = e.Delta;

            double newOffset = LastLocation - (WheelChange * 1);

            if (newOffset < 0)
                newOffset = 0;
            if (newOffset > ScrollableHeight)
                newOffset = ScrollableHeight;

            if (newOffset == LastLocation)
            {
                e.Handled = true;
                return;
            }

            ScrollToVerticalOffset(LastLocation);

            double scale = Math.Abs((LastLocation - newOffset) / (WheelChange * 1));

            AnimateScroll(newOffset, scale);
            LastLocation = newOffset;

            e.Handled = true;
        }

        private void AnimateScroll(double ToValue, double Scale)
        {
            BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, null);
            DoubleAnimation Animation = new DoubleAnimation();
            Animation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            Animation.From = VerticalOffset;
            Animation.To = ToValue;
            Animation.Duration = TimeSpan.FromMilliseconds(400 * Scale);
            //Timeline.SetDesiredFrameRate(Animation, 40);
            BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, Animation);
        }

        private void UpdateVisualState(bool useTransitions = true)
        {
            string stateName = AutoHideScrollBars ? "NoIndicator" : "MouseIndicator";
            VisualStateManager.GoToState(this, stateName, useTransitions);
        }
    }
}
