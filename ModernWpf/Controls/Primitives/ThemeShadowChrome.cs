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
        public ThemeShadowChrome()
        {
            _background = new Grid();
            AddVisualChild(_background);
        }

        private void EnsureShadows()
        {
            _shadow1 = new Border
            {
                CornerRadius = CornerRadius,
                Background = Brushes.Black
            };
            UpdateShadow1();

            _shadow2 = new Border
            {
                CornerRadius = CornerRadius,
                Background = Brushes.Black
            };
            UpdateShadow2();
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
                    _background.Children.Remove(_shadow2);
                    _background.Children.Remove(_shadow1);
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

        internal Thickness DesiredMargin => _desiredMargin;

        internal double DesiredPopupHorizontalOffset => -_desiredMargin.Left;

        internal double DesiredPopupVerticalOffset => -_desiredMargin.Top;

        protected override int VisualChildrenCount =>
            IsShadowEnabled ? Child == null ? 1 : 2 : base.VisualChildrenCount;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _autoMargin = HasDefaultValue(this, MarginProperty);

            OnIsShadowEnabledChanged();
        }

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
            if (_popup != null)
            {
                _popup.Opened -= OnPopupOpened;
                _popup.Closed -= OnPopupClosed;
                _popup = null;

                if (_transform != null)
                {
                    _transform.ClearValue(TranslateTransform.XProperty);
                    _transform = null;
                    ClearValue(RenderTransformProperty);
                }
            }

            if (IsShadowEnabled)
            {
                PopupControl parentPopupControl = null;
                Popup popup = null;
                bool shouldForceLeftAlignWithTarget = false;

                var newParent = VisualParent;
                if (newParent is ContextMenu contextMenu)
                {
                    parentPopupControl = new PopupControl(contextMenu);

                    popup = FindParentPopup(this);
                    if (popup != null && popup.PlacementTarget != null &&
                        popup.PlacementTarget.GetType().Name.Contains("DropDownButton"))
                    {
                        shouldForceLeftAlignWithTarget = true;
                    }
                }
                else if (newParent is ToolTip toolTip)
                {
                    parentPopupControl = new PopupControl(toolTip);
                }
                else
                {
                    popup = FindParentPopup(this);
                    if (popup != null)
                    {
                        bool shouldManagePosition = true;

                        if (TemplatedParent is MenuItem menuItem)
                        {
                            shouldManagePosition = false;

                            if (menuItem.Role == MenuItemRole.TopLevelHeader)
                            {
                                shouldForceLeftAlignWithTarget = true;
                            }
                        }
                        else if (newParent is Calendar)
                        {
                            shouldForceLeftAlignWithTarget = true;
                        }
                        else
                        {
                            var target = popup.PlacementTarget;
                            if (target != null)
                            {
                                if (target is Button ||
                                    target.GetType().Name.Contains("SplitButton"))
                                {
                                    shouldForceLeftAlignWithTarget = true;
                                }
                            }
                        }

                        if (shouldManagePosition)
                        {
                            parentPopupControl = new PopupControl(popup);
                        }
                    }
                }

                SetParentPopupControl(parentPopupControl);

                if (shouldForceLeftAlignWithTarget)
                {
                    _popup = popup;
                    _popup.Opened += OnPopupOpened;
                    _popup.Closed += OnPopupClosed;
                }
            }
            else
            {
                SetParentPopupControl(null);
            }
        }

        private void UpdateShadow1()
        {
            if (_shadow1 != null)
            {
                double depth = Depth;
                _shadow1.Opacity = depth >= 32 ? 0.22 : 0.13;
                _shadow1.Effect = new DropShadowEffect
                {
                    Direction = 270,
                    ShadowDepth = 0.4 * depth,
                    BlurRadius = 0.9 * depth,
                    Color = Colors.Black
                };
            }
        }

        private void UpdateShadow2()
        {
            if (_shadow2 != null)
            {
                double depth = Depth;
                _shadow2.Opacity = depth >= 32 ? 0.13 : 0.11;
                _shadow2.Effect = new DropShadowEffect
                {
                    Direction = 270,
                    ShadowDepth = 0.08 * depth,
                    BlurRadius = 0.22 * depth,
                    Color = Colors.Black
                };
            }
        }

        private void AdjustLayout()
        {
            if (IsShadowEnabled)
            {
                double depth = Depth;
                double radius = 0.9 * depth;
                double offset = 0.4 * depth;

                _desiredMargin = new Thickness(
                    radius,
                    radius,
                    radius,
                    radius + offset);
            }
            else
            {
                _desiredMargin = new Thickness();
            }

            UpdateMargin();
            PositionParentPopupControl();
        }

        private void UpdateMargin()
        {
            if (_autoMargin)
            {
                if (IsShadowEnabled)
                {
                    Margin = _desiredMargin;
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
                _parentPopupControl.Dispose();
            }

            _parentPopupControl = value as PopupControl;

            PositionParentPopupControl();
        }

        private void PositionParentPopupControl()
        {
            if (_parentPopupControl != null)
            {
                Debug.Assert(IsShadowEnabled);
                _parentPopupControl.UpdatePosition(DesiredPopupHorizontalOffset, DesiredPopupVerticalOffset);
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            var popup = (Popup)sender;
            ForceLeftAlignWithTarget(popup);
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (_transform != null)
            {
                _transform.ClearValue(TranslateTransform.XProperty);
            }
        }

        private void ForceLeftAlignWithTarget(Popup popup)
        {
            var target = popup.PlacementTarget;
            if (target != null)
            {
                var p1 = target.PointToScreen(new Point());
                var p2 = PointToScreen(new Point());
                var offsetX = p1.X - p2.X;
                if (offsetX >= 0)
                {
                    if (_transform != null)
                    {
                        _transform.ClearValue(TranslateTransform.XProperty);
                    }
                }
                else
                {
                    if (_transform == null)
                    {
                        _transform = new TranslateTransform();
                        RenderTransform = _transform;
                    }
                    _transform.X = p1.X - p2.X;
                }
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
            private bool _shouldUpdatePosition;

            public PopupControl(ContextMenu contextMenu)
            {
                _contextMenu = contextMenu;
                _shouldUpdatePosition =
                    HasDefaultValue(contextMenu, ContextMenu.HorizontalOffsetProperty) &&
                    HasDefaultValue(contextMenu, ContextMenu.VerticalOffsetProperty);
            }

            public PopupControl(ToolTip toolTip)
            {
                _toolTip = toolTip;
                _shouldUpdatePosition =
                    HasDefaultValue(toolTip, System.Windows.Controls.ToolTip.HorizontalOffsetProperty) &&
                    HasDefaultValue(toolTip, System.Windows.Controls.ToolTip.VerticalOffsetProperty);
            }

            public PopupControl(Popup popup)
            {
                _popup = popup;
                _shouldUpdatePosition =
                    HasDefaultValue(popup, Popup.HorizontalOffsetProperty) &&
                    HasDefaultValue(popup, Popup.VerticalOffsetProperty);
            }

            public void UpdatePosition(double horizontalOffset, double verticalOffset)
            {
                if (_shouldUpdatePosition)
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

            public void Dispose()
            {
                if (_shouldUpdatePosition)
                {
                    _shouldUpdatePosition = false;

                    if (_contextMenu != null)
                    {
                        _contextMenu.ClearValue(ContextMenu.HorizontalOffsetProperty);
                        _contextMenu.ClearValue(ContextMenu.VerticalOffsetProperty);
                    }
                    else if (_toolTip != null)
                    {
                        _toolTip.ClearValue(System.Windows.Controls.ToolTip.HorizontalOffsetProperty);
                        _toolTip.ClearValue(System.Windows.Controls.ToolTip.VerticalOffsetProperty);
                    }
                    else if (_popup != null)
                    {
                        _popup.ClearValue(Popup.HorizontalOffsetProperty);
                        _popup.ClearValue(Popup.VerticalOffsetProperty);
                    }
                }

                _contextMenu = null;
                _toolTip = null;
                _popup = null;
            }
        }

        private readonly Grid _background;
        private Border _shadow1;
        private Border _shadow2;
        private PopupControl _parentPopupControl;
        private Popup _popup;
        private TranslateTransform _transform;
        private bool _autoMargin;
        private Thickness _desiredMargin;
    }
}
