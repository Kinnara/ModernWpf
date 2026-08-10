using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ModernWpf.Automation.Peers;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = CloseButtonName, Type = typeof(ButtonBase))]
    [TemplatePart(Name = DragVisualName, Type = typeof(Border))]
    [TemplatePart(Name = SeparatorName, Type = typeof(FrameworkElement))]
    public partial class TabViewItem : ListBoxItem
    {
        private const string CloseButtonName = "PART_CloseButton";
        private const string DragVisualName = "PART_DragVisual";
        private const string GeometryCornerRadiusProbeName = "PART_GeometryCornerRadiusProbe";
        private const string SeparatorName = "TabSeparator";
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(TabView));
        private static readonly DependencyPropertyDescriptor CornerRadiusDescriptor =
            DependencyPropertyDescriptor.FromProperty(Border.CornerRadiusProperty, typeof(Border));

        private ButtonBase _closeButton;
        private Border _dragVisual;
        private Border _geometryCornerRadiusProbe;
        private FrameworkElement _separator;
        private Point _dragStartPoint;
        private bool _hasDragStartPoint;
        private bool _isCornerRadiusListenerAttached;
        private Size _lastGeometrySize;
        private CornerRadius _lastGeometryCornerRadius;

        static TabViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabViewItem), new FrameworkPropertyMetadata(typeof(TabViewItem)));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
                typeof(TabViewItem),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
        }

        public TabViewItem()
        {
            SetValue(TabViewTemplateSettingsPropertyKey, new TabViewItemTemplateSettings());
            UpdateIcon();
            LayoutUpdated += OnLayoutUpdated;
            Loaded += OnItemLoaded;
            Unloaded += OnItemUnloaded;
            LostMouseCapture += OnItemLostMouseCapture;
        }

        public event TypedEventHandler<TabViewItem, TabViewTabCloseRequestedEventArgs> CloseRequested;

        public override void OnApplyTemplate()
        {
            if (_closeButton != null)
            {
                _closeButton.Click -= OnCloseButtonClick;
            }
            DetachCornerRadiusListener();

            base.OnApplyTemplate();

            _closeButton = GetTemplateChild(CloseButtonName) as ButtonBase;
            _dragVisual = GetTemplateChild(DragVisualName) as Border;
            _geometryCornerRadiusProbe = GetTemplateChild(GeometryCornerRadiusProbeName) as Border;
            _separator = GetTemplateChild(SeparatorName) as FrameworkElement;
            if (_closeButton != null)
            {
                _closeButton.Click += OnCloseButtonClick;
                AutomationProperties.SetName(
                    _closeButton,
                    ResourceAccessor.GetLocalizedStringResource(SR_TabViewCloseButtonName));
                ToolTipService.SetToolTip(
                    _closeButton,
                    ResourceAccessor.GetLocalizedStringResource(SR_TabViewCloseButtonTooltipWithKA));
            }
            AttachCornerRadiusListener();

            UpdateTabGeometry();
            UpdateVisualState(false);
            Owner?.UpdateTabSeparators();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TabViewItemAutomationPeer(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled && IsEnabled)
            {
                _dragStartPoint = e.GetPosition(this);
                _hasDragStartPoint = true;
                Owner?.SelectTab(this);
                if (!IsMouseCaptured)
                {
                    CaptureMouse();
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _hasDragStartPoint = false;
            base.OnMouseLeftButtonUp(e);
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_hasDragStartPoint ||
                e.LeftButton != MouseButtonState.Pressed ||
                Owner?.CanDragTabs != true)
            {
                return;
            }

            var point = e.GetPosition(this);
            if (Math.Abs(point.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(point.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            _hasDragStartPoint = false;
            Owner.StartDrag(this);
            e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (!e.Handled && e.ChangedButton == MouseButton.Middle && IsEnabled && IsClosable)
            {
                Owner?.RequestClose(this);
                e.Handled = true;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Handled)
            {
                return;
            }

            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                Owner?.SelectTab(this);
                e.Handled = true;
            }
            else if ((e.Key == Key.Left || e.Key == Key.Right) &&
                (Keyboard.Modifiers & (ModifierKeys.Alt | ModifierKeys.Shift)) != (ModifierKeys.Alt | ModifierKeys.Shift) &&
                Owner?.MoveFocus(
                    (FlowDirection == FlowDirection.LeftToRight && e.Key == Key.Right) ||
                    (FlowDirection == FlowDirection.RightToLeft && e.Key == Key.Left)) == true)
            {
                e.Handled = true;
            }
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            Owner?.OnContainerSelectionChanged(this, true);
            RaiseSelectionAutomationEvents(false, true);
            UpdateVisualState();
        }

        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            Owner?.OnContainerSelectionChanged(this, false);
            RaiseSelectionAutomationEvents(true, false);
            UpdateVisualState();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateVisualState();
            Owner?.UpdateTabSeparators();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateVisualState();
            Owner?.UpdateTabSeparators();
        }

        internal TabView Owner { get; set; }

        internal object Item { get; set; }

        internal ButtonBase CloseButton => _closeButton;

        internal FrameworkElement DragVisual => _dragVisual;

        internal FrameworkElement Separator => _separator;

        internal void SetDragging(bool isDragging)
        {
            if (_dragVisual != null)
            {
                _dragVisual.Visibility = isDragging ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        internal void SetSeparatorOpacity(double opacity)
        {
            if (_separator != null)
            {
                _separator.Opacity = opacity;
            }
        }

        internal bool IsCloseButtonFocusable =>
            _closeButton != null &&
            _closeButton.Visibility == Visibility.Visible &&
            _closeButton.IsEnabled &&
            _closeButton.Focusable;

        internal bool FocusCloseButton()
        {
            if (!IsCloseButtonFocusable)
            {
                return false;
            }

            var isTabStop = _closeButton.IsTabStop;
            try
            {
                _closeButton.IsTabStop = true;
                return _closeButton.Focus();
            }
            finally
            {
                _closeButton.IsTabStop = isTabStop;
            }
        }

        internal bool IsCloseButtonOrDescendant(DependencyObject element)
        {
            for (var current = element; current != null; current = VisualTreeHelper.GetParent(current))
            {
                if (ReferenceEquals(current, _closeButton))
                {
                    return true;
                }

                if (ReferenceEquals(current, this))
                {
                    break;
                }
            }

            return false;
        }

        internal void RaiseCloseRequested(TabViewTabCloseRequestedEventArgs args)
        {
            CloseRequested?.Invoke(this, args);
        }

        internal void UpdateVisualState(bool useTransitions = true)
        {
            var commonState = !IsEnabled
                ? "Disabled"
                : IsMouseCaptured && Mouse.LeftButton == MouseButtonState.Pressed
                    ? "Pressed"
                    : IsMouseOver ? "PointerOver" : "Normal";
            VisualStateManager.GoToState(this, commonState, useTransitions);
            VisualStateManager.GoToState(this, IsSelected ? "Selected" : "Unselected", useTransitions);
            VisualStateManager.GoToState(this, IsClosable ? "Closable" : "NotClosable", useTransitions);

            var overlayMode = Owner?.CloseButtonOverlayMode ?? TabViewCloseButtonOverlayMode.Auto;
            var closeState = overlayMode != TabViewCloseButtonOverlayMode.OnPointerOver ||
                IsSelected ||
                IsMouseOver ||
                IsKeyboardFocusWithin
                ? "CloseButtonVisible"
                : "CloseButtonCollapsed";
            VisualStateManager.GoToState(this, closeState, useTransitions);
            if (_closeButton != null)
            {
                _closeButton.Visibility = IsClosable && closeState == "CloseButtonVisible"
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                _closeButton.IsTabStop = false;
            }
        }

        private static void OnHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TabViewItem item)
            {
                item.Owner?.OnTabHeaderChanged(item);
            }
        }

        private static void OnIconSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TabViewItem)d).UpdateIcon();
        }

        private static void OnIsClosablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TabViewItem)d).UpdateVisualState();
        }

        private void UpdateIcon()
        {
            TabViewTemplateSettings.IconElement = IconSource == null
                ? null
                : SharedHelpers.MakeIconElementFrom(IconSource);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            var cornerRadius = GetGeometryCornerRadius();
            var size = new Size(ActualWidth, ActualHeight);
            if (size != _lastGeometrySize || cornerRadius != _lastGeometryCornerRadius)
            {
                UpdateTabGeometry(size, cornerRadius);
            }
        }

        private void UpdateTabGeometry()
        {
            UpdateTabGeometry(new Size(ActualWidth, ActualHeight), GetGeometryCornerRadius());
        }

        private CornerRadius GetGeometryCornerRadius()
        {
            if (_geometryCornerRadiusProbe != null)
            {
                return _geometryCornerRadiusProbe.CornerRadius;
            }

            return TryFindResource("OverlayCornerRadius") is CornerRadius radius
                ? radius
                : new CornerRadius(8.0);
        }

        private void OnGeometryCornerRadiusChanged(object sender, EventArgs e)
        {
            UpdateTabGeometry();
        }

        private void OnItemLoaded(object sender, RoutedEventArgs e)
        {
            AttachCornerRadiusListener();
        }

        private void OnItemUnloaded(object sender, RoutedEventArgs e)
        {
            DetachCornerRadiusListener();
        }

        private void AttachCornerRadiusListener()
        {
            if (!_isCornerRadiusListenerAttached && IsLoaded && _geometryCornerRadiusProbe != null)
            {
                CornerRadiusDescriptor?.AddValueChanged(
                    _geometryCornerRadiusProbe,
                    OnGeometryCornerRadiusChanged);
                _isCornerRadiusListenerAttached = CornerRadiusDescriptor != null;
            }
        }

        private void DetachCornerRadiusListener()
        {
            if (_isCornerRadiusListenerAttached && _geometryCornerRadiusProbe != null)
            {
                CornerRadiusDescriptor?.RemoveValueChanged(
                    _geometryCornerRadiusProbe,
                    OnGeometryCornerRadiusChanged);
            }

            _isCornerRadiusListenerAttached = false;
        }

        private void UpdateTabGeometry(Size size, CornerRadius cornerRadius)
        {
            _lastGeometrySize = size;
            _lastGeometryCornerRadius = cornerRadius;

            if (size.Width <= 0.0 || size.Height <= 0.0 ||
                double.IsNaN(size.Width) || double.IsNaN(size.Height) ||
                double.IsInfinity(size.Width) || double.IsInfinity(size.Height))
            {
                TabViewTemplateSettings.TabGeometry = null;
                return;
            }

            const double outwardRadius = 4.0;
            var leftCorner = Math.Max(0.0, Math.Min(cornerRadius.TopLeft, size.Width / 2.0));
            var rightCorner = Math.Max(0.0, Math.Min(cornerRadius.TopRight, size.Width - leftCorner));
            var geometry = new StreamGeometry { FillRule = FillRule.Nonzero };
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(0.0, size.Height), true, true);
                context.ArcTo(
                    new Point(outwardRadius, Math.Max(0.0, size.Height - outwardRadius)),
                    new Size(outwardRadius, outwardRadius),
                    0.0,
                    false,
                    SweepDirection.Counterclockwise,
                    true,
                    false);
                context.LineTo(new Point(outwardRadius, leftCorner), true, false);
                if (leftCorner > 0.0)
                {
                    context.ArcTo(
                        new Point(outwardRadius + leftCorner, 0.0),
                        new Size(leftCorner, leftCorner),
                        0.0,
                        false,
                        SweepDirection.Clockwise,
                        true,
                        false);
                }

                context.LineTo(new Point(size.Width - rightCorner, 0.0), true, false);
                if (rightCorner > 0.0)
                {
                    context.ArcTo(
                        new Point(size.Width, rightCorner),
                        new Size(rightCorner, rightCorner),
                        0.0,
                        false,
                        SweepDirection.Clockwise,
                        true,
                        false);
                }

                context.LineTo(new Point(size.Width, Math.Max(rightCorner, size.Height - outwardRadius)), true, false);
                context.ArcTo(
                    new Point(size.Width + outwardRadius, size.Height),
                    new Size(outwardRadius, outwardRadius),
                    0.0,
                    false,
                    SweepDirection.Counterclockwise,
                    true,
                    false);
            }

            geometry.Freeze();
            TabViewTemplateSettings.TabGeometry = geometry;
        }

        private void RaiseSelectionAutomationEvents(bool oldValue, bool newValue)
        {
            if (UIElementAutomationPeer.FromElement(this) is TabViewItemAutomationPeer peer)
            {
                peer.RaiseIsSelectedChanged(oldValue, newValue);
            }
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Owner?.RequestClose(this);
            e.Handled = true;
        }

        private void OnItemLostMouseCapture(object sender, MouseEventArgs e)
        {
            _hasDragStartPoint = false;
            UpdateVisualState();
            Owner?.UpdateTabSeparators();
        }
    }
}
