using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ModernWpf.Controls.Primitives
{
    public enum ThemeShadowChromeWindowedPopupInsetMode
    {
        Default,
        Small,
        Medium
    }

    public class ThemeShadowChrome : Decorator
    {
        public ThemeShadowChrome()
        {
#if NET462_OR_NEWER
            _bitmapCache = new BitmapCache(VisualTreeHelper.GetDpi(this).PixelsPerDip);
#else
            _bitmapCache = new BitmapCache();
#endif
            _background = new Grid
            {
                CacheMode = _bitmapCache,
                Focusable = false,
                IsHitTestVisible = false,
                SnapsToDevicePixels = false
            };
            AddVisualChild(_background);

            ThemeManager.AddActualThemeChangedHandler(this, OnActualThemeChanged);
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            SetCurrentValue(ShadowProperty, new ThemeShadow());
        }

        #region IsShadowEnabled

        public static readonly DependencyProperty IsShadowEnabledProperty =
            DependencyProperty.Register(
                nameof(IsShadowEnabled),
                typeof(bool),
                typeof(ThemeShadowChrome),
                new PropertyMetadata(true, OnIsShadowEnabledChanged));

        public bool IsShadowEnabled
        {
            get => (bool)GetValue(IsShadowEnabledProperty);
            set => SetValue(IsShadowEnabledProperty, value);
        }

        private static void OnIsShadowEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeShadowChrome)d).OnIsShadowEnabledChanged();
        }

        private void OnIsShadowEnabledChanged()
        {
            SyncShadowFromIsShadowEnabled();

            if (IsInitialized)
            {
                if (IsShadowEnabled)
                {
                    EnsureShadow();
                    Debug.Assert(_background.Children.Count == 0);
                    _background.Children.Add(_shadow);
                    _background.Visibility = Visibility.Visible;
                }
                else
                {
                    _background.Children.Clear();
                    _background.Visibility = Visibility.Collapsed;
                }

                OnVisualParentChanged();
                UpdatePopupMargin();
                UpdateShadowOpacity();
                UpdateShadowOpacitySubscription();
            }
        }

        #endregion

        #region Shadow

        public static readonly DependencyProperty ShadowProperty =
            DependencyProperty.Register(
                nameof(Shadow),
                typeof(ThemeShadow),
                typeof(ThemeShadowChrome),
                new PropertyMetadata(null, OnShadowChanged));

        public ThemeShadow Shadow
        {
            get => (ThemeShadow)GetValue(ShadowProperty);
            set => SetValue(ShadowProperty, value);
        }

        private static void OnShadowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeShadowChrome)d).OnShadowChanged();
        }

        private void OnShadowChanged()
        {
            SyncIsShadowEnabledFromShadow();
        }

        private void SyncShadowFromIsShadowEnabled()
        {
            if (_updatingShadowAlias)
            {
                return;
            }

            try
            {
                _updatingShadowAlias = true;
                SetCurrentValue(ShadowProperty, IsShadowEnabled ? Shadow ?? new ThemeShadow() : null);
            }
            finally
            {
                _updatingShadowAlias = false;
            }
        }

        private void SyncIsShadowEnabledFromShadow()
        {
            if (_updatingShadowAlias)
            {
                return;
            }

            try
            {
                _updatingShadowAlias = true;
                SetCurrentValue(IsShadowEnabledProperty, Shadow != null);
            }
            finally
            {
                _updatingShadowAlias = false;
            }
        }

        #endregion

        #region Depth

        public static readonly DependencyProperty DepthProperty =
            DependencyProperty.Register(
                nameof(Depth),
                typeof(double),
                typeof(ThemeShadowChrome),
                new PropertyMetadata(32d, OnDepthChanged));

        public double Depth
        {
            get => (double)GetValue(DepthProperty);
            set => SetValue(DepthProperty, value);
        }

        private static void OnDepthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeShadowChrome)d).OnDepthChanged();
        }

        internal Thickness ShadowPadding => ThemeShadowRenderer.GetPadding(Depth);

        internal Thickness PopupShadowPadding => ThemeShadowRenderer.GetPopupPadding(WindowedPopupInsetMode, Depth);

        private void OnDepthChanged()
        {
            SyncTranslationZFromDepth();

            if (IsInitialized)
            {
                UpdateShadow(invalidateLayout: ReservesShadowSpace);
                UpdatePopupMargin();
            }
        }

        #endregion

        #region TranslationZ

        public static readonly DependencyProperty TranslationZProperty =
            DependencyProperty.Register(
                nameof(TranslationZ),
                typeof(double),
                typeof(ThemeShadowChrome),
                new PropertyMetadata(32d, OnTranslationZChanged));

        public double TranslationZ
        {
            get => (double)GetValue(TranslationZProperty);
            set => SetValue(TranslationZProperty, value);
        }

        private static void OnTranslationZChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeShadowChrome)d).OnTranslationZChanged();
        }

        private void OnTranslationZChanged()
        {
            SyncDepthFromTranslationZ();
        }

        private void SyncTranslationZFromDepth()
        {
            if (_updatingTranslationZAlias)
            {
                return;
            }

            try
            {
                _updatingTranslationZAlias = true;
                if (!AreClose(TranslationZ, Depth))
                {
                    SetCurrentValue(TranslationZProperty, Depth);
                }
            }
            finally
            {
                _updatingTranslationZAlias = false;
            }
        }

        private void SyncDepthFromTranslationZ()
        {
            if (_updatingTranslationZAlias)
            {
                return;
            }

            try
            {
                _updatingTranslationZAlias = true;
                if (!AreClose(Depth, TranslationZ))
                {
                    SetCurrentValue(DepthProperty, TranslationZ);
                }
            }
            finally
            {
                _updatingTranslationZAlias = false;
            }
        }

        private static bool AreClose(double value1, double value2)
        {
            return value1.Equals(value2) || Math.Abs(value1 - value2) < 0.000001;
        }

        #endregion

        #region WindowedPopupInsetMode

        public static readonly DependencyProperty WindowedPopupInsetModeProperty =
            DependencyProperty.Register(
                nameof(WindowedPopupInsetMode),
                typeof(ThemeShadowChromeWindowedPopupInsetMode),
                typeof(ThemeShadowChrome),
                new PropertyMetadata(ThemeShadowChromeWindowedPopupInsetMode.Default, OnWindowedPopupInsetModeChanged));

        public ThemeShadowChromeWindowedPopupInsetMode WindowedPopupInsetMode
        {
            get => (ThemeShadowChromeWindowedPopupInsetMode)GetValue(WindowedPopupInsetModeProperty);
            set => SetValue(WindowedPopupInsetModeProperty, value);
        }

        private static void OnWindowedPopupInsetModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeShadowChrome)d).OnWindowedPopupInsetModeChanged();
        }

        private void OnWindowedPopupInsetModeChanged()
        {
            if (IsInitialized)
            {
                UpdateShadow(invalidateLayout: ReservesShadowSpace);
                UpdatePopupMargin();
            }
        }

        #endregion

        #region ReservesShadowSpace

        public static readonly DependencyProperty ReservesShadowSpaceProperty =
            DependencyProperty.Register(
                nameof(ReservesShadowSpace),
                typeof(bool),
                typeof(ThemeShadowChrome),
                new PropertyMetadata(false, OnReservesShadowSpaceChanged));

        public bool ReservesShadowSpace
        {
            get => (bool)GetValue(ReservesShadowSpaceProperty);
            set => SetValue(ReservesShadowSpaceProperty, value);
        }

        private static void OnReservesShadowSpaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeShadowChrome)d).OnReservesShadowSpaceChanged();
        }

        private void OnReservesShadowSpaceChanged()
        {
            if (IsInitialized)
            {
                UpdateShadow(invalidateLayout: true);
            }
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(ThemeShadowChrome),
                new PropertyMetadata(new CornerRadius(), OnCornerRadiusChanged));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeShadowChrome)d).OnCornerRadiusChanged(e);
        }

        private void OnCornerRadiusChanged(DependencyPropertyChangedEventArgs e)
        {
            var cornerRadius = (CornerRadius)e.NewValue;

            if (_shadow != null)
            {
                _shadow.CornerRadius = cornerRadius;
            }
        }

        #endregion

        #region PopupMargin

        private static readonly DependencyProperty PopupMarginProperty =
            DependencyProperty.Register(
                nameof(PopupMargin),
                typeof(Thickness),
                typeof(ThemeShadowChrome),
                new PropertyMetadata(new Thickness(), OnPopupMarginChanged));

        private Thickness PopupMargin
        {
            get => (Thickness)GetValue(PopupMarginProperty);
            set => SetValue(PopupMarginProperty, value);
        }

        private static void OnPopupMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeShadowChrome)d).OnPopupMarginChanged(e);
        }

        private void OnPopupMarginChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_parentPopupControl != null)
            {
                PositionParentPopupControl();
            }
        }

        private void UpdatePopupMargin()
        {
            if (IsShadowEnabled)
            {
                PopupMargin = PopupShadowPadding;
            }
            else
            {
                ClearValue(PopupMarginProperty);
            }
        }

        #endregion

        protected override int VisualChildrenCount =>
            IsShadowEnabled ? Child == null ? 1 : 2 : base.VisualChildrenCount;

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if (IsInitialized)
            {
                OnVisualParentChanged();
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (IsShadowEnabled)
            {
                if (index == 0)
                {
                    return _background;
                }
                else if (index == 1 && Child != null)
                {
                    return Child;
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
            else
            {
                return base.GetVisualChild(index);
            }
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            UpdateShadowOpacitySource();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            OnIsShadowEnabledChanged();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (!IsShadowEnabled)
            {
                return base.MeasureOverride(constraint);
            }

            var padding = LayoutShadowPadding;
            var child = Child;
            if (child == null)
            {
                _background.Measure(constraint);
                return base.MeasureOverride(constraint);
            }

            child.Measure(new Size(
                SubtractConstraintPadding(constraint.Width, padding.Left + padding.Right),
                SubtractConstraintPadding(constraint.Height, padding.Top + padding.Bottom)));

            var childSize = child.DesiredSize;
            var desiredSize = new Size(
                childSize.Width + padding.Left + padding.Right,
                childSize.Height + padding.Top + padding.Bottom);

            if (_shadow != null)
            {
                _shadow.LayoutSize = desiredSize;
            }
            _background.Measure(desiredSize);

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (!IsShadowEnabled)
            {
                return base.ArrangeOverride(arrangeSize);
            }

            var padding = LayoutShadowPadding;
            var child = Child;
            var hasChild = child != null;
            var contentSize = hasChild
                ? new Size(
                    Math.Max(0, arrangeSize.Width - padding.Left - padding.Right),
                    Math.Max(0, arrangeSize.Height - padding.Top - padding.Bottom))
                : arrangeSize;

            if (_shadow != null)
            {
                _shadow.ContentSize = contentSize;
                _shadow.LayoutSize = arrangeSize;
                _shadow.ContentOrigin = hasChild
                    ? new Point(padding.Left, padding.Top)
                    : new Point();
                _background.Arrange(new Rect(arrangeSize));
            }

            if (child != null)
            {
                child.Arrange(new Rect(padding.Left, padding.Top, contentSize.Width, contentSize.Height));
            }

            return arrangeSize;
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            // WinUI's DropShadowVisual is TransparentForInput. Keep the WPF
            // shadow host itself transparent so only the decorated child can hit.
            if (Child is UIElement child && child.IsHitTestVisible)
            {
                var childPoint = TranslatePoint(hitTestParameters.HitPoint, child);
                if (child.InputHitTest(childPoint) != null)
                {
                    return new PointHitTestResult(child, childPoint);
                }
            }

            return null;
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return IsShadowEnabled ? null : base.GetLayoutClip(layoutSlotSize);
        }

#if NET462_OR_NEWER
        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            _bitmapCache.RenderAtScale = newDpi.PixelsPerDip;
            _shadow?.InvalidateVisual();
        }
#endif

        private void OnVisualParentChanged()
        {
            if (IsShadowEnabled)
            {
                PopupControl parentPopupControl = null;

                var visualParent = VisualParent;
                if (visualParent is ContextMenu contextMenu)
                {
                    parentPopupControl = new PopupControl(contextMenu);
                }
                else if (visualParent is ToolTip toolTip)
                {
                    parentPopupControl = new PopupControl(toolTip);
                }
                else if (FindParentPopup(this) is Popup parentPopup)
                {
                    parentPopupControl = new PopupControl(parentPopup);
                }

                SetParentPopupControl(parentPopupControl);
            }
            else
            {
                SetParentPopupControl(null);
            }
        }

        internal bool UsesSoftwareRenderer => _shadow != null;

        private void EnsureShadow()
        {
            if (_shadow == null)
            {
                _shadow = new ThemeShadowElement
                {
                    Depth = Depth,
                    CornerRadius = CornerRadius,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
            }
        }

        private void UpdateShadow(bool invalidateLayout)
        {
            if (_shadow != null)
            {
                _shadow.Depth = Depth;
            }

            if (invalidateLayout)
            {
                InvalidateMeasure();
                InvalidateArrange();
            }
        }

        private void UpdateShadowOpacitySource()
        {
            var child = Child as UIElement;
            if (_shadowOpacitySource == child)
            {
                return;
            }

            if (_shadowOpacitySource != null)
            {
                OpacityPropertyDescriptor.RemoveValueChanged(_shadowOpacitySource, OnShadowOpacitySourceChanged);
            }

            _shadowOpacitySource = child;

            if (_shadowOpacitySource != null)
            {
                OpacityPropertyDescriptor.AddValueChanged(_shadowOpacitySource, OnShadowOpacitySourceChanged);
            }

            UpdateShadowOpacity();
            UpdateShadowOpacitySubscription();
        }

        private void OnShadowOpacitySourceChanged(object sender, EventArgs e)
        {
            UpdateShadowOpacity();
        }

        private void UpdateShadowOpacitySubscription()
        {
            if (IsLoaded && IsShadowEnabled && _shadowOpacitySource != null)
            {
                if (!_isShadowOpacityRenderingHooked)
                {
                    CompositionTarget.Rendering += OnRendering;
                    _isShadowOpacityRenderingHooked = true;
                }
            }
            else
            {
                StopShadowOpacityRenderingSync();
            }
        }

        private void StopShadowOpacityRenderingSync()
        {
            if (_isShadowOpacityRenderingHooked)
            {
                CompositionTarget.Rendering -= OnRendering;
                _isShadowOpacityRenderingHooked = false;
            }
        }

        private void OnRendering(object sender, EventArgs e)
        {
            UpdateShadowOpacity();
        }

        private void UpdateShadowOpacity()
        {
            var opacity = _shadowOpacitySource?.Opacity ?? 1.0;
            if (Math.Abs(_background.Opacity - opacity) > 0.001)
            {
                _background.Opacity = opacity;
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ClearMarginAdjustment();
            UpdateLayout();
            AdjustMargin();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateShadowOpacitySubscription();

            if (IsVisible)
            {
                AdjustMargin();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopShadowOpacityRenderingSync();
        }

        private void OnActualThemeChanged(object sender, RoutedEventArgs e)
        {
            _shadow?.InvalidateVisual();
        }

        private Thickness LayoutShadowPadding => ReservesShadowSpace ? PopupShadowPadding : new Thickness();

        private static double SubtractConstraintPadding(double value, double padding)
        {
            return double.IsInfinity(value) ? value : Math.Max(0, value - padding);
        }

        private void AdjustMargin()
        {
            if (_parentPopupControl != null)
            {
                var margin = Margin;
                if (margin != new Thickness() && VisualParent is UIElement parent)
                {
                    var parentWidth = parent.RenderSize.Width;
                    var shadowWidth = ActualWidth;
                    if (parentWidth > 0 && shadowWidth > 0)
                    {
                        if (parentWidth < shadowWidth + margin.Left + margin.Right)
                        {
                            var leftRightMargin = (parentWidth - shadowWidth) / 2;
                            var adjustedMargin = new Thickness(leftRightMargin, margin.Top, leftRightMargin, margin.Bottom);
                            var marginAnim = new ThicknessAnimation(adjustedMargin, TimeSpan.Zero);
                            BeginAnimation(MarginProperty, marginAnim);
                            UpdateLayout();
                        }
                    }
                }
            }
        }

        private void ClearMarginAdjustment()
        {
            BeginAnimation(MarginProperty, null);
        }

        private void SetParentPopupControl(PopupControl value)
        {
            if (_parentPopupControl == value)
            {
                return;
            }

            if (_popupPositioner != null)
            {
                _popupPositioner.Dispose();
                _popupPositioner = null;
            }

            if (_parentPopupControl != null)
            {
                _parentPopupControl.Opened -= OnParentPopupControlOpened;
                _parentPopupControl.Closed -= OnParentPopupControlClosed;
                _parentPopupControl.Dispose();
            }

            _parentPopupControl = value as PopupControl;

            if (_parentPopupControl != null)
            {
                _parentPopupControl.Opened += OnParentPopupControlOpened;
                _parentPopupControl.Closed += OnParentPopupControlClosed;
            }
        }

        private void OnParentPopupControlOpened(object sender, EventArgs e)
        {
            if (_popupPositioner != null)
            {
                return;
            }

            if (_parentPopupControl != null)
            {
                if (_parentPopupControl.Control is { } control)
                {
                    if (control is ToolTip toolTip && toolTip.PlacementTarget is Thumb thumb && thumb.TemplatedParent is Slider)
                    {
                        // Do not reposition slider auto tool tip
                        return;
                    }
                    else
                    {
                        var popup = (control as Popup) ?? (control.Parent as Popup);
                        if (popup != null && PopupPositioner.IsSupported)
                        {
                            _popupPositioner = new PopupPositioner(popup);
                        }
                    }
                }
            }

            if (_popupPositioner == null)
            {
                PositionParentPopupControl();
            }
        }

        private void OnParentPopupControlClosed(object sender, EventArgs e)
        {
            ClearMarginAdjustment();
            ResetTransform();
        }

        private void PositionParentPopupControl()
        {
            var popup = _parentPopupControl;
            if (popup != null)
            {
                Debug.Assert(IsShadowEnabled);

                CustomPlacementMode? placement = null;

                switch (popup.Placement)
                {
                    case PlacementMode.Bottom:
                        placement = CustomPlacementMode.BottomEdgeAlignedLeft;
                        break;
                    case PlacementMode.Top:
                        placement = CustomPlacementMode.TopEdgeAlignedLeft;
                        break;
                    case PlacementMode.Custom:
                        if (TryGetCustomPlacementMode(out var customPlacement))
                        {
                            placement = customPlacement;
                        }
                        break;
                }

                if (placement.HasValue)
                {
                    if (!EnsureEdgesAligned(placement.Value))
                    {
                        if (placement == CustomPlacementMode.BottomEdgeAlignedLeft)
                        {
                            if (shouldAlignRightEdges())
                            {
                                EnsureEdgesAligned(CustomPlacementMode.BottomEdgeAlignedRight);
                            }
                        }
                        else if (placement == CustomPlacementMode.TopEdgeAlignedLeft)
                        {
                            if (shouldAlignRightEdges())
                            {
                                EnsureEdgesAligned(CustomPlacementMode.TopEdgeAlignedRight);
                            }
                        }
                    }
                }

                bool shouldAlignRightEdges()
                {
                    var target = popup.PlacementTarget;
                    if (target != null)
                    {
                        var targetWidth = target.RenderSize.Width;
                        if (ActualWidth > 0 && targetWidth > 0)
                        {
                            if (ActualWidth == targetWidth)
                            {
                                return true;
                            }
                            else if (ActualWidth > targetWidth)
                            {
                                if (TryGetOffsetToTarget(InterestPoint.TopRight, InterestPoint.TopRight, out Vector offset))
                                {
                                    if (offset.X < 0)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }
            }
        }

        private bool TryGetCustomPlacementMode(out CustomPlacementMode placement)
        {
            if (TryGetCustomPlacementMode(_parentPopupControl?.Control, out placement))
            {
                return true;
            }
            if (TryGetCustomPlacementMode(VisualParent, out placement))
            {
                return true;
            }
            return false;
        }

        private bool TryGetCustomPlacementMode(DependencyObject element, out CustomPlacementMode placement)
        {
            if (element != null &&
                element.ReadLocalValue(CustomPopupPlacementHelper.PlacementProperty) != DependencyProperty.UnsetValue)
            {
                placement = CustomPopupPlacementHelper.GetPlacement(element);
                return true;
            }

            placement = default;
            return false;
        }

        private bool TryGetOffsetToTarget(
            InterestPoint targetInterestPoint,
            InterestPoint childInterestPoint,
            out Vector offset)
        {
            var popup = _parentPopupControl;
            if (popup != null)
            {
                var target = popup.PlacementTarget;
                if (target != null)
                {
                    if (IsVisible && target.IsVisible)
                    {
                        offset = Helper.GetOffset(this, childInterestPoint, target, targetInterestPoint, popup.PlacementRectangle);

                        if (Math.Abs(offset.X) < 0.5)
                        {
                            offset.X = 0;
                        }

                        if (Math.Abs(offset.Y) < 0.5)
                        {
                            offset.Y = 0;
                        }

                        return true;
                    }
                }
            }

            offset = default;
            return false;
        }

        private bool EnsureEdgesAligned(CustomPlacementMode placement)
        {
            Vector offsetToTarget;
            Vector translation = s_noTranslation;

            switch (placement)
            {
                case CustomPlacementMode.TopEdgeAlignedLeft:
                    if (TryGetOffsetToTarget(InterestPoint.TopLeft, InterestPoint.BottomLeft, out offsetToTarget))
                    {
                        translation = getTranslation(true, true, offsetToTarget);
                    }
                    break;
                case CustomPlacementMode.TopEdgeAlignedRight:
                    if (TryGetOffsetToTarget(InterestPoint.TopRight, InterestPoint.BottomRight, out offsetToTarget))
                    {
                        translation = getTranslation(true, false, offsetToTarget);
                    }
                    break;
                case CustomPlacementMode.BottomEdgeAlignedLeft:
                    if (TryGetOffsetToTarget(InterestPoint.BottomLeft, InterestPoint.TopLeft, out offsetToTarget))
                    {
                        translation = getTranslation(false, true, offsetToTarget);
                    }
                    break;
                case CustomPlacementMode.BottomEdgeAlignedRight:
                    if (TryGetOffsetToTarget(InterestPoint.BottomRight, InterestPoint.TopRight, out offsetToTarget))
                    {
                        translation = getTranslation(false, false, offsetToTarget);
                    }
                    break;
            }

            if (translation != s_noTranslation)
            {
                SetupTransform(translation);
                return true;
            }
            else
            {
                ResetTransform();
                return false;
            }

            Vector getTranslation(bool top, bool left, Vector offset)
            {
                double offsetX = 0;
                double offsetY = 0;

                if (left && offset.X > 0 ||
                    !left && offset.X < 0 ||
                    Math.Abs(offset.X) < 0.5)
                {
                    offsetX = -offset.X;
                }

                if (top && offset.Y < PopupMargin.Top ||
                    !top && offset.Y > -PopupMargin.Bottom ||
                    Math.Abs(offset.Y) < 0.5)
                {
                    offsetY = -offset.Y;
                }

                return new Vector(offsetX, offsetY);
            }
        }

        private void SetupTransform(Vector translation)
        {
            if (_transform == null)
            {
                _transform = new TranslateTransform();
                RenderTransform = _transform;
            }
            _transform.X = translation.X;
            _transform.Y = translation.Y;
        }

        private void ResetTransform()
        {
            if (_transform != null)
            {
                _transform.ClearValue(TranslateTransform.XProperty);
                _transform.ClearValue(TranslateTransform.YProperty);
            }
        }

        private Popup FindParentPopup(FrameworkElement element)
        {
            var parent = element.Parent;
            if (parent is Popup popup)
            {
                return popup;
            }
            else if (parent is FrameworkElement fe)
            {
                return FindParentPopup(fe);
            }
            else
            {
                if (VisualTreeHelper.GetParent(element) is FrameworkElement visualParent)
                {
                    return FindParentPopup(visualParent);
                }
            }
            return null;
        }

        private class PopupControl : IDisposable
        {
            private ContextMenu _contextMenu;
            private ToolTip _toolTip;
            private Popup _popup;

            public PopupControl(ContextMenu contextMenu)
            {
                _contextMenu = contextMenu;
                _contextMenu.Opened += OnOpened;
                _contextMenu.Closed += OnClosed;
            }

            public PopupControl(ToolTip toolTip)
            {
                _toolTip = toolTip;
                _toolTip.Opened += OnOpened;
                _toolTip.Closed += OnClosed;
            }

            public PopupControl(Popup popup)
            {
                _popup = popup;
                _popup.Opened += OnOpened;
                _popup.Closed += OnClosed;
            }

            public FrameworkElement Control =>
                _contextMenu as FrameworkElement ??
                _toolTip as FrameworkElement ??
                _popup as FrameworkElement;

            public PlacementMode Placement
            {
                get
                {
                    if (_contextMenu != null)
                    {
                        return _contextMenu.Placement;
                    }
                    if (_toolTip != null)
                    {
                        return _toolTip.Placement;
                    }
                    if (_popup != null)
                    {
                        return _popup.Placement;
                    }
                    return default;
                }
            }

            public UIElement PlacementTarget
            {
                get
                {
                    if (_contextMenu != null)
                    {
                        return _contextMenu.PlacementTarget;
                    }
                    if (_toolTip != null)
                    {
                        return _toolTip.PlacementTarget;
                    }
                    if (_popup != null)
                    {
                        return _popup.PlacementTarget ??
                            VisualTreeHelper.GetParent(_popup) as UIElement;
                    }
                    return null;
                }
            }

            public Rect PlacementRectangle
            {
                get
                {
                    if (_contextMenu != null)
                    {
                        return _contextMenu.PlacementRectangle;
                    }
                    if (_toolTip != null)
                    {
                        return _toolTip.PlacementRectangle;
                    }
                    if (_popup != null)
                    {
                        return _popup.PlacementRectangle;
                    }
                    return Rect.Empty;
                }
            }

            public event EventHandler Opened;

            public event EventHandler Closed;

            public void Dispose()
            {
                if (_contextMenu != null)
                {
                    _contextMenu.Opened -= OnOpened;
                    _contextMenu.Closed -= OnClosed;
                    _contextMenu = null;
                }
                else if (_toolTip != null)
                {
                    _toolTip.Opened -= OnOpened;
                    _toolTip.Closed -= OnClosed;
                    _toolTip = null;
                }
                else if (_popup != null)
                {
                    _popup.Opened -= OnOpened;
                    _popup.Closed -= OnClosed;
                    _popup = null;
                }
            }

            private void OnOpened(object sender, EventArgs e)
            {
                Opened?.Invoke(this, e);
            }

            private void OnClosed(object sender, EventArgs e)
            {
                Closed?.Invoke(this, e);
            }
        }

        private readonly Grid _background;
        private readonly BitmapCache _bitmapCache;
        private ThemeShadowElement _shadow;
        private PopupControl _parentPopupControl;
        private TranslateTransform _transform;
        private PopupPositioner _popupPositioner;
        private UIElement _shadowOpacitySource;
        private bool _isShadowOpacityRenderingHooked;
        private bool _updatingShadowAlias;
        private bool _updatingTranslationZAlias;

        private static readonly Vector s_noTranslation = new Vector(0, 0);
        private static readonly DependencyPropertyDescriptor OpacityPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(UIElement.OpacityProperty, typeof(UIElement));

        private sealed class ThemeShadowElement : FrameworkElement
        {
            public double Depth
            {
                get => _depth;
                set
                {
                    if (Math.Abs(_depth - value) > 0.001)
                    {
                        _depth = value;
                        InvalidateVisual();
                    }
                }
            }

            public CornerRadius CornerRadius
            {
                get => _cornerRadius;
                set
                {
                    if (_cornerRadius != value)
                    {
                        _cornerRadius = value;
                        InvalidateVisual();
                    }
                }
            }

            public Size ContentSize
            {
                get => _contentSize;
                set
                {
                    if (_contentSize != value)
                    {
                        _contentSize = value;
                        InvalidateVisual();
                    }
                }
            }

            public Size LayoutSize
            {
                get => _layoutSize;
                set
                {
                    if (_layoutSize != value)
                    {
                        _layoutSize = value;
                        InvalidateMeasure();
                    }
                }
            }

            public Point ContentOrigin
            {
                get => _contentOrigin;
                set
                {
                    if (_contentOrigin != value)
                    {
                        _contentOrigin = value;
                        InvalidateVisual();
                    }
                }
            }

#if NET462_OR_NEWER
            protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
            {
                base.OnDpiChanged(oldDpi, newDpi);

                InvalidateVisual();
            }
#endif

            protected override Size MeasureOverride(Size availableSize)
            {
                return LayoutSize;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                var contentSize = ContentSize;
                if (contentSize.Width > 0 && contentSize.Height > 0)
                {
                    ThemeShadowRenderer.DrawShadow(
                        drawingContext,
                        contentSize,
                        CornerRadius,
                        Depth,
                        ThemeManager.GetActualTheme(this),
                        VisualTreeHelper.GetDpi(this),
                        ContentOrigin);
                }
            }

            private double _depth;
            private CornerRadius _cornerRadius;
            private Size _contentSize;
            private Size _layoutSize;
            private Point _contentOrigin;
        }

        internal static class ThemeShadowRenderer
        {
            internal static (double Ambient, double Directional) GetLayerOpacities(double depth, ElementTheme theme)
            {
                var recipe = ThemeShadowRecipe.FromDepth(depth, theme);
                return (recipe.AmbientOpacity, recipe.DirectionalOpacity);
            }

            public static Thickness GetPadding(double depth)
            {
                if (depth <= 0 || double.IsNaN(depth) || double.IsInfinity(depth))
                {
                    return new Thickness();
                }

                var profile = ThemeShadowProfile.FromDepth(depth);
                double left = 0;
                double top = 0;
                double right = 0;
                double bottom = 0;

                for (int i = 0; i < profile.Layers.Length; i++)
                {
                    var layer = profile.Layers[i];
                    left = Math.Max(left, layer.BlurRadius - layer.OffsetX);
                    top = Math.Max(top, layer.BlurRadius - layer.OffsetY);
                    right = Math.Max(right, layer.BlurRadius + layer.OffsetX);
                    bottom = Math.Max(bottom, layer.BlurRadius + layer.OffsetY);
                }

                return new Thickness(
                    Math.Ceiling(left),
                    Math.Ceiling(top),
                    Math.Ceiling(right),
                    Math.Ceiling(bottom));
            }

            internal static Thickness GetPopupPadding(ThemeShadowChromeWindowedPopupInsetMode insetMode, double depth)
            {
                switch (insetMode)
                {
                    case ThemeShadowChromeWindowedPopupInsetMode.Small:
                        return new Thickness(4, 1, 4, 8);
                    case ThemeShadowChromeWindowedPopupInsetMode.Medium:
                        return new Thickness(10, 2, 10, 18);
                    default:
                        return GetPadding(depth);
                }
            }

            internal static ThemeShadowRenderMetrics GetRenderMetrics(
                Size contentSize,
                CornerRadius cornerRadius,
                double depth,
                ElementTheme theme,
                DpiScale dpi)
            {
                if (depth <= 0 || double.IsNaN(depth) || double.IsInfinity(depth) ||
                    contentSize.Width <= 0 || contentSize.Height <= 0)
                {
                    return ThemeShadowRenderMetrics.Empty;
                }

                var padding = GetPadding(depth);
                var key = ThemeShadowKey.Create(contentSize, cornerRadius, depth, theme, padding, dpi);
                var image = GetShadowImage(contentSize, cornerRadius, depth, theme, padding, dpi);

                return MeasureShadowImage(image, key);
            }

            public static void DrawShadow(
                DrawingContext drawingContext,
                Size contentSize,
                CornerRadius cornerRadius,
                double depth,
                ElementTheme theme,
                DpiScale dpi,
                Point? contentOrigin = null)
            {
                if (depth <= 0 || double.IsNaN(depth) || double.IsInfinity(depth) ||
                    contentSize.Width <= 0 || contentSize.Height <= 0)
                {
                    return;
                }

                var padding = GetPadding(depth);
                var image = GetShadowImage(contentSize, cornerRadius, depth, theme, padding, dpi);
                var imageWidth = image.PixelWidth / dpi.DpiScaleX;
                var imageHeight = image.PixelHeight / dpi.DpiScaleY;
                var origin = contentOrigin ?? new Point();

                drawingContext.DrawImage(
                    image,
                    new Rect(
                        origin.X - padding.Left,
                        origin.Y - padding.Top,
                        imageWidth,
                        imageHeight));
            }

            private static BitmapSource GetShadowImage(Size contentSize, CornerRadius cornerRadius, double depth, ElementTheme theme, Thickness padding, DpiScale dpi)
            {
                var key = ThemeShadowKey.Create(contentSize, cornerRadius, depth, theme, padding, dpi);

                lock (s_cache)
                {
                    if (s_cache.TryGetValue(key, out var cached))
                    {
                        return cached;
                    }
                }

                var image = RenderShadowImage(key, cornerRadius);
                image.Freeze();

                lock (s_cache)
                {
                    if (s_cache.Count >= MaxCacheEntries)
                    {
                        s_cache.Clear();
                    }

                    s_cache[key] = image;
                }

                return image;
            }

            private static ThemeShadowRenderMetrics MeasureShadowImage(BitmapSource image, ThemeShadowKey key)
            {
                int stride = image.PixelWidth * 4;
                var pixels = new byte[stride * image.PixelHeight];
                image.CopyPixels(pixels, stride, 0);

                int minX = image.PixelWidth;
                int minY = image.PixelHeight;
                int maxX = -1;
                int maxY = -1;
                int peakAlpha = 0;
                int nonZeroPixelCount = 0;
                long alphaSum = 0;
                long weightedAlphaX = 0;
                long weightedAlphaY = 0;

                for (int y = 0; y < image.PixelHeight; y++)
                {
                    int row = y * stride;
                    for (int x = 0; x < image.PixelWidth; x++)
                    {
                        int alpha = pixels[row + x * 4 + 3];
                        if (alpha == 0)
                        {
                            continue;
                        }

                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                        peakAlpha = Math.Max(peakAlpha, alpha);
                        nonZeroPixelCount++;
                        alphaSum += alpha;
                        weightedAlphaX += (long)alpha * x;
                        weightedAlphaY += (long)alpha * y;
                    }
                }

                var nonZeroBounds = nonZeroPixelCount == 0
                    ? Int32Rect.Empty
                    : new Int32Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
                double alphaCentroidX = alphaSum == 0 ? double.NaN : (double)weightedAlphaX / alphaSum;
                double alphaCentroidY = alphaSum == 0 ? double.NaN : (double)weightedAlphaY / alphaSum;

                return new ThemeShadowRenderMetrics(
                    image.PixelWidth,
                    image.PixelHeight,
                    key.PaddingLeft,
                    key.PaddingTop,
                    key.ContentWidth,
                    key.ContentHeight,
                    peakAlpha,
                    nonZeroPixelCount,
                    nonZeroBounds,
                    alphaCentroidX,
                    alphaCentroidY);
            }

            private static BitmapSource RenderShadowImage(ThemeShadowKey key, CornerRadius cornerRadius)
            {
                int pixelCount = key.BitmapWidth * key.BitmapHeight;
                var outputAlpha = new byte[pixelCount];
                var sourceAlpha = new byte[pixelCount];
                var blurredAlpha = new byte[pixelCount];
                var profile = ThemeShadowProfile.FromDepth(key.Depth, key.IsDarkTheme ? ElementTheme.Dark : ElementTheme.Light);

                for (int i = 0; i < profile.Layers.Length; i++)
                {
                    var layer = profile.Layers[i];
                    if (layer.Opacity <= 0)
                    {
                        continue;
                    }

                    Array.Clear(sourceAlpha, 0, sourceAlpha.Length);
                    Array.Clear(blurredAlpha, 0, blurredAlpha.Length);

                    FillRoundedRectMask(sourceAlpha, key, cornerRadius, layer.OffsetX, layer.OffsetY);
                    Blur(sourceAlpha, blurredAlpha, key.BitmapWidth, key.BitmapHeight, DipToPixel(layer.BlurRadius, key.DpiScaleX));
                    CompositeAlpha(outputAlpha, blurredAlpha, layer.Opacity);
                }

                ClearRoundedRectMask(outputAlpha, key, cornerRadius);

                var pixels = new byte[pixelCount * 4];
                for (int i = 0, pixelOffset = 0; i < outputAlpha.Length; i++, pixelOffset += 4)
                {
                    pixels[pixelOffset + 3] = outputAlpha[i];
                }

                int stride = key.BitmapWidth * 4;
                return BitmapSource.Create(
                    key.BitmapWidth,
                    key.BitmapHeight,
                    96 * key.DpiScaleX,
                    96 * key.DpiScaleY,
                    PixelFormats.Pbgra32,
                    null,
                    pixels,
                    stride);
            }

            private static void FillRoundedRectMask(byte[] alpha, ThemeShadowKey key, CornerRadius cornerRadius, double offsetX, double offsetY)
            {
                double left = key.PaddingLeft + offsetX * key.DpiScaleX;
                double top = key.PaddingTop + offsetY * key.DpiScaleY;
                double width = key.ContentWidth;
                double height = key.ContentHeight;

                var radii = CornerRadii.From(cornerRadius, key.DpiScaleX, key.DpiScaleY, width, height);
                int minX = Math.Max(0, (int)Math.Floor(left));
                int minY = Math.Max(0, (int)Math.Floor(top));
                int maxX = Math.Min(key.BitmapWidth, (int)Math.Ceiling(left + width));
                int maxY = Math.Min(key.BitmapHeight, (int)Math.Ceiling(top + height));

                for (int y = minY; y < maxY; y++)
                {
                    double py = y + 0.5;
                    int row = y * key.BitmapWidth;

                    for (int x = minX; x < maxX; x++)
                    {
                        double px = x + 0.5;
                        if (IsInsideRoundedRect(px, py, left, top, width, height, radii))
                        {
                            alpha[row + x] = 255;
                        }
                    }
                }
            }

            private static void ClearRoundedRectMask(byte[] alpha, ThemeShadowKey key, CornerRadius cornerRadius)
            {
                double left = key.PaddingLeft;
                double top = key.PaddingTop;
                double width = key.ContentWidth;
                double height = key.ContentHeight;

                var radii = CornerRadii.From(cornerRadius, key.DpiScaleX, key.DpiScaleY, width, height);
                int minX = Math.Max(0, (int)Math.Floor(left));
                int minY = Math.Max(0, (int)Math.Floor(top));
                int maxX = Math.Min(key.BitmapWidth, (int)Math.Ceiling(left + width));
                int maxY = Math.Min(key.BitmapHeight, (int)Math.Ceiling(top + height));

                for (int y = minY; y < maxY; y++)
                {
                    double py = y + 0.5;
                    int row = y * key.BitmapWidth;

                    for (int x = minX; x < maxX; x++)
                    {
                        double px = x + 0.5;
                        if (IsInsideRoundedRect(px, py, left, top, width, height, radii))
                        {
                            alpha[row + x] = 0;
                        }
                    }
                }
            }

            private static bool IsInsideRoundedRect(double x, double y, double left, double top, double width, double height, CornerRadii radii)
            {
                double right = left + width;
                double bottom = top + height;

                if (x < left || x >= right || y < top || y >= bottom)
                {
                    return false;
                }

                if (x < left + radii.TopLeft && y < top + radii.TopLeft)
                {
                    return IsInsideCorner(x, y, left + radii.TopLeft, top + radii.TopLeft, radii.TopLeft);
                }

                if (x >= right - radii.TopRight && y < top + radii.TopRight)
                {
                    return IsInsideCorner(x, y, right - radii.TopRight, top + radii.TopRight, radii.TopRight);
                }

                if (x >= right - radii.BottomRight && y >= bottom - radii.BottomRight)
                {
                    return IsInsideCorner(x, y, right - radii.BottomRight, bottom - radii.BottomRight, radii.BottomRight);
                }

                if (x < left + radii.BottomLeft && y >= bottom - radii.BottomLeft)
                {
                    return IsInsideCorner(x, y, left + radii.BottomLeft, bottom - radii.BottomLeft, radii.BottomLeft);
                }

                return true;
            }

            private static bool IsInsideCorner(double x, double y, double centerX, double centerY, double radius)
            {
                if (radius <= 0)
                {
                    return true;
                }

                double dx = x - centerX;
                double dy = y - centerY;
                return dx * dx + dy * dy <= radius * radius;
            }

            private static void Blur(byte[] source, byte[] target, int width, int height, int radius)
            {
                if (radius <= 0)
                {
                    Buffer.BlockCopy(source, 0, target, 0, source.Length);
                    return;
                }

                var kernel = CreateGaussianKernel(radius);
                var temp = new double[source.Length];

                for (int y = 0; y < height; y++)
                {
                    int row = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        double sum = 0;
                        for (int k = -radius; k <= radius; k++)
                        {
                            int sampleX = Clamp(x + k, 0, width - 1);
                            sum += source[row + sampleX] * kernel[k + radius];
                        }

                        temp[row + x] = sum;
                    }
                }

                for (int y = 0; y < height; y++)
                {
                    int row = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        double sum = 0;
                        for (int k = -radius; k <= radius; k++)
                        {
                            int sampleY = Clamp(y + k, 0, height - 1);
                            sum += temp[sampleY * width + x] * kernel[k + radius];
                        }

                        target[row + x] = (byte)Clamp((int)Math.Round(sum), 0, 255);
                    }
                }
            }

            private static double[] CreateGaussianKernel(int radius)
            {
                var kernel = new double[radius * 2 + 1];
                double sigma = Math.Max(0.5, radius / 3.0);
                double twoSigmaSquared = 2 * sigma * sigma;
                double sum = 0;

                for (int i = -radius; i <= radius; i++)
                {
                    double value = Math.Exp(-(i * i) / twoSigmaSquared);
                    kernel[i + radius] = value;
                    sum += value;
                }

                for (int i = 0; i < kernel.Length; i++)
                {
                    kernel[i] /= sum;
                }

                return kernel;
            }

            private static void CompositeAlpha(byte[] outputAlpha, byte[] layerAlpha, double opacity)
            {
                for (int i = 0; i < outputAlpha.Length; i++)
                {
                    double source = layerAlpha[i] * opacity / 255.0;
                    double destination = outputAlpha[i] / 255.0;
                    outputAlpha[i] = (byte)Clamp((int)Math.Round((source + destination * (1 - source)) * 255), 0, 255);
                }
            }

            private static int DipToPixel(double value, double scale)
            {
                return Math.Max(0, (int)Math.Ceiling(value * scale));
            }

            private static int Clamp(int value, int min, int max)
            {
                if (value < min)
                {
                    return min;
                }

                if (value > max)
                {
                    return max;
                }

                return value;
            }

            private const int MaxCacheEntries = 64;
            private static readonly Dictionary<ThemeShadowKey, BitmapSource> s_cache = new Dictionary<ThemeShadowKey, BitmapSource>();

            internal readonly struct ThemeShadowRenderMetrics
            {
                public ThemeShadowRenderMetrics(
                    int bitmapWidth,
                    int bitmapHeight,
                    int contentLeft,
                    int contentTop,
                    int contentWidth,
                    int contentHeight,
                    int peakAlpha,
                    int nonZeroPixelCount,
                    Int32Rect nonZeroBounds,
                    double alphaCentroidX,
                    double alphaCentroidY)
                {
                    BitmapWidth = bitmapWidth;
                    BitmapHeight = bitmapHeight;
                    ContentLeft = contentLeft;
                    ContentTop = contentTop;
                    ContentWidth = contentWidth;
                    ContentHeight = contentHeight;
                    PeakAlpha = peakAlpha;
                    NonZeroPixelCount = nonZeroPixelCount;
                    NonZeroBounds = nonZeroBounds;
                    AlphaCentroidX = alphaCentroidX;
                    AlphaCentroidY = alphaCentroidY;
                }

                public int BitmapWidth { get; }
                public int BitmapHeight { get; }
                public int ContentLeft { get; }
                public int ContentTop { get; }
                public int ContentWidth { get; }
                public int ContentHeight { get; }
                public int PeakAlpha { get; }
                public int NonZeroPixelCount { get; }
                public Int32Rect NonZeroBounds { get; }
                public double AlphaCentroidX { get; }
                public double AlphaCentroidY { get; }
                public bool HasShadow => NonZeroPixelCount > 0;
                public double ContentCenterX => ContentLeft + ContentWidth / 2.0;
                public double ContentCenterY => ContentTop + ContentHeight / 2.0;

                public static ThemeShadowRenderMetrics Empty { get; } =
                    new ThemeShadowRenderMetrics(0, 0, 0, 0, 0, 0, 0, 0, Int32Rect.Empty, double.NaN, double.NaN);
            }

            private readonly struct ThemeShadowLayer
            {
                public ThemeShadowLayer(double blurRadius, double offsetX, double offsetY, double opacity)
                {
                    BlurRadius = blurRadius;
                    OffsetX = offsetX;
                    OffsetY = offsetY;
                    Opacity = opacity;
                }

                public double BlurRadius { get; }
                public double OffsetX { get; }
                public double OffsetY { get; }
                public double Opacity { get; }
            }

            private readonly struct ThemeShadowProfile
            {
                private ThemeShadowProfile(ThemeShadowLayer[] layers)
                {
                    Layers = layers;
                }

                public ThemeShadowLayer[] Layers { get; }

                public static ThemeShadowProfile FromDepth(double depth, ElementTheme theme = ElementTheme.Light)
                {
                    var recipe = ThemeShadowRecipe.FromDepth(depth, theme);

                    return new ThemeShadowProfile(new[]
                    {
                        new ThemeShadowLayer(recipe.AmbientBlurRadius, 0, recipe.AmbientYOffset, recipe.AmbientOpacity),
                        new ThemeShadowLayer(recipe.DirectionalBlurRadius, 0, recipe.DirectionalYOffset, recipe.DirectionalOpacity)
                    });
                }
            }

            private readonly struct ThemeShadowRecipe
            {
                private ThemeShadowRecipe(
                    double ambientBlurRadius,
                    double directionalBlurRadius,
                    double ambientYOffset,
                    double directionalYOffset,
                    double ambientOpacity,
                    double directionalOpacity)
                {
                    AmbientBlurRadius = ambientBlurRadius;
                    DirectionalBlurRadius = directionalBlurRadius;
                    AmbientYOffset = ambientYOffset;
                    DirectionalYOffset = directionalYOffset;
                    AmbientOpacity = ambientOpacity;
                    DirectionalOpacity = directionalOpacity;
                }

                public double AmbientBlurRadius { get; }
                public double DirectionalBlurRadius { get; }
                public double AmbientYOffset { get; }
                public double DirectionalYOffset { get; }
                public double AmbientOpacity { get; }
                public double DirectionalOpacity { get; }

                public static ThemeShadowRecipe FromDepth(double depth, ElementTheme theme)
                {
                    double elevation = Math.Min(64, Math.Max(0, depth) / 2);
                    double ambientBlurRadius;
                    double directionalBlurRadius;
                    double ambientYOffset = 0;
                    double directionalYOffset;
                    double ambientOpacity = 0;
                    double directionalOpacity = 0;
                    bool isDarkTheme = theme == ElementTheme.Dark;

                    if (elevation < 2)
                    {
                        ambientBlurRadius = 2;
                    }
                    else if (elevation <= 16)
                    {
                        ambientBlurRadius = 2;
                        directionalOpacity = isDarkTheme ? 0.26 : Math.Min((elevation / 100) + 0.06, 0.14);
                    }
                    else
                    {
                        ambientBlurRadius = elevation / 3;
                        ambientYOffset = 2;

                        if (isDarkTheme)
                        {
                            ambientOpacity = 0.37;
                            directionalOpacity = 0.37;
                        }
                        else
                        {
                            ambientOpacity = 0.15;
                            directionalOpacity = 0.19;
                        }
                    }

                    directionalBlurRadius = elevation;
                    directionalYOffset = elevation * 0.5;

                    return new ThemeShadowRecipe(
                        ambientBlurRadius,
                        directionalBlurRadius,
                        ambientYOffset,
                        directionalYOffset,
                        ambientOpacity,
                        directionalOpacity);
                }
            }

            private readonly struct ThemeShadowKey : IEquatable<ThemeShadowKey>
            {
                private ThemeShadowKey(
                    int bitmapWidth,
                    int bitmapHeight,
                    int contentWidth,
                    int contentHeight,
                    int paddingLeft,
                    int paddingTop,
                    double depth,
                    bool isDarkTheme,
                    double dpiScaleX,
                    double dpiScaleY,
                    double topLeft,
                    double topRight,
                    double bottomRight,
                    double bottomLeft)
                {
                    BitmapWidth = bitmapWidth;
                    BitmapHeight = bitmapHeight;
                    ContentWidth = contentWidth;
                    ContentHeight = contentHeight;
                    PaddingLeft = paddingLeft;
                    PaddingTop = paddingTop;
                    Depth = depth;
                    IsDarkTheme = isDarkTheme;
                    DpiScaleX = dpiScaleX;
                    DpiScaleY = dpiScaleY;
                    TopLeft = topLeft;
                    TopRight = topRight;
                    BottomRight = bottomRight;
                    BottomLeft = bottomLeft;
                }

                public int BitmapWidth { get; }
                public int BitmapHeight { get; }
                public int ContentWidth { get; }
                public int ContentHeight { get; }
                public int PaddingLeft { get; }
                public int PaddingTop { get; }
                public double Depth { get; }
                public bool IsDarkTheme { get; }
                public double DpiScaleX { get; }
                public double DpiScaleY { get; }
                private double TopLeft { get; }
                private double TopRight { get; }
                private double BottomRight { get; }
                private double BottomLeft { get; }

                public static ThemeShadowKey Create(Size contentSize, CornerRadius cornerRadius, double depth, ElementTheme theme, Thickness padding, DpiScale dpi)
                {
                    int contentWidth = Math.Max(1, (int)Math.Ceiling(contentSize.Width * dpi.DpiScaleX));
                    int contentHeight = Math.Max(1, (int)Math.Ceiling(contentSize.Height * dpi.DpiScaleY));
                    int paddingLeft = Math.Max(0, (int)Math.Ceiling(padding.Left * dpi.DpiScaleX));
                    int paddingTop = Math.Max(0, (int)Math.Ceiling(padding.Top * dpi.DpiScaleY));
                    int paddingRight = Math.Max(0, (int)Math.Ceiling(padding.Right * dpi.DpiScaleX));
                    int paddingBottom = Math.Max(0, (int)Math.Ceiling(padding.Bottom * dpi.DpiScaleY));

                    return new ThemeShadowKey(
                        contentWidth + paddingLeft + paddingRight,
                        contentHeight + paddingTop + paddingBottom,
                        contentWidth,
                        contentHeight,
                        paddingLeft,
                        paddingTop,
                        Math.Round(depth, 2),
                        theme == ElementTheme.Dark,
                        Math.Round(dpi.DpiScaleX, 3),
                        Math.Round(dpi.DpiScaleY, 3),
                        Math.Round(cornerRadius.TopLeft, 2),
                        Math.Round(cornerRadius.TopRight, 2),
                        Math.Round(cornerRadius.BottomRight, 2),
                        Math.Round(cornerRadius.BottomLeft, 2));
                }

                public bool Equals(ThemeShadowKey other)
                {
                    return BitmapWidth == other.BitmapWidth &&
                        BitmapHeight == other.BitmapHeight &&
                        ContentWidth == other.ContentWidth &&
                        ContentHeight == other.ContentHeight &&
                        PaddingLeft == other.PaddingLeft &&
                        PaddingTop == other.PaddingTop &&
                        Depth.Equals(other.Depth) &&
                        IsDarkTheme == other.IsDarkTheme &&
                        DpiScaleX.Equals(other.DpiScaleX) &&
                        DpiScaleY.Equals(other.DpiScaleY) &&
                        TopLeft.Equals(other.TopLeft) &&
                        TopRight.Equals(other.TopRight) &&
                        BottomRight.Equals(other.BottomRight) &&
                        BottomLeft.Equals(other.BottomLeft);
                }

                public override bool Equals(object obj)
                {
                    return obj is ThemeShadowKey other && Equals(other);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        int hash = BitmapWidth;
                        hash = (hash * 397) ^ BitmapHeight;
                        hash = (hash * 397) ^ ContentWidth;
                        hash = (hash * 397) ^ ContentHeight;
                        hash = (hash * 397) ^ PaddingLeft;
                        hash = (hash * 397) ^ PaddingTop;
                        hash = (hash * 397) ^ Depth.GetHashCode();
                        hash = (hash * 397) ^ IsDarkTheme.GetHashCode();
                        hash = (hash * 397) ^ DpiScaleX.GetHashCode();
                        hash = (hash * 397) ^ DpiScaleY.GetHashCode();
                        hash = (hash * 397) ^ TopLeft.GetHashCode();
                        hash = (hash * 397) ^ TopRight.GetHashCode();
                        hash = (hash * 397) ^ BottomRight.GetHashCode();
                        hash = (hash * 397) ^ BottomLeft.GetHashCode();
                        return hash;
                    }
                }
            }

            private readonly struct CornerRadii
            {
                private CornerRadii(double topLeft, double topRight, double bottomRight, double bottomLeft)
                {
                    TopLeft = topLeft;
                    TopRight = topRight;
                    BottomRight = bottomRight;
                    BottomLeft = bottomLeft;
                }

                public double TopLeft { get; }
                public double TopRight { get; }
                public double BottomRight { get; }
                public double BottomLeft { get; }

                public static CornerRadii From(CornerRadius cornerRadius, double dpiScaleX, double dpiScaleY, double width, double height)
                {
                    double topLeft = Math.Max(0, cornerRadius.TopLeft * dpiScaleX);
                    double topRight = Math.Max(0, cornerRadius.TopRight * dpiScaleX);
                    double bottomRight = Math.Max(0, cornerRadius.BottomRight * dpiScaleX);
                    double bottomLeft = Math.Max(0, cornerRadius.BottomLeft * dpiScaleX);

                    double scale = 1;
                    scale = Math.Min(scale, ScaleFor(width, topLeft + topRight));
                    scale = Math.Min(scale, ScaleFor(width, bottomLeft + bottomRight));
                    scale = Math.Min(scale, ScaleFor(height, topLeft + bottomLeft));
                    scale = Math.Min(scale, ScaleFor(height, topRight + bottomRight));

                    return new CornerRadii(
                        topLeft * scale,
                        topRight * scale,
                        bottomRight * scale,
                        bottomLeft * scale);
                }

                private static double ScaleFor(double available, double requested)
                {
                    return requested > available && requested > 0 ? available / requested : 1;
                }
            }
        }
    }
}
