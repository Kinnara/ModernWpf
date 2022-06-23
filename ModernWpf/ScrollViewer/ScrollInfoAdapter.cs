// Ported from https://www.wpf-controls.com/wpf-smooth-scroll-viewer

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls
{
    public class ScrollInfoAdapter : UIElement, IScrollInfo
    {
        private IScrollInfo _child;
        private double _computedVerticalOffset = 0;
        private double _computedHorizontalOffset = 0;
        internal const double _scrollLineDelta = 16.0;
        internal const double _mouseWheelDelta = 48.0;

        public ScrollInfoAdapter(IScrollInfo child)
        {
            _child = child;
        }

        #region ForceUseSmoothScroll

        public static readonly DependencyProperty ForceUseSmoothScrollProperty =
            DependencyProperty.Register(
                nameof(ForceUseSmoothScroll),
                typeof(bool),
                typeof(ScrollInfoAdapter),
                new PropertyMetadata(true, OnForceUseSmoothScrollChanged));

        public bool ForceUseSmoothScroll
        {
            get => (bool)GetValue(ForceUseSmoothScrollProperty);
            set => SetValue(ForceUseSmoothScrollProperty, value);
        }

        private static void OnForceUseSmoothScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ScrollInfoAdapter)d).UpdateOffsets();
        }

        #endregion

        /// <inheritdoc/>
        public bool CanVerticallyScroll
        {
            get => _child.CanVerticallyScroll;
            set => _child.CanVerticallyScroll = value;
        }

        /// <inheritdoc/>
        public bool CanHorizontallyScroll
        {
            get => _child.CanHorizontallyScroll;
            set => _child.CanHorizontallyScroll = value;
        }

        /// <inheritdoc/>
        public double ExtentWidth => _child.ExtentWidth;

        /// <inheritdoc/>
        public double ExtentHeight => _child.ExtentHeight;

        /// <inheritdoc/>
        public double ViewportWidth => _child.ViewportWidth;

        /// <inheritdoc/>
        public double ViewportHeight => _child.ViewportHeight;

        /// <inheritdoc/>
        public double HorizontalOffset => _child.HorizontalOffset;

        /// <inheritdoc/>
        public double VerticalOffset => _child.VerticalOffset;

        /// <inheritdoc/>
        public ScrollViewer ScrollOwner
        {
            get => _child.ScrollOwner;
            set => _child.ScrollOwner = value;
        }

        public void UpdateOffsets()
        {
            _computedVerticalOffset = VerticalOffset;
            _computedHorizontalOffset = HorizontalOffset;
        }

        /// <inheritdoc/>
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return _child.MakeVisible(visual, rectangle);
        }

        /// <inheritdoc/>
        public void LineUp()
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.LineUp();
            }
            else
            {
                VerticalScroll(_computedVerticalOffset - _scrollLineDelta);
            }
        }

        /// <inheritdoc/>
        public void LineDown()
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.LineDown();
            }
            else
            {
                VerticalScroll(_computedVerticalOffset + _scrollLineDelta);
            }
        }

        /// <inheritdoc/>
        public void LineLeft()
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.LineLeft();
            }
            else
            {
                HorizontalScroll(_computedHorizontalOffset - _scrollLineDelta);
            }
        }

        /// <inheritdoc/>
        public void LineRight()
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.LineRight();
            }
            else
            {
                HorizontalScroll(_computedHorizontalOffset + _scrollLineDelta);
            }
        }

        /// <inheritdoc/>
        public void MouseWheelUp()
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.MouseWheelUp();
            }
            else
            {
                VerticalScroll(_computedVerticalOffset - _mouseWheelDelta);
            }
        }

        /// <inheritdoc/>
        public void MouseWheelDown()
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.MouseWheelDown();
            }
            else
            {
                VerticalScroll(_computedVerticalOffset + _mouseWheelDelta);
            }
        }

        /// <inheritdoc/>
        public void MouseWheelLeft()
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.MouseWheelLeft();
            }
            else
            {
                HorizontalScroll(_computedHorizontalOffset - _mouseWheelDelta);
            }
        }

        /// <inheritdoc/>
        public void MouseWheelRight()
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.MouseWheelRight();
            }
            else
            {
                HorizontalScroll(_computedHorizontalOffset + _mouseWheelDelta);
            }
        }

        /// <inheritdoc/>
        public void PageUp()
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.PageUp();
            }
            else
            {
                VerticalScroll(_computedVerticalOffset - ViewportHeight);
            }
        }

        /// <inheritdoc/>
        public void PageDown()
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.PageDown();
            }
            else
            {
                VerticalScroll(_computedVerticalOffset + ViewportHeight);
            }
        }

        /// <inheritdoc/>
        public void PageLeft()
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.PageLeft();
            }
            else
            {
                HorizontalScroll(_computedHorizontalOffset - ViewportWidth);
            }
        }

        /// <inheritdoc/>
        public void PageRight()
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.PageRight();
            }
            else
            {
                HorizontalScroll(_computedHorizontalOffset + ViewportWidth);
            }
        }

        /// <inheritdoc/>
        public void SetHorizontalOffset(double offset)
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.SetHorizontalOffset(offset);
            }
            else
            {
                _computedHorizontalOffset = offset;
                Animate(HorizontalScrollOffsetProperty, offset, 0);
            }
        }

        /// <inheritdoc/>
        public void SetVerticalOffset(double offset)
        {
            if (!ForceUseSmoothScroll && _child.ScrollOwner.CanContentScroll == true)
            {
                _child.SetVerticalOffset(offset);
            }
            else
            {
                _computedVerticalOffset = offset;
                Animate(VerticalScrollOffsetProperty, offset, 0);
            }
        }

        #region not exposed methods
        private void Animate(DependencyProperty property, double targetValue, int duration = 300)
        {
            //make a smooth animation that starts and ends slowly
            var keyFramesAnimation = new DoubleAnimationUsingKeyFrames();
            keyFramesAnimation.Duration = TimeSpan.FromMilliseconds(duration);
            keyFramesAnimation.KeyFrames.Add(
                new SplineDoubleKeyFrame(
                    targetValue,
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration)),
                    new KeySpline(0.5, 0.0, 0.5, 1.0)
                    )
                );

            BeginAnimation(property, keyFramesAnimation);
        }

        private void VerticalScroll(double val)
        {
            if (Math.Abs(_computedVerticalOffset - ValidateVerticalOffset(val)) > 0.1)//prevent restart of animation in case of frequent event fire
            {
                _computedVerticalOffset = ValidateVerticalOffset(val);
                Animate(VerticalScrollOffsetProperty, _computedVerticalOffset);
            }
        }

        private void HorizontalScroll(double val)
        {
            if (Math.Abs(_computedHorizontalOffset - ValidateHorizontalOffset(val)) > 0.1)//prevent restart of animation in case of frequent event fire
            {
                _computedHorizontalOffset = ValidateHorizontalOffset(val);
                Animate(HorizontalScrollOffsetProperty, _computedHorizontalOffset);
            }
        }

        private double ValidateVerticalOffset(double verticalOffset)
        {
            return verticalOffset < 0
                ? 0
                : verticalOffset > _child.ScrollOwner.ScrollableHeight
                ? _child.ScrollOwner.ScrollableHeight
                : verticalOffset;
        }

        private double ValidateHorizontalOffset(double horizontalOffset)
        {
            return horizontalOffset < 0
                ? 0
                : horizontalOffset > _child.ScrollOwner.ScrollableWidth
                ? _child.ScrollOwner.ScrollableWidth
                : horizontalOffset;
        }
        #endregion

        #region helper dependency properties as scrollbars are not animatable by default
        internal double VerticalScrollOffset
        {
            get { return (double)GetValue(VerticalScrollOffsetProperty); }
            set { SetValue(VerticalScrollOffsetProperty, value); }
        }
        internal static readonly DependencyProperty VerticalScrollOffsetProperty =
            DependencyProperty.Register("VerticalScrollOffset", typeof(double), typeof(ScrollInfoAdapter),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnVerticalScrollOffsetChanged)));
        private static void OnVerticalScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var smoothScrollViewer = (ScrollInfoAdapter)d;
            smoothScrollViewer._child.SetVerticalOffset((double)e.NewValue);
        }

        internal double HorizontalScrollOffset
        {
            get { return (double)GetValue(HorizontalScrollOffsetProperty); }
            set { SetValue(HorizontalScrollOffsetProperty, value); }
        }
        internal static readonly DependencyProperty HorizontalScrollOffsetProperty =
            DependencyProperty.Register("HorizontalScrollOffset", typeof(double), typeof(ScrollInfoAdapter),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnHorizontalScrollOffsetChanged)));
        private static void OnHorizontalScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var smoothScrollViewer = (ScrollInfoAdapter)d;
            smoothScrollViewer._child.SetHorizontalOffset((double)e.NewValue);
        }
        #endregion
    }
}
