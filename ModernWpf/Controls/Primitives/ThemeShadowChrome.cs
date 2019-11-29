using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
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

            s_bitmapCache = new BitmapCache
            {
                SnapsToDevicePixels = true
            };
            s_bitmapCache.Freeze();
        }

        public ThemeShadowChrome()
        {
            _background = new Grid { CacheMode = s_bitmapCache };
            AddVisualChild(_background);
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
                AdjustLayout();
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
                AdjustLayout();
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

        #region DesiredMargin

        private static readonly DependencyProperty DesiredMarginProperty =
            DependencyProperty.Register(
                nameof(DesiredMargin),
                typeof(Thickness),
                typeof(ThemeShadowChrome),
                new PropertyMetadata(new Thickness(), OnDesiredMarginChanged));

        private Thickness DesiredMargin
        {
            get => (Thickness)GetValue(DesiredMarginProperty);
            set => SetValue(DesiredMarginProperty, value);
        }

        private static void OnDesiredMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeShadowChrome)d).OnDesiredMarginChanged(e);
        }

        private void OnDesiredMarginChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateDesiredPopupOffsets();
        }

        #endregion

        #region DesiredPopupHorizontalOffset

        private static readonly DependencyProperty DesiredPopupHorizontalOffsetProperty =
            DependencyProperty.Register(
                nameof(DesiredPopupHorizontalOffset),
                typeof(double),
                typeof(ThemeShadowChrome),
                new PropertyMetadata(0d));

        private double DesiredPopupHorizontalOffset
        {
            get => (double)GetValue(DesiredPopupHorizontalOffsetProperty);
            set => SetValue(DesiredPopupHorizontalOffsetProperty, value);
        }

        #endregion

        #region DesiredPopupVerticalOffset

        private static readonly DependencyProperty DesiredPopupVerticalOffsetProperty =
            DependencyProperty.Register(
                nameof(DesiredPopupVerticalOffset),
                typeof(double),
                typeof(ThemeShadowChrome),
                new PropertyMetadata(0d));

        private double DesiredPopupVerticalOffset
        {
            get => (double)GetValue(DesiredPopupVerticalOffsetProperty);
            set => SetValue(DesiredPopupVerticalOffsetProperty, value);
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

            _shouldUpdateMargin = HasDefaultValue(this, MarginProperty);

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
                else
                {
                    if (FindParentPopup(this) is Popup parentPopup)
                    {
                        bool shouldSetOffsets = true;
                        bool edgeAlignedLeft = false;

                        if (TemplatedParent is MenuItem menuItem)
                        {
                            shouldSetOffsets = false;

                            if (menuItem.Role == MenuItemRole.TopLevelHeader)
                            {
                                edgeAlignedLeft = true;
                            }
                        }
                        else if (visualParent is Calendar)
                        {
                            edgeAlignedLeft = true;
                        }

                        parentPopupControl = new PopupControl(parentPopup)
                        {
                            ShouldSetOffsets = shouldSetOffsets,
                            EdgeAlignedLeft = edgeAlignedLeft
                        };
                    }
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
            _shadow1 = CreateShadowElement();
            UpdateShadow1();

            _shadow2 = CreateShadowElement();
            UpdateShadow2();
        }

        private Border CreateShadowElement()
        {
            return new Border
            {
                Background = Brushes.Black,
                Margin = new Thickness(c_shadowMargin),
                CornerRadius = CornerRadius,
                Effect = new DropShadowEffect
                {
                    Direction = 270,
                    Color = Colors.Black
                }
            };
        }

        private void UpdateShadow1()
        {
            if (_shadow1 != null)
            {
                double depth = Depth;
                var effect = (DropShadowEffect)_shadow1.Effect;
                effect.ShadowDepth = 0.4 * depth + c_shadowMargin;
                effect.BlurRadius = 0.9 * depth + c_shadowMargin;
                _shadow1.Background = depth >= 32 ? s_bg4 : s_bg3;
            }
        }

        private void UpdateShadow2()
        {
            if (_shadow2 != null)
            {
                double depth = Depth;
                var effect = (DropShadowEffect)_shadow2.Effect;
                effect.ShadowDepth = 0.08 * depth + c_shadowMargin;
                effect.BlurRadius = 0.22 * depth + c_shadowMargin;
                _shadow2.Background = depth >= 32 ? s_bg2 : s_bg1;
            }
        }

        private void AdjustLayout()
        {
            if (IsShadowEnabled)
            {
                double depth = Depth;
                double radius = 0.9 * depth;
                double offset = 0.4 * depth;

                DesiredMargin = new Thickness(
                    radius,
                    radius,
                    radius,
                    radius + offset);
            }
            else
            {
                DesiredMargin = new Thickness();
            }

            UpdateMargin();
        }

        private void UpdateMargin()
        {
            if (_shouldUpdateMargin)
            {
                if (IsShadowEnabled)
                {
                    Margin = DesiredMargin;
                }
                else
                {
                    ClearValue(MarginProperty);
                }
            }
        }

        private void SetParentPopupControl(PopupControl value)
        {
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
            UpdateDesiredPopupOffsets();
            PositionParentPopupControl();
        }

        private void OnParentPopupControlClosed(object sender, EventArgs e)
        {
            ((PopupControl)sender).ClearOffsets();
            ResetTransform();
        }

        private void PositionParentPopupControl()
        {
            if (_parentPopupControl != null)
            {
                Debug.Assert(IsShadowEnabled);
                _parentPopupControl.SetOffsets(DesiredPopupHorizontalOffset, DesiredPopupVerticalOffset);

                if (_parentPopupControl.Placement == PlacementMode.Custom)
                {
                    if (TryGetCustomPlacementMode(out var placement))
                    {
                        switch (placement)
                        {
                            case CustomPopupPlacementMode.TopEdgeAlignedLeft:
                            case CustomPopupPlacementMode.BottomEdgeAlignedLeft:
                                EnsureEdgeAligned(true);
                                break;
                            case CustomPopupPlacementMode.TopEdgeAlignedRight:
                            case CustomPopupPlacementMode.BottomEdgeAlignedRight:
                                EnsureEdgeAligned(false);
                                break;
                        }
                    }
                }
                else if (_parentPopupControl.EdgeAlignedLeft)
                {
                    EnsureEdgeAligned(true);
                }
            }
        }

        private bool TryGetCustomPlacementMode(out CustomPopupPlacementMode placement)
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

        private bool TryGetCustomPlacementMode(DependencyObject element, out CustomPopupPlacementMode placement)
        {
            if (element != null &&
                element.ReadLocalValue(PopupPlacementHelper.PlacementProperty) != DependencyProperty.UnsetValue)
            {
                placement = PopupPlacementHelper.GetPlacement(element);
                return true;
            }

            placement = default;
            return false;
        }

        private bool TryGetOffsetToTarget(out Vector offset, bool leftEdge)
        {
            var popup = _parentPopupControl;
            if (popup != null)
            {
                var target = popup.PlacementTarget;
                if (target != null)
                {
                    if (IsVisible && target.IsVisible)
                    {
                        var popupPoint = PointToScreen(new Point(leftEdge ? 0 : ActualWidth, 0));
                        var targetPoint = target.PointToScreen(new Point(leftEdge ? 0 : target.RenderSize.Width, 0));
                        offset = popupPoint - targetPoint;
                        return true;
                    }
                }
            }

            offset = default;
            return false;
        }

        private void UpdateDesiredPopupOffsets()
        {
            var desiredMargin = DesiredMargin;

            if (_parentPopupControl != null)
            {
                switch (_parentPopupControl.Placement)
                {
                    case PlacementMode.Bottom:
                        DesiredPopupHorizontalOffset = -desiredMargin.Left;
                        DesiredPopupVerticalOffset = CalculateVerticalOffset(true);
                        break;
                    case PlacementMode.Center:
                        DesiredPopupHorizontalOffset = 0;
                        DesiredPopupVerticalOffset = 0;
                        break;
                    case PlacementMode.Right:
                        DesiredPopupHorizontalOffset = CalculateHorizontalOffset(false);
                        DesiredPopupVerticalOffset = -desiredMargin.Top;
                        break;
                    case PlacementMode.Left:
                        DesiredPopupHorizontalOffset = CalculateHorizontalOffset(false);
                        DesiredPopupVerticalOffset = -desiredMargin.Top;
                        break;
                    case PlacementMode.Top:
                        DesiredPopupHorizontalOffset = -desiredMargin.Left;
                        DesiredPopupVerticalOffset = CalculateVerticalOffset(false);
                        break;
                    case PlacementMode.Relative:
                    case PlacementMode.RelativePoint:
                        DesiredPopupHorizontalOffset = 0;
                        DesiredPopupVerticalOffset = 0;
                        break;
                    case PlacementMode.Custom:
                        UpdateDesiredPopupOffsetsForCustomPlacement(desiredMargin);
                        break;
                    default:
                        DesiredPopupHorizontalOffset = -desiredMargin.Left;
                        DesiredPopupVerticalOffset = -desiredMargin.Top;
                        break;
                }
            }
            else
            {
                ClearValue(DesiredPopupHorizontalOffsetProperty);
                ClearValue(DesiredPopupVerticalOffsetProperty);
            }
        }

        private void UpdateDesiredPopupOffsetsForCustomPlacement(Thickness desiredMargin)
        {
            if (TryGetCustomPlacementMode(out var placement))
            {
                switch (placement)
                {
                    case CustomPopupPlacementMode.Top:
                        DesiredPopupHorizontalOffset = 0;
                        DesiredPopupVerticalOffset = CalculateVerticalOffset(false);
                        break;
                    case CustomPopupPlacementMode.Bottom:
                        DesiredPopupHorizontalOffset = 0;
                        DesiredPopupVerticalOffset = CalculateVerticalOffset(true);
                        break;
                    case CustomPopupPlacementMode.Left:
                        DesiredPopupHorizontalOffset = CalculateHorizontalOffset(false);
                        DesiredPopupVerticalOffset = 0;
                        break;
                    case CustomPopupPlacementMode.Right:
                        DesiredPopupHorizontalOffset = CalculateHorizontalOffset(true);
                        DesiredPopupVerticalOffset = 0;
                        break;
                    case CustomPopupPlacementMode.TopEdgeAlignedLeft:
                        DesiredPopupHorizontalOffset = -desiredMargin.Left;
                        DesiredPopupVerticalOffset = CalculateVerticalOffset(false);
                        break;
                    case CustomPopupPlacementMode.TopEdgeAlignedRight:
                        DesiredPopupHorizontalOffset = desiredMargin.Right;
                        DesiredPopupVerticalOffset = CalculateVerticalOffset(false);
                        break;
                    case CustomPopupPlacementMode.BottomEdgeAlignedLeft:
                        DesiredPopupHorizontalOffset = -desiredMargin.Left;
                        DesiredPopupVerticalOffset = CalculateVerticalOffset(true);
                        break;
                    case CustomPopupPlacementMode.BottomEdgeAlignedRight:
                        DesiredPopupHorizontalOffset = desiredMargin.Right;
                        DesiredPopupVerticalOffset = CalculateVerticalOffset(true);
                        break;
                    case CustomPopupPlacementMode.LeftEdgeAlignedTop:
                        DesiredPopupHorizontalOffset = CalculateHorizontalOffset(false);
                        DesiredPopupVerticalOffset = -desiredMargin.Top;
                        break;
                    case CustomPopupPlacementMode.LeftEdgeAlignedBottom:
                        DesiredPopupHorizontalOffset = CalculateHorizontalOffset(false);
                        DesiredPopupVerticalOffset = desiredMargin.Bottom;
                        break;
                    case CustomPopupPlacementMode.RightEdgeAlignedTop:
                        DesiredPopupHorizontalOffset = CalculateHorizontalOffset(false);
                        DesiredPopupVerticalOffset = -desiredMargin.Top;
                        break;
                    case CustomPopupPlacementMode.RightEdgeAlignedBottom:
                        DesiredPopupHorizontalOffset = CalculateHorizontalOffset(false);
                        DesiredPopupVerticalOffset = desiredMargin.Bottom;
                        break;
                    default:
                        DesiredPopupHorizontalOffset = 0;
                        DesiredPopupVerticalOffset = 0;
                        break;
                }
            }
            else
            {
                DesiredPopupHorizontalOffset = 0;
                DesiredPopupVerticalOffset = 0;
            }
        }

        private double CalculateHorizontalOffset(bool isOpenRightDesired)
        {
            bool isOpenRight = IsPopupOpenRight() ?? isOpenRightDesired;
            if (isOpenRight)
            {
                return -DesiredMargin.Left;
            }
            else
            {
                return DesiredMargin.Right;
            }
        }

        private double CalculateVerticalOffset(bool isOpenDownDesired)
        {
            bool isOpenDown = IsPopupOpenDown() ?? isOpenDownDesired;
            if (isOpenDown)
            {
                return -DesiredMargin.Top;
            }
            else
            {
                return DesiredMargin.Bottom;
            }
        }

        private bool? IsPopupOpenRight()
        {
            if (TryGetOffsetToTarget(out Vector offset, true))
            {
                return offset.X > 0;
            }
            return null;
        }

        private bool? IsPopupOpenDown()
        {
            if (TryGetOffsetToTarget(out Vector offset, true))
            {
                return offset.Y > 0;
            }
            return null;
        }

        private void EnsureEdgeAligned(bool left)
        {
            if (TryGetOffsetToTarget(out var offset, left))
            {
                bool shouldTransform = left ? offset.X > 0 : offset.X < 0;
                if (shouldTransform)
                {
                    SetupTransform(-offset.X);
                }
                else
                {
                    ResetTransform();
                }
            }
        }

        private void SetupTransform(double offsetX)
        {
            if (_transform == null)
            {
                _transform = new TranslateTransform();
                RenderTransform = _transform;
            }
            _transform.X = offsetX;
        }

        private void ResetTransform()
        {
            if (_transform != null)
            {
                _transform.ClearValue(TranslateTransform.XProperty);
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

        private static bool HasDefaultValue(DependencyObject d, DependencyProperty dp)
        {
            return DependencyPropertyHelper.GetValueSource(d, dp).BaseValueSource == BaseValueSource.Default;
        }

        private class PopupControl : IDisposable
        {
            private ContextMenu _contextMenu;
            private ToolTip _toolTip;
            private Popup _popup;

            public PopupControl(ContextMenu contextMenu)
            {
                _contextMenu = contextMenu;

                ShouldSetOffsets =
                    HasDefaultValue(contextMenu, ContextMenu.HorizontalOffsetProperty) &&
                    HasDefaultValue(contextMenu, ContextMenu.VerticalOffsetProperty);

                _contextMenu.Opened += OnOpened;
                _contextMenu.Closed += OnClosed;
            }

            public PopupControl(ToolTip toolTip)
            {
                _toolTip = toolTip;

                ShouldSetOffsets =
                    HasDefaultValue(toolTip, System.Windows.Controls.ToolTip.HorizontalOffsetProperty) &&
                    HasDefaultValue(toolTip, System.Windows.Controls.ToolTip.VerticalOffsetProperty);

                _toolTip.Opened += OnOpened;
                _toolTip.Closed += OnClosed;
            }

            public PopupControl(Popup popup)
            {
                _popup = popup;

                ShouldSetOffsets =
                    HasDefaultValue(popup, Popup.HorizontalOffsetProperty) &&
                    HasDefaultValue(popup, Popup.VerticalOffsetProperty);

                _popup.Opened += OnOpened;
                _popup.Closed += OnClosed;
            }

            public FrameworkElement Control
            {
                get
                {
                    return
                        _contextMenu as FrameworkElement ??
                        _toolTip as FrameworkElement ??
                        _popup as FrameworkElement;
                }
            }

            public bool ShouldSetOffsets { get; set; }

            public bool EdgeAlignedLeft { get; set; }

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
                        return _popup.PlacementTarget;
                    }
                    return null;
                }
            }

            public event EventHandler Opened;

            public event EventHandler Closed;

            public void SetOffsets(double horizontalOffset, double verticalOffset)
            {
                if (ShouldSetOffsets)
                {
                    if (_contextMenu != null)
                    {
                        _contextMenu.HorizontalOffset = horizontalOffset;
                        _contextMenu.VerticalOffset = verticalOffset;
                    }
                    else if (_toolTip != null)
                    {
                        _toolTip.HorizontalOffset = horizontalOffset;
                        _toolTip.VerticalOffset = verticalOffset;
                    }
                    else if (_popup != null)
                    {
                        _popup.HorizontalOffset = horizontalOffset;
                        _popup.VerticalOffset = verticalOffset;
                    }
                }
            }

            public void ClearOffsets()
            {
                SetOffsets(0, 0);
            }

            public void Dispose()
            {
                if (_contextMenu != null)
                {
                    _contextMenu.Opened -= OnOpened;
                    _contextMenu.Closed -= OnClosed;

                    if (ShouldSetOffsets)
                    {
                        _contextMenu.ClearValue(ContextMenu.HorizontalOffsetProperty);
                        _contextMenu.ClearValue(ContextMenu.VerticalOffsetProperty);
                    }
                }
                else if (_toolTip != null)
                {
                    _toolTip.Opened -= OnOpened;
                    _toolTip.Closed -= OnClosed;

                    if (ShouldSetOffsets)
                    {
                        _toolTip.ClearValue(System.Windows.Controls.ToolTip.HorizontalOffsetProperty);
                        _toolTip.ClearValue(System.Windows.Controls.ToolTip.VerticalOffsetProperty);
                    }
                }
                else if (_popup != null)
                {
                    _popup.Opened -= OnOpened;
                    _popup.Closed -= OnClosed;

                    if (ShouldSetOffsets)
                    {
                        _popup.ClearValue(Popup.HorizontalOffsetProperty);
                        _popup.ClearValue(Popup.VerticalOffsetProperty);
                    }
                }

                _contextMenu = null;
                _toolTip = null;
                _popup = null;
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
        private Border _shadow1;
        private Border _shadow2;
        private PopupControl _parentPopupControl;
        private TranslateTransform _transform;
        private bool _shouldUpdateMargin;

        private static readonly Brush s_bg1, s_bg2, s_bg3, s_bg4;
        private static readonly BitmapCache s_bitmapCache;
        private const double c_shadowMargin = 1;
    }
}
