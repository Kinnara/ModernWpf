using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = LeftItemsPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = RightItemsPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = TopItemsPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = BottomItemsPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = ContentTransformName, Type = typeof(TranslateTransform))]
    public class SwipeControl : ContentControl
    {
        private const double DragThreshold = 8;
        private const double OpenThreshold = 32;
        private const double ExecuteThresholdRatio = 0.75;
        private const string LeftItemsPanelName = "PART_LeftItemsPanel";
        private const string RightItemsPanelName = "PART_RightItemsPanel";
        private const string TopItemsPanelName = "PART_TopItemsPanel";
        private const string BottomItemsPanelName = "PART_BottomItemsPanel";
        private const string ContentTransformName = "PART_ContentTransform";

        static SwipeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SwipeControl),
                new FrameworkPropertyMetadata(typeof(SwipeControl)));
        }

        public SwipeControl()
        {
            IsTabStop = false;
            PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            PreviewMouseMove += OnPreviewMouseMove;
            PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            LostMouseCapture += OnLostMouseCapture;
            Unloaded += OnUnloaded;
        }

        public static readonly DependencyProperty LeftItemsProperty =
            DependencyProperty.Register(
                nameof(LeftItems),
                typeof(SwipeItems),
                typeof(SwipeControl),
                new FrameworkPropertyMetadata(null, OnItemsPropertyChanged));

        public SwipeItems LeftItems
        {
            get => (SwipeItems)GetValue(LeftItemsProperty);
            set
            {
                ValidateSwipeItemsCanSet(value, SwipeItemsPlacement.Left);
                SetValue(LeftItemsProperty, value);
            }
        }

        public static readonly DependencyProperty RightItemsProperty =
            DependencyProperty.Register(
                nameof(RightItems),
                typeof(SwipeItems),
                typeof(SwipeControl),
                new FrameworkPropertyMetadata(null, OnItemsPropertyChanged));

        public SwipeItems RightItems
        {
            get => (SwipeItems)GetValue(RightItemsProperty);
            set
            {
                ValidateSwipeItemsCanSet(value, SwipeItemsPlacement.Right);
                SetValue(RightItemsProperty, value);
            }
        }

        public static readonly DependencyProperty TopItemsProperty =
            DependencyProperty.Register(
                nameof(TopItems),
                typeof(SwipeItems),
                typeof(SwipeControl),
                new FrameworkPropertyMetadata(null, OnItemsPropertyChanged));

        public SwipeItems TopItems
        {
            get => (SwipeItems)GetValue(TopItemsProperty);
            set
            {
                ValidateSwipeItemsCanSet(value, SwipeItemsPlacement.Top);
                SetValue(TopItemsProperty, value);
            }
        }

        public static readonly DependencyProperty BottomItemsProperty =
            DependencyProperty.Register(
                nameof(BottomItems),
                typeof(SwipeItems),
                typeof(SwipeControl),
                new FrameworkPropertyMetadata(null, OnItemsPropertyChanged));

        public SwipeItems BottomItems
        {
            get => (SwipeItems)GetValue(BottomItemsProperty);
            set
            {
                ValidateSwipeItemsCanSet(value, SwipeItemsPlacement.Bottom);
                SetValue(BottomItemsProperty, value);
            }
        }

        public void Close()
        {
            CloseSwipe();
        }

        public override void OnApplyTemplate()
        {
            UpdateDismissHook(null);

            base.OnApplyTemplate();

            _leftItemsPanel = GetTemplateChild(LeftItemsPanelName) as Panel;
            _rightItemsPanel = GetTemplateChild(RightItemsPanelName) as Panel;
            _topItemsPanel = GetTemplateChild(TopItemsPanelName) as Panel;
            _bottomItemsPanel = GetTemplateChild(BottomItemsPanelName) as Panel;
            _contentTransform = GetTemplateChild(ContentTransformName) as TranslateTransform;

            RebuildSwipeItems();
            ApplySwipeOffset(_openPlacement, _currentOffset);
            UpdateDismissHook(IsOpen ? Window.GetWindow(this) : null);
        }

        internal void ValidateSwipeItemsCanAdd(SwipeItemsPlacement placement)
        {
            if (IsHorizontalPlacement(placement) && HasVerticalItems())
            {
                throw new ArgumentException("SwipeControl can only have horizontal or vertical items.");
            }

            if (IsVerticalPlacement(placement) && HasHorizontalItems())
            {
                throw new ArgumentException("SwipeControl can only have horizontal or vertical items.");
            }
        }

        internal void OnSwipeItemsChanged()
        {
            RebuildSwipeItems();
            if (_openPlacement != SwipeItemsPlacement.None && !HasItems(GetItemsForPlacement(_openPlacement)))
            {
                CloseSwipe();
            }
        }

        internal bool IsOpenForTesting => IsOpen;

        internal SwipeItemsPlacement OpenedItemsPlacementForTesting => _openPlacement;

        internal double HorizontalOffsetForTesting => _contentTransform?.X ?? _currentOffset;

        internal double VerticalOffsetForTesting => _contentTransform?.Y ?? _currentOffset;

        internal void DragForTesting(double horizontalDelta, double verticalDelta, bool complete)
        {
            StartDrag(new Point());
            UpdateDrag(new Point(horizontalDelta, verticalDelta));

            if (complete)
            {
                CompleteDrag();
            }
        }

        private static void OnItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var swipeControl = (SwipeControl)d;
            var placement = GetPlacement(e.Property);

            if (e.OldValue is SwipeItems oldItems)
            {
                oldItems.DetachOwner(swipeControl);
            }

            if (e.NewValue is SwipeItems newItems)
            {
                swipeControl.ValidateSwipeItemsCanSet(newItems, placement);
                newItems.AttachOwner(swipeControl, placement);
            }

            swipeControl.RebuildSwipeItems();
            if (swipeControl._openPlacement != SwipeItemsPlacement.None &&
                !HasItems(swipeControl.GetItemsForPlacement(swipeControl._openPlacement)))
            {
                swipeControl.CloseSwipe();
            }
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsWithinSwipeItems(e.OriginalSource as DependencyObject))
            {
                return;
            }

            if (s_openSwipeControl != null && !ReferenceEquals(s_openSwipeControl, this))
            {
                s_openSwipeControl.Close();
            }

            StartDrag(e.GetPosition(this));
            CaptureMouse();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPointerDown)
            {
                return;
            }

            UpdateDrag(e.GetPosition(this));
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isPointerDown)
            {
                return;
            }

            ReleaseMouseCapture();
            CompleteDrag();
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            if (_isPointerDown)
            {
                CompleteDrag();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UpdateDismissHook(null);
            if (ReferenceEquals(s_openSwipeControl, this))
            {
                s_openSwipeControl = null;
            }
        }

        private void StartDrag(Point position)
        {
            _isPointerDown = true;
            _isDragging = false;
            _dragStartPoint = position;
            _dragStartOffset = _currentOffset;
            _dragPlacement = _openPlacement;
        }

        private void UpdateDrag(Point position)
        {
            var horizontalDelta = position.X - _dragStartPoint.X;
            var verticalDelta = position.Y - _dragStartPoint.Y;
            var placement = _dragPlacement != SwipeItemsPlacement.None
                ? _dragPlacement
                : GetPlacementFromDelta(horizontalDelta, verticalDelta);

            if (placement == SwipeItemsPlacement.None)
            {
                return;
            }

            var delta = IsHorizontalPlacement(placement) ? horizontalDelta : verticalDelta;
            if (!_isDragging && Math.Abs(delta) < DragThreshold)
            {
                return;
            }

            var directionalDelta = GetDirectionalDelta(placement, delta);
            if (directionalDelta <= 0 && _openPlacement == SwipeItemsPlacement.None)
            {
                return;
            }

            _isDragging = true;
            _dragPlacement = placement;

            var revealSize = GetRevealSize(placement);
            var unsignedOffset = Math.Max(0, Math.Min(revealSize, GetDirectionalOffset(placement, _dragStartOffset) + directionalDelta));
            OpenSwipe(placement, GetSignedOffset(placement, unsignedOffset), hookDismiss: false);
        }

        private void CompleteDrag()
        {
            var wasDragging = _isDragging;
            var placement = _dragPlacement;

            _isPointerDown = false;
            _isDragging = false;
            _dragPlacement = SwipeItemsPlacement.None;

            if (!wasDragging || placement == SwipeItemsPlacement.None)
            {
                return;
            }

            var items = GetItemsForPlacement(placement);
            var revealSize = GetRevealSize(placement);
            var directionalOffset = GetDirectionalOffset(placement, _currentOffset);

            if (items?.Mode == SwipeMode.Execute &&
                items.Count > 0 &&
                revealSize > 0 &&
                directionalOffset >= revealSize * ExecuteThresholdRatio)
            {
                items[0].Invoke(this);
                if (items[0].BehaviorOnInvoked == SwipeBehaviorOnInvoked.RemainOpen)
                {
                    OpenSwipe(placement, GetSignedOffset(placement, revealSize), hookDismiss: true);
                }

                return;
            }

            if (directionalOffset >= Math.Min(OpenThreshold, revealSize / 2))
            {
                OpenSwipe(placement, GetSignedOffset(placement, revealSize), hookDismiss: true);
            }
            else
            {
                CloseSwipe();
            }
        }

        private SwipeItemsPlacement GetPlacementFromDelta(double horizontalDelta, double verticalDelta)
        {
            if (Math.Abs(horizontalDelta) >= Math.Abs(verticalDelta) && HasHorizontalItems())
            {
                return horizontalDelta > 0 && HasItems(LeftItems)
                    ? SwipeItemsPlacement.Left
                    : horizontalDelta < 0 && HasItems(RightItems)
                        ? SwipeItemsPlacement.Right
                        : SwipeItemsPlacement.None;
            }

            if (HasVerticalItems())
            {
                return verticalDelta > 0 && HasItems(TopItems)
                    ? SwipeItemsPlacement.Top
                    : verticalDelta < 0 && HasItems(BottomItems)
                        ? SwipeItemsPlacement.Bottom
                        : SwipeItemsPlacement.None;
            }

            return SwipeItemsPlacement.None;
        }

        private static double GetDirectionalDelta(SwipeItemsPlacement placement, double delta)
        {
            return placement == SwipeItemsPlacement.Left || placement == SwipeItemsPlacement.Top ? delta : -delta;
        }

        private static double GetSignedOffset(SwipeItemsPlacement placement, double offset)
        {
            return placement == SwipeItemsPlacement.Left || placement == SwipeItemsPlacement.Top ? offset : -offset;
        }

        private static double GetDirectionalOffset(SwipeItemsPlacement placement, double offset)
        {
            return placement == SwipeItemsPlacement.Left || placement == SwipeItemsPlacement.Top ? offset : -offset;
        }

        private void OpenSwipe(SwipeItemsPlacement placement, double offset, bool hookDismiss)
        {
            _openPlacement = placement;
            _currentOffset = offset;
            s_openSwipeControl = this;
            ApplySwipeOffset(placement, offset);

            if (hookDismiss)
            {
                UpdateDismissHook(Window.GetWindow(this));
            }
        }

        private void CloseSwipe()
        {
            _openPlacement = SwipeItemsPlacement.None;
            _dragPlacement = SwipeItemsPlacement.None;
            _currentOffset = 0;
            ApplySwipeOffset(SwipeItemsPlacement.None, 0);
            UpdateDismissHook(null);

            if (ReferenceEquals(s_openSwipeControl, this))
            {
                s_openSwipeControl = null;
            }
        }

        private void ApplySwipeOffset(SwipeItemsPlacement placement, double offset)
        {
            if (_contentTransform == null)
            {
                return;
            }

            if (IsHorizontalPlacement(placement))
            {
                _contentTransform.X = offset;
                _contentTransform.Y = 0;
            }
            else if (IsVerticalPlacement(placement))
            {
                _contentTransform.X = 0;
                _contentTransform.Y = offset;
            }
            else
            {
                _contentTransform.X = 0;
                _contentTransform.Y = 0;
            }
        }

        private double GetRevealSize(SwipeItemsPlacement placement)
        {
            var panel = GetPanelForPlacement(placement);
            if (panel == null)
            {
                return 0;
            }

            panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var size = IsHorizontalPlacement(placement)
                ? Math.Max(panel.ActualWidth, panel.DesiredSize.Width)
                : Math.Max(panel.ActualHeight, panel.DesiredSize.Height);

            return Math.Max(0, size);
        }

        private SwipeItems GetItemsForPlacement(SwipeItemsPlacement placement)
        {
            switch (placement)
            {
                case SwipeItemsPlacement.Left:
                    return LeftItems;
                case SwipeItemsPlacement.Right:
                    return RightItems;
                case SwipeItemsPlacement.Top:
                    return TopItems;
                case SwipeItemsPlacement.Bottom:
                    return BottomItems;
                default:
                    return null;
            }
        }

        private Panel GetPanelForPlacement(SwipeItemsPlacement placement)
        {
            switch (placement)
            {
                case SwipeItemsPlacement.Left:
                    return _leftItemsPanel;
                case SwipeItemsPlacement.Right:
                    return _rightItemsPanel;
                case SwipeItemsPlacement.Top:
                    return _topItemsPanel;
                case SwipeItemsPlacement.Bottom:
                    return _bottomItemsPanel;
                default:
                    return null;
            }
        }

        private bool IsOpen => _openPlacement != SwipeItemsPlacement.None;

        private void UpdateDismissHook(Window window)
        {
            if (ReferenceEquals(_dismissWindow, window))
            {
                return;
            }

            if (_dismissWindow != null)
            {
                _dismissWindow.PreviewMouseDown -= OnWindowPreviewMouseDown;
            }

            _dismissWindow = window;

            if (_dismissWindow != null)
            {
                _dismissWindow.PreviewMouseDown += OnWindowPreviewMouseDown;
            }
        }

        private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsOpen)
            {
                return;
            }

            var position = e.GetPosition(this);
            if (position.X >= 0 &&
                position.Y >= 0 &&
                position.X <= ActualWidth &&
                position.Y <= ActualHeight)
            {
                return;
            }

            CloseSwipe();
        }

        private bool IsWithinSwipeItems(DependencyObject source)
        {
            while (source != null)
            {
                if (ReferenceEquals(source, _leftItemsPanel) ||
                    ReferenceEquals(source, _rightItemsPanel) ||
                    ReferenceEquals(source, _topItemsPanel) ||
                    ReferenceEquals(source, _bottomItemsPanel))
                {
                    return true;
                }

                var parent = VisualTreeHelper.GetParent(source);
                if (parent == null)
                {
                    parent = LogicalTreeHelper.GetParent(source);
                }

                source = parent;
            }

            return false;
        }

        private void ValidateSwipeItemsCanSet(SwipeItems items, SwipeItemsPlacement placement)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            if (items.Mode == SwipeMode.Execute && items.Count > 1)
            {
                throw new ArgumentException("Execute items should only have one item.");
            }

            if (IsHorizontalPlacement(placement) && HasVerticalItems())
            {
                throw new ArgumentException("SwipeControl can only have horizontal or vertical items.");
            }

            if (IsVerticalPlacement(placement) && HasHorizontalItems())
            {
                throw new ArgumentException("SwipeControl can only have horizontal or vertical items.");
            }
        }

        private bool HasHorizontalItems()
        {
            return HasItems(LeftItems) || HasItems(RightItems);
        }

        private bool HasVerticalItems()
        {
            return HasItems(TopItems) || HasItems(BottomItems);
        }

        private static bool HasItems(SwipeItems items)
        {
            return items?.Count > 0;
        }

        private static bool IsHorizontalPlacement(SwipeItemsPlacement placement)
        {
            return placement == SwipeItemsPlacement.Left || placement == SwipeItemsPlacement.Right;
        }

        private static bool IsVerticalPlacement(SwipeItemsPlacement placement)
        {
            return placement == SwipeItemsPlacement.Top || placement == SwipeItemsPlacement.Bottom;
        }

        private static SwipeItemsPlacement GetPlacement(DependencyProperty property)
        {
            if (property == LeftItemsProperty)
            {
                return SwipeItemsPlacement.Left;
            }

            if (property == RightItemsProperty)
            {
                return SwipeItemsPlacement.Right;
            }

            if (property == TopItemsProperty)
            {
                return SwipeItemsPlacement.Top;
            }

            if (property == BottomItemsProperty)
            {
                return SwipeItemsPlacement.Bottom;
            }

            return SwipeItemsPlacement.None;
        }

        private void RebuildSwipeItems()
        {
            RebuildPanel(_leftItemsPanel, LeftItems);
            RebuildPanel(_rightItemsPanel, RightItems);
            RebuildPanel(_topItemsPanel, TopItems);
            RebuildPanel(_bottomItemsPanel, BottomItems);
        }

        private void RebuildPanel(Panel panel, SwipeItems items)
        {
            if (panel == null)
            {
                return;
            }

            foreach (var button in panel.Children.OfType<Button>().ToList())
            {
                button.Click -= OnSwipeItemButtonClick;
            }

            panel.Children.Clear();
            panel.Visibility = HasItems(items) ? Visibility.Visible : Visibility.Collapsed;

            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                var button = CreateButtonForItem(item);
                button.Click += OnSwipeItemButtonClick;
                panel.Children.Add(button);
            }
        }

        private Button CreateButtonForItem(SwipeItem item)
        {
            var button = new Button
            {
                Tag = item,
                Content = CreateButtonContent(item),
                MinWidth = 68,
                MinHeight = 44,
                Padding = new Thickness(8, 4, 8, 4),
                Background = item.Background,
                Foreground = item.Foreground
            };

            AutomationProperties.SetName(button, item.Text ?? string.Empty);

            var command = item.Command;
            if (command != null)
            {
                button.IsEnabled = command.CanExecute(item.CommandParameter);
            }

            return button;
        }

        private static object CreateButtonContent(SwipeItem item)
        {
            if (item.IconSource == null)
            {
                return item.Text;
            }

            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var icon = item.IconSource.CreateIconElement();
            icon.Margin = new Thickness(0, 0, 4, 0);
            panel.Children.Add(icon);
            panel.Children.Add(new TextBlock
            {
                Text = item.Text,
                VerticalAlignment = VerticalAlignment.Center
            });
            return panel;
        }

        private void OnSwipeItemButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: SwipeItem item })
            {
                item.Invoke(this);
            }
        }

        private Panel _leftItemsPanel;
        private Panel _rightItemsPanel;
        private Panel _topItemsPanel;
        private Panel _bottomItemsPanel;
        private TranslateTransform _contentTransform;
        private Window _dismissWindow;
        private Point _dragStartPoint;
        private double _dragStartOffset;
        private double _currentOffset;
        private bool _isPointerDown;
        private bool _isDragging;
        private SwipeItemsPlacement _openPlacement = SwipeItemsPlacement.None;
        private SwipeItemsPlacement _dragPlacement = SwipeItemsPlacement.None;
        private static SwipeControl s_openSwipeControl;
    }
}
