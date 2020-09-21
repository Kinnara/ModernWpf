using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace ModernWpf.Controls.Primitives
{
    public class ThemeShadowChrome : Decorator
    {
        static ThemeShadowChrome()
        {
            s_bg1 = new SolidColorBrush(Colors.Black) { Opacity = 0.11 };
            s_bg2 = new SolidColorBrush(Colors.Black) { Opacity = 0.13 };
            s_bg3 = new SolidColorBrush(Colors.Black) { Opacity = 0.18 };
            s_bg4 = new SolidColorBrush(Colors.Black) { Opacity = 0.22 };

            s_bg1.Freeze();
            s_bg2.Freeze();
            s_bg3.Freeze();
            s_bg4.Freeze();
        }

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

            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
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
            if (IsInitialized)
            {
                if (IsShadowEnabled)
                {
                    EnsureShadows();
                    Debug.Assert(_background.Children.Count == 0);
                    _background.Children.Add(_shadow1);
                    _background.Children.Add(_shadow2);
                    _background.Visibility = Visibility.Visible;
                }
                else
                {
                    _background.Children.Clear();
                    _background.Visibility = Visibility.Collapsed;
                }

                OnVisualParentChanged();
                UpdatePopupMargin();
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

        private void OnDepthChanged()
        {
            if (IsInitialized)
            {
                UpdateShadow1();
                UpdateShadow2();
                UpdatePopupMargin();
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

            if (_shadow1 != null)
            {
                _shadow1.CornerRadius = cornerRadius;
            }

            if (_shadow2 != null)
            {
                _shadow2.CornerRadius = cornerRadius;
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
            ApplyPopupMargin();
        }

        private void UpdatePopupMargin()
        {
            if (IsShadowEnabled)
            {
                double depth = Depth;
                double radius = 0.9 * depth;
                double offset = 0.4 * depth;

                PopupMargin = new Thickness(
                    radius,
                    radius,
                    radius,
                    radius + offset);
            }
            else
            {
                ClearValue(PopupMarginProperty);
            }
        }

        private void ApplyPopupMargin()
        {
            if (_parentPopupControl != null)
            {
                if (ReadLocalValue(PopupMarginProperty) == DependencyProperty.UnsetValue)
                {
                    _parentPopupControl.ClearMargin();
                }
                else
                {
                    _parentPopupControl.SetMargin(PopupMargin);
                }
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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            OnIsShadowEnabledChanged();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (IsShadowEnabled)
            {
                _background.Measure(constraint);
            }

            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (IsShadowEnabled)
            {
                _background.Arrange(new Rect(arrangeSize));
            }

            return base.ArrangeOverride(arrangeSize);
        }

#if NET462_OR_NEWER
        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            _bitmapCache.RenderAtScale = newDpi.PixelsPerDip;
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

        private void EnsureShadows()
        {
            if (_shadow1 == null)
            {
                _shadow1 = CreateShadowElement();
                UpdateShadow1();
            }

            if (_shadow2 == null)
            {
                _shadow2 = CreateShadowElement();
                UpdateShadow2();
            }
        }

        private Border CreateShadowElement()
        {
            return new Border
            {
                CornerRadius = CornerRadius,
                Effect = new BlurEffect(),
                RenderTransform = new TranslateTransform()
            };
        }

        private void UpdateShadow1()
        {
            if (_shadow1 != null)
            {
                double depth = Depth;

                var effect = (BlurEffect)_shadow1.Effect;
                effect.Radius = 0.9 * depth;

                var transform = (TranslateTransform)_shadow1.RenderTransform;
                transform.Y = 0.4 * depth;

                _shadow1.Background = depth >= 32 ? s_bg4 : s_bg2;
            }
        }

        private void UpdateShadow2()
        {
            if (_shadow2 != null)
            {
                double depth = Depth;

                var effect = (BlurEffect)_shadow2.Effect;
                effect.Radius = 0.225 * depth;

                var transform = (TranslateTransform)_shadow2.RenderTransform;
                transform.Y = 0.075 * depth;

                _shadow2.Background = depth >= 32 ? s_bg3 : s_bg1;
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
            if (IsVisible)
            {
                AdjustMargin();
            }
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
                _parentPopupControl.ClearMargin();
                _parentPopupControl.Dispose();
            }

            _parentPopupControl = value as PopupControl;

            if (_parentPopupControl != null)
            {
                _parentPopupControl.Opened += OnParentPopupControlOpened;
                _parentPopupControl.Closed += OnParentPopupControlClosed;
                ApplyPopupMargin();
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

            private FrameworkElement ChildAsFE =>
                _contextMenu as FrameworkElement ??
                _toolTip as FrameworkElement ??
                _popup?.Child as FrameworkElement;

            public event EventHandler Opened;

            public event EventHandler Closed;

            public void SetMargin(Thickness margin)
            {
                var child = ChildAsFE;
                if (child != null)
                {
                    child.Margin = margin;
                }
            }

            public void ClearMargin()
            {
                ChildAsFE?.ClearValue(MarginProperty);
            }

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
        private Border _shadow1;
        private Border _shadow2;
        private PopupControl _parentPopupControl;
        private TranslateTransform _transform;
        private PopupPositioner _popupPositioner;

        private static readonly Brush s_bg1, s_bg2, s_bg3, s_bg4;
        private static readonly Vector s_noTranslation = new Vector(0, 0);
    }
}
