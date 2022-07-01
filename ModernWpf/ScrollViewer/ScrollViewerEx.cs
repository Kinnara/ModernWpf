using ModernWpf.Controls.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls
{
    public class ScrollViewerEx : ScrollViewer
    {
        private double LastVerticalLocation = 0;
        private double LastHorizontalLocation = 0;

        public ScrollViewerEx()
        {
            Loaded += OnLoaded;
            var valueSource = DependencyPropertyHelper.GetValueSource(this, AutoPanningMode.IsEnabledProperty).BaseValueSource;
            if (valueSource == BaseValueSource.Default)
            {
                AutoPanningMode.SetIsEnabled(this, true);
            }
        }

        #region Orientation

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(ScrollViewerEx),
                new PropertyMetadata(Orientation.Vertical));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        #endregion

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

        #region ForceUseSmoothScroll

        public static readonly DependencyProperty ForceUseSmoothScrollProperty =
            DependencyProperty.Register(
                nameof(ForceUseSmoothScroll),
                typeof(bool),
                typeof(ScrollViewerEx),
                new PropertyMetadata(true));

        public bool ForceUseSmoothScroll
        {
            get => (bool)GetValue(ForceUseSmoothScrollProperty);
            set => SetValue(ForceUseSmoothScrollProperty, value);
        }

        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            LastVerticalLocation = VerticalOffset;
            LastHorizontalLocation = HorizontalOffset;
            if (ScrollInfo != null)
            {
                ScrollInfo = new ScrollInfoAdapter(ScrollInfo);
                ((ScrollInfoAdapter)ScrollInfo).UpdateOffsets();
                BindingOperations.SetBinding((ScrollInfoAdapter)ScrollInfo, ScrollInfoAdapter.ForceUseSmoothScrollProperty, new Binding
                {
                    Source = this,
                    Path = new PropertyPath(nameof(ForceUseSmoothScroll))
                });
            }
            UpdateVisualState(false);
        }

        /// <inheritdoc/>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (Style == null && ReadLocalValue(StyleProperty) == DependencyProperty.UnsetValue)
            {
                SetResourceReference(StyleProperty, typeof(ScrollViewer));
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            ScrollViewerBehavior.SetIsAnimating(this, true);

            double WheelChange = e.Delta;
            Orientation Direction = GetDirection();

            if (Direction == Orientation.Vertical)
            {
                double newOffset = LastVerticalLocation - (WheelChange * 1);

                if (newOffset < 0)
                {
                    newOffset = 0;
                }

                if (newOffset > ScrollableHeight)
                {
                    newOffset = ScrollableHeight;
                }

                if (newOffset == LastVerticalLocation)
                {
                    e.Handled = true;
                    return;
                }

                ScrollToVerticalOffset(LastVerticalLocation);

                double scale = Math.Abs((LastVerticalLocation - newOffset) / (WheelChange * 1));

                AnimateScroll(newOffset, Direction, scale);
                LastVerticalLocation = newOffset;
            }
            else
            {
                double newOffset = LastHorizontalLocation - (WheelChange * 1);

                if (newOffset < 0)
                {
                    newOffset = 0;
                }

                if (newOffset > ScrollableWidth)
                {
                    newOffset = ScrollableWidth;
                }

                if (newOffset == LastHorizontalLocation)
                {
                    e.Handled = true;
                    return;
                }

                ScrollToHorizontalOffset(LastHorizontalLocation);

                double scale = Math.Abs((LastHorizontalLocation - newOffset) / (WheelChange * 1));

                AnimateScroll(newOffset, Direction, scale);
                LastHorizontalLocation = newOffset;
            }

            e.Handled = true;
        }

        /// <inheritdoc/>
        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            base.OnScrollChanged(e);
            if (!ScrollViewerBehavior.GetIsAnimating(this))
            {
                LastVerticalLocation = VerticalOffset;
                LastHorizontalLocation = HorizontalOffset;
            }
        }

        private Orientation GetDirection()
        {
            var isShiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            if (Orientation == Orientation.Horizontal)
            {
                return isShiftDown ? Orientation.Vertical : Orientation.Horizontal;
            }
            else
            {
                return isShiftDown ? Orientation.Horizontal : Orientation.Vertical;
            }
        }

        /// <summary>
        /// Causes the <see cref="ScrollViewerEx"/> to load a new view into the viewport using the specified offsets and zoom factor.
        /// </summary>
        /// <param name="horizontalOffset">A value between 0 and <see cref="ScrollViewer.ScrollableWidth"/> that specifies the distance the content should be scrolled horizontally.</param>
        /// <param name="verticalOffset">A value between 0 and <see cref="ScrollViewer.ScrollableHeight"/> that specifies the distance the content should be scrolled vertically.</param>
        /// <param name="zoomFactor">A value between MinZoomFactor and MaxZoomFactor that specifies the required target ZoomFactor.</param>
        /// <returns><see langword="true"/> if the view is changed; otherwise, <see langword="false"/>.</returns>
        public bool ChangeView(double? horizontalOffset, double? verticalOffset, float? zoomFactor)
        {
            return ChangeView(horizontalOffset, verticalOffset, zoomFactor, false);
        }

        /// <summary>
        /// Causes the <see cref="ScrollViewerEx"/> to load a new view into the viewport using the specified offsets and zoom factor, and optionally disables scrolling animation.
        /// </summary>
        /// <param name="horizontalOffset">A value between 0 and <see cref="ScrollViewer.ScrollableWidth"/> that specifies the distance the content should be scrolled horizontally.</param>
        /// <param name="verticalOffset">A value between 0 and <see cref="ScrollViewer.ScrollableHeight"/> that specifies the distance the content should be scrolled vertically.</param>
        /// <param name="zoomFactor">A value between MinZoomFactor and MaxZoomFactor that specifies the required target ZoomFactor.</param>
        /// <param name="disableAnimation"><see langword="true"/> to disable zoom/pan animations while changing the view; otherwise, <see langword="false"/>. The default is false.</param>
        /// <returns><see langword="true"/> if the view is changed; otherwise, <see langword="false"/>.</returns>
        public bool ChangeView(double? horizontalOffset, double? verticalOffset, float? zoomFactor, bool disableAnimation)
        {
            if (disableAnimation)
            {
                if (horizontalOffset.HasValue)
                {
                    ScrollToHorizontalOffset(horizontalOffset.Value);
                }

                if (verticalOffset.HasValue)
                {
                    ScrollToVerticalOffset(verticalOffset.Value);
                }
            }
            else
            {
                if (horizontalOffset.HasValue)
                {
                    ScrollToHorizontalOffset(LastHorizontalLocation);
                    AnimateScroll(horizontalOffset.Value, Orientation.Vertical, 1);
                    LastHorizontalLocation = horizontalOffset.Value;
                }

                if (verticalOffset.HasValue)
                {
                    ScrollToVerticalOffset(LastVerticalLocation);
                    AnimateScroll(verticalOffset.Value, Orientation.Vertical, 1);
                    LastVerticalLocation = verticalOffset.Value;
                }
            }

            return true; // TODO
        }

        private void AnimateScroll(double ToValue, Orientation Direction, double Scale)
        {
            if (Direction == Orientation.Vertical)
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
            else
            {
                BeginAnimation(ScrollViewerBehavior.HorizontalOffsetProperty, null);
                DoubleAnimation Animation = new DoubleAnimation();
                Animation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                Animation.From = HorizontalOffset;
                Animation.To = ToValue;
                Animation.Duration = TimeSpan.FromMilliseconds(400 * Scale);
                //Timeline.SetDesiredFrameRate(Animation, 40);
                BeginAnimation(ScrollViewerBehavior.HorizontalOffsetProperty, Animation);
            }

            BeginAnimation(ScrollViewerBehavior.IsAnimatingProperty, null);
            BooleanAnimationUsingKeyFrames keyFramesAnimation = new BooleanAnimationUsingKeyFrames();
            keyFramesAnimation.KeyFrames.Add(new DiscreteBooleanKeyFrame(true, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));
            keyFramesAnimation.KeyFrames.Add(new DiscreteBooleanKeyFrame(false, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400 * Scale + 1))));
            BeginAnimation(ScrollViewerBehavior.IsAnimatingProperty, keyFramesAnimation);
        }

        private void UpdateVisualState(bool useTransitions = true)
        {
            string stateName = AutoHideScrollBars ? "NoIndicator" : "MouseIndicator";
            VisualStateManager.GoToState(this, stateName, useTransitions);
        }
    }
}
