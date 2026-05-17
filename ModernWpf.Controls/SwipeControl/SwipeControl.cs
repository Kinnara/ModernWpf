using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = RootGridName, Type = typeof(Grid))]
    [TemplatePart(Name = SwipeContentRootName, Type = typeof(Grid))]
    [TemplatePart(Name = SwipeContentStackPanelName, Type = typeof(StackPanel))]
    [TemplatePart(Name = ContentRootName, Type = typeof(Grid))]
    [TemplatePart(Name = ContentPresenterName, Type = typeof(ContentPresenterEx))]
    [TemplatePart(Name = InputEaterName, Type = typeof(Grid))]
    public class SwipeControl : ContentControl
    {
        private const double DragThreshold = 8;
        private const double ThresholdValue = 100;
        private const double Epsilon = 0.0001;
        private const string RootGridName = "RootGrid";
        private const string SwipeContentRootName = "SwipeContentRoot";
        private const string SwipeContentStackPanelName = "SwipeContentStackPanel";
        private const string ContentRootName = "ContentRoot";
        private const string ContentPresenterName = "ContentPresenter";
        private const string InputEaterName = "InputEater";
        private const string SwipeItemStyleKey = "SwipeItemStyle";
        private const string SwipeItemBackgroundKey = "SwipeItemBackground";
        private const string SwipeItemForegroundKey = "SwipeItemForeground";
        private const string SwipeItemPreThresholdExecuteForegroundKey = "SwipeItemPreThresholdExecuteForeground";
        private const string SwipeItemPreThresholdExecuteBackgroundKey = "SwipeItemPreThresholdExecuteBackground";
        private const string SwipeItemPostThresholdExecuteForegroundKey = "SwipeItemPostThresholdExecuteForeground";
        private const string SwipeItemPostThresholdExecuteBackgroundKey = "SwipeItemPostThresholdExecuteBackground";

        private enum CreatedContent
        {
            None,
            Left,
            Top,
            Bottom,
            Right
        }

        static SwipeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SwipeControl),
                new FrameworkPropertyMetadata(typeof(SwipeControl)));
        }

        public SwipeControl()
        {
            IsTabStop = false;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
            PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            PreviewMouseMove += OnPreviewMouseMove;
            PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            LostMouseCapture += OnLostMouseCapture;
        }

        public static readonly DependencyProperty ContentTransitionsProperty =
            ControlHelper.ContentTransitionsProperty.AddOwner(typeof(SwipeControl));

        public TransitionCollection ContentTransitions
        {
            get => (TransitionCollection)GetValue(ContentTransitionsProperty);
            set => SetValue(ContentTransitionsProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(SwipeControl));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
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
            if (_isOpen && !_isInteracting)
            {
                CloseWithoutAnimation();
            }
        }

        public override void OnApplyTemplate()
        {
            DetachTemplateEventHandlers();

            base.OnApplyTemplate();

            _templateApplied = true;
            ThrowIfHasVerticalAndHorizontalContent(setIsHorizontal: true);
            GetTemplateParts();
            EnsureClip();
            AttachTemplateEventHandlers();
            CloseWithoutAnimation();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_rootGrid != null)
            {
                _rootGrid.Measure(availableSize);
                var desiredSize = _rootGrid.DesiredSize;

                if (!double.IsInfinity(availableSize.Width))
                {
                    desiredSize.Width = availableSize.Width;
                }

                if (!double.IsInfinity(availableSize.Height))
                {
                    desiredSize.Height = availableSize.Height;
                }

                return desiredSize;
            }

            return base.MeasureOverride(availableSize);
        }

        internal void ValidateSwipeItemsCanAdd(SwipeItemsPlacement placement)
        {
            var hasHorizontal = HasHorizontalItems() || IsHorizontalPlacement(placement);
            var hasVertical = HasVerticalItems() || IsVerticalPlacement(placement);
            ThrowIfInvalidAxis(hasHorizontal, hasVertical);
        }

        internal void OnSwipeItemsChanged()
        {
            ThrowIfHasVerticalAndHorizontalContent();

            if (_createdContent != CreatedContent.None && !HasItems(GetItemsForCreatedContent(_createdContent)))
            {
                CloseWithoutAnimation();
                return;
            }

            if (_createdContent != CreatedContent.None)
            {
                CreateContent(_createdContent);
                ApplySwipeValue(_currentValue);
            }
        }

        internal bool IsOpenForTesting => _isOpen;

        internal SwipeItemsPlacement OpenedItemsPlacementForTesting => GetPlacementFromCreatedContent(_createdContent);

        internal double HorizontalOffsetForTesting => _isHorizontal ? _currentValue : 0;

        internal double VerticalOffsetForTesting => !_isHorizontal ? _currentValue : 0;

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

            swipeControl.OnSwipeItemsChanged();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CloseWithoutAnimation();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachDismissingHandlers();

            if (ReferenceEquals(s_lastInteractedWithSwipeControl, this))
            {
                s_lastInteractedWithSwipeControl = null;
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            EnsureClip();
            UpdateButtonSizes();
            ApplySwipeValue(_currentValue);
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsWithinSwipeItems(e.OriginalSource as DependencyObject))
            {
                return;
            }

            if (_isOpen && IsWithinInputEater(e.OriginalSource as DependencyObject))
            {
                CloseWithoutAnimation();
                e.Handled = true;
                return;
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

        private void StartDrag(Point position)
        {
            _isPointerDown = true;
            _isDragging = false;
            _isInteracting = true;
            _dragStartPoint = position;
            _dragStartValue = _currentValue;
            _dragStartedOpen = _isOpen;
            _dragCreatedContent = _createdContent;
        }

        private void UpdateDrag(Point position)
        {
            var horizontalDelta = position.X - _dragStartPoint.X;
            var verticalDelta = position.Y - _dragStartPoint.Y;
            var candidateContent = _dragCreatedContent != CreatedContent.None
                ? _dragCreatedContent
                : GetCreatedContentFromDelta(horizontalDelta, verticalDelta);

            if (candidateContent == CreatedContent.None)
            {
                return;
            }

            var axisDelta = IsHorizontalCreatedContent(candidateContent) ? horizontalDelta : verticalDelta;
            if (!_isDragging && Math.Abs(axisDelta) < DragThreshold)
            {
                return;
            }

            if (!ReferenceEquals(s_lastInteractedWithSwipeControl, this))
            {
                s_lastInteractedWithSwipeControl?.CloseIfNotRemainOpenExecuteItem();
                s_lastInteractedWithSwipeControl = this;
            }

            _isDragging = true;
            _createdContent = candidateContent;
            CreateContent(candidateContent);

            var openValue = GetOpenValue(candidateContent);
            var value = _dragStartedOpen ? _dragStartValue + axisDelta : axisDelta;
            value = ClampValue(candidateContent, value, openValue);

            ApplySwipeValue(value);
            UpdateThresholdReached(value);
        }

        private void CompleteDrag()
        {
            var wasDragging = _isDragging;
            var createdContent = _createdContent;
            var startedOpen = _dragStartedOpen;

            _isPointerDown = false;
            _isDragging = false;
            _isInteracting = false;
            _dragCreatedContent = CreatedContent.None;
            _dragStartedOpen = false;

            if (!wasDragging || createdContent == CreatedContent.None)
            {
                return;
            }

            var currentItems = _currentItems;
            var openValue = GetOpenValue(createdContent);
            var shouldOpen = startedOpen
                ? Math.Abs(_currentValue) >= Math.Max(0, Math.Abs(openValue) - Epsilon)
                : IsThresholdReached(_currentValue);

            if (!shouldOpen)
            {
                CloseWithoutAnimation();
                return;
            }

            OpenToValue(createdContent, openValue);

            if (currentItems?.Mode == SwipeMode.Execute && currentItems.Count > 0)
            {
                currentItems[0].Invoke(this);

                if (currentItems[0].BehaviorOnInvoked == SwipeBehaviorOnInvoked.RemainOpen)
                {
                    OpenToValue(createdContent, openValue);
                }
            }
        }

        private void OpenToValue(CreatedContent createdContent, double value)
        {
            _createdContent = createdContent;
            CreateContent(createdContent);
            ApplySwipeValue(value);
            UpdateIsOpen(true);
        }

        private void CloseWithoutAnimation()
        {
            ApplySwipeValue(0);
            UpdateIsOpen(false);
            ClearContent();
            _createdContent = CreatedContent.None;
            _currentItems = null;
            _thresholdReached = false;
            _dragCreatedContent = CreatedContent.None;
            _dragStartedOpen = false;
            _isInteracting = false;
        }

        private void CloseIfNotRemainOpenExecuteItem()
        {
            if (_isOpen &&
                _currentItems?.Mode == SwipeMode.Execute &&
                _currentItems.Count > 0 &&
                _currentItems[0].BehaviorOnInvoked == SwipeBehaviorOnInvoked.RemainOpen)
            {
                return;
            }

            CloseWithoutAnimation();
        }

        private void UpdateIsOpen(bool isOpen)
        {
            if (_isOpen == isOpen)
            {
                UpdateInputEater();
                return;
            }

            _isOpen = isOpen;

            if (_isOpen)
            {
                if (_currentItems?.Mode != SwipeMode.Execute)
                {
                    AttachDismissingHandlers();
                }
            }
            else
            {
                DetachDismissingHandlers();

                if (ReferenceEquals(s_lastInteractedWithSwipeControl, this))
                {
                    s_lastInteractedWithSwipeControl = null;
                }
            }

            UpdateInputEater();
        }

        private void GetTemplateParts()
        {
            _rootGrid = GetTemplateChild(RootGridName) as Grid;
            _swipeContentRoot = GetTemplateChild(SwipeContentRootName) as Grid;
            _swipeContentStackPanel = GetTemplateChild(SwipeContentStackPanelName) as StackPanel;
            _contentRoot = GetTemplateChild(ContentRootName) as Grid;
            _contentPresenter = GetTemplateChild(ContentPresenterName) as ContentPresenterEx;
            _inputEater = GetTemplateChild(InputEaterName) as Grid;

            if (_rootGrid != null && _swipeContentRoot == null)
            {
                _swipeContentRoot = new Grid
                {
                    Name = SwipeContentRootName
                };
                _rootGrid.Children.Insert(0, _swipeContentRoot);
            }

            if (_swipeContentRoot != null && _swipeContentStackPanel == null)
            {
                _swipeContentStackPanel = new StackPanel
                {
                    Name = SwipeContentStackPanelName
                };
                _swipeContentRoot.Children.Add(_swipeContentStackPanel);
            }

            if (_contentRoot != null)
            {
                _contentTransform = EnsureTranslateTransform(_contentRoot);
            }

            if (_swipeContentStackPanel != null)
            {
                _swipeContentStackPanelTransform = EnsureTranslateTransform(_swipeContentStackPanel);
                _swipeContentStackPanel.Orientation = _isHorizontal ? Orientation.Horizontal : Orientation.Vertical;
            }

            _swipeItemStyle = FindSwipeItemStyle();
            UpdateInputEater();
        }

        private void AttachTemplateEventHandlers()
        {
            if (_inputEater != null)
            {
                _inputEater.MouseLeftButtonDown += OnInputEaterMouseLeftButtonDown;
            }
        }

        private void DetachTemplateEventHandlers()
        {
            if (_inputEater != null)
            {
                _inputEater.MouseLeftButtonDown -= OnInputEaterMouseLeftButtonDown;
            }
        }

        private void OnInputEaterMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isOpen)
            {
                CloseWithoutAnimation();
                e.Handled = true;
            }
        }

        private void AttachDismissingHandlers()
        {
            var window = Window.GetWindow(this);
            if (ReferenceEquals(_dismissWindow, window))
            {
                return;
            }

            DetachDismissingHandlers();
            _dismissWindow = window;

            if (_dismissWindow != null)
            {
                _dismissWindow.PreviewMouseDown += OnWindowPreviewMouseDown;
                _dismissWindow.PreviewKeyDown += OnWindowPreviewKeyDown;
            }
        }

        private void DetachDismissingHandlers()
        {
            if (_dismissWindow != null)
            {
                _dismissWindow.PreviewMouseDown -= OnWindowPreviewMouseDown;
                _dismissWindow.PreviewKeyDown -= OnWindowPreviewKeyDown;
                _dismissWindow = null;
            }
        }

        private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isOpen)
            {
                return;
            }

            if (!IsWithinThisControl(e.OriginalSource as DependencyObject))
            {
                CloseWithoutAnimation();
            }
        }

        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_isOpen && e.Key == Key.Escape)
            {
                CloseWithoutAnimation();
                e.Handled = true;
            }
        }

        private void CreateContent(CreatedContent createdContent)
        {
            if (_swipeContentStackPanel == null)
            {
                return;
            }

            var items = GetItemsForCreatedContent(createdContent);
            if (!HasItems(items))
            {
                return;
            }

            _createdContent = createdContent;
            _currentItems = items;
            _isHorizontal = IsHorizontalCreatedContent(createdContent);
            _swipeContentStackPanel.Orientation = _isHorizontal ? Orientation.Horizontal : Orientation.Vertical;
            _swipeContentStackPanel.Children.Clear();

            AlignStackPanel(createdContent);

            foreach (var swipeItem in items)
            {
                _swipeContentStackPanel.Children.Add(GetSwipeItemButton(swipeItem));
            }

            UpdateColors();
            UpdateButtonSizes();
        }

        private AppBarButton GetSwipeItemButton(SwipeItem swipeItem)
        {
            var itemAsButton = new AppBarButton();
            swipeItem.GenerateControl(itemAsButton, _swipeItemStyle);

            if (swipeItem.Background == null)
            {
                itemAsButton.Background = GetBrush(_currentItems?.Mode == SwipeMode.Reveal
                    ? SwipeItemBackgroundKey
                    : _thresholdReached
                        ? SwipeItemPostThresholdExecuteBackgroundKey
                        : SwipeItemPreThresholdExecuteBackgroundKey);
            }

            if (swipeItem.Foreground == null)
            {
                itemAsButton.Foreground = GetBrush(_currentItems?.Mode == SwipeMode.Reveal
                    ? SwipeItemForegroundKey
                    : _thresholdReached
                        ? SwipeItemPostThresholdExecuteForegroundKey
                        : SwipeItemPreThresholdExecuteForegroundKey);
            }

            SetSwipeItemButtonSize(itemAsButton);
            return itemAsButton;
        }

        private void AlignStackPanel(CreatedContent createdContent)
        {
            if (_swipeContentStackPanel == null)
            {
                return;
            }

            switch (createdContent)
            {
                case CreatedContent.Left:
                    _swipeContentStackPanel.HorizontalAlignment = HorizontalAlignment.Left;
                    _swipeContentStackPanel.VerticalAlignment = VerticalAlignment.Stretch;
                    break;
                case CreatedContent.Right:
                    _swipeContentStackPanel.HorizontalAlignment = HorizontalAlignment.Right;
                    _swipeContentStackPanel.VerticalAlignment = VerticalAlignment.Stretch;
                    break;
                case CreatedContent.Top:
                    _swipeContentStackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                    _swipeContentStackPanel.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case CreatedContent.Bottom:
                    _swipeContentStackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                    _swipeContentStackPanel.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                default:
                    _swipeContentStackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _swipeContentStackPanel.VerticalAlignment = VerticalAlignment.Stretch;
                    break;
            }
        }

        private void UpdateColors()
        {
            if (_currentItems == null)
            {
                return;
            }

            if (_currentItems.Mode == SwipeMode.Execute)
            {
                UpdateColorsIfExecuteItem();
            }
            else
            {
                UpdateColorsIfRevealItems();
            }
        }

        private void UpdateColorsIfExecuteItem()
        {
            if (_swipeContentStackPanel == null ||
                _currentItems == null ||
                _currentItems.Mode != SwipeMode.Execute)
            {
                return;
            }

            var swipeItem = _currentItems.Count > 0 ? _currentItems[0] : null;
            var background = _thresholdReached
                ? GetBrush(SwipeItemPostThresholdExecuteBackgroundKey)
                : GetBrush(SwipeItemPreThresholdExecuteBackgroundKey);
            var foreground = _thresholdReached
                ? GetBrush(SwipeItemPostThresholdExecuteForegroundKey)
                : GetBrush(SwipeItemPreThresholdExecuteForegroundKey);

            if (swipeItem?.Background != null)
            {
                background = swipeItem.Background;
            }

            if (swipeItem?.Foreground != null)
            {
                foreground = swipeItem.Foreground;
            }

            _swipeContentStackPanel.Background = background;

            if (_swipeContentRoot != null)
            {
                _swipeContentRoot.Background = null;
            }

            if (_swipeContentStackPanel.Children.OfType<AppBarButton>().FirstOrDefault() is AppBarButton button)
            {
                button.Foreground = foreground;
                button.Background = Brushes.Transparent;
            }
        }

        private void UpdateColorsIfRevealItems()
        {
            if (_currentItems == null || _currentItems.Mode != SwipeMode.Reveal)
            {
                return;
            }

            var background = GetBrush(SwipeItemBackgroundKey);

            if (_currentItems.Count > 0)
            {
                var backgroundItem = _createdContent == CreatedContent.Left || _createdContent == CreatedContent.Top
                    ? _currentItems[_currentItems.Count - 1]
                    : _currentItems[0];

                if (backgroundItem.Background != null)
                {
                    background = backgroundItem.Background;
                }
            }

            if (_swipeContentRoot != null)
            {
                _swipeContentRoot.Background = background;
            }

            if (_swipeContentStackPanel != null)
            {
                _swipeContentStackPanel.Background = null;
            }
        }

        private void ClearContent()
        {
            if (_swipeContentStackPanel != null)
            {
                _swipeContentStackPanel.Background = null;
                _swipeContentStackPanel.Children.Clear();
            }

            if (_swipeContentRoot != null)
            {
                _swipeContentRoot.Background = null;
            }
        }

        private void UpdateButtonSizes()
        {
            if (_swipeContentStackPanel == null)
            {
                return;
            }

            foreach (var button in _swipeContentStackPanel.Children.OfType<AppBarButton>())
            {
                SetSwipeItemButtonSize(button);
            }
        }

        private void SetSwipeItemButtonSize(AppBarButton button)
        {
            if (_isHorizontal)
            {
                button.Height = ActualHeight;
                button.Width = _currentItems?.Mode == SwipeMode.Execute ? ActualWidth : double.NaN;
            }
            else
            {
                button.Width = ActualWidth;
                button.Height = _currentItems?.Mode == SwipeMode.Execute ? ActualHeight : double.NaN;
            }
        }

        private void ApplySwipeValue(double value)
        {
            _currentValue = value;
            var visualValue = -value;

            if (_contentTransform != null)
            {
                if (_isHorizontal)
                {
                    _contentTransform.X = visualValue;
                    _contentTransform.Y = 0;
                }
                else
                {
                    _contentTransform.X = 0;
                    _contentTransform.Y = visualValue;
                }
            }

            if (_swipeContentStackPanelTransform != null)
            {
                _swipeContentStackPanelTransform.X = 0;
                _swipeContentStackPanelTransform.Y = 0;
            }
        }

        private void UpdateThresholdReached(double value)
        {
            var oldValue = _thresholdReached;
            _thresholdReached = IsThresholdReached(value);

            if (_thresholdReached != oldValue)
            {
                UpdateColorsIfExecuteItem();
            }
        }

        private bool IsThresholdReached(double value)
        {
            var effectiveStackPanelSize = Math.Max(0, GetSwipeContentStackPanelSize() - 1);
            return Math.Abs(value) > Math.Min(effectiveStackPanelSize, ThresholdValue);
        }

        private double GetSwipeContentStackPanelSize()
        {
            if (_swipeContentStackPanel == null)
            {
                return 0;
            }

            _swipeContentStackPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var size = _isHorizontal
                ? Math.Max(_swipeContentStackPanel.ActualWidth, _swipeContentStackPanel.DesiredSize.Width)
                : Math.Max(_swipeContentStackPanel.ActualHeight, _swipeContentStackPanel.DesiredSize.Height);
            return Math.Max(0, size);
        }

        private double GetOpenValue(CreatedContent createdContent)
        {
            var size = GetSwipeContentStackPanelSize();
            return createdContent == CreatedContent.Left || createdContent == CreatedContent.Top ? -size : size;
        }

        private static double ClampValue(CreatedContent createdContent, double value, double openValue)
        {
            if (createdContent == CreatedContent.Left || createdContent == CreatedContent.Top)
            {
                return Math.Min(0, Math.Max(openValue, value));
            }

            return Math.Max(0, Math.Min(openValue, value));
        }

        private CreatedContent GetCreatedContentFromDelta(double horizontalDelta, double verticalDelta)
        {
            if (_isHorizontal)
            {
                if (horizontalDelta < -Epsilon && HasItems(LeftItems))
                {
                    return CreatedContent.Left;
                }

                if (horizontalDelta > Epsilon && HasItems(RightItems))
                {
                    return CreatedContent.Right;
                }
            }
            else
            {
                if (verticalDelta < -Epsilon && HasItems(TopItems))
                {
                    return CreatedContent.Top;
                }

                if (verticalDelta > Epsilon && HasItems(BottomItems))
                {
                    return CreatedContent.Bottom;
                }
            }

            return CreatedContent.None;
        }

        private SwipeItems GetItemsForCreatedContent(CreatedContent createdContent)
        {
            switch (createdContent)
            {
                case CreatedContent.Left:
                    return LeftItems;
                case CreatedContent.Right:
                    return RightItems;
                case CreatedContent.Top:
                    return TopItems;
                case CreatedContent.Bottom:
                    return BottomItems;
                default:
                    return null;
            }
        }

        private static SwipeItemsPlacement GetPlacementFromCreatedContent(CreatedContent createdContent)
        {
            switch (createdContent)
            {
                case CreatedContent.Left:
                    return SwipeItemsPlacement.Left;
                case CreatedContent.Right:
                    return SwipeItemsPlacement.Right;
                case CreatedContent.Top:
                    return SwipeItemsPlacement.Top;
                case CreatedContent.Bottom:
                    return SwipeItemsPlacement.Bottom;
                default:
                    return SwipeItemsPlacement.None;
            }
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

            var hasHorizontal = HasHorizontalItems(excludePlacement: placement) ||
                (IsHorizontalPlacement(placement) && HasItems(items));
            var hasVertical = HasVerticalItems(excludePlacement: placement) ||
                (IsVerticalPlacement(placement) && HasItems(items));
            ThrowIfInvalidAxis(hasHorizontal, hasVertical);
        }

        private void ThrowIfHasVerticalAndHorizontalContent(bool setIsHorizontal = false)
        {
            var hasHorizontal = HasHorizontalItems();
            var hasVertical = HasVerticalItems();

            if (setIsHorizontal)
            {
                _isHorizontal = hasHorizontal || !hasVertical;
            }

            ThrowIfInvalidAxis(hasHorizontal, hasVertical);
        }

        private void ThrowIfInvalidAxis(bool hasHorizontal, bool hasVertical)
        {
            if (_templateApplied)
            {
                if (_isHorizontal && hasVertical)
                {
                    throw new ArgumentException("This SwipeControl is horizontal and can not have vertical items.");
                }

                if (!_isHorizontal && hasHorizontal)
                {
                    throw new ArgumentException("This SwipeControl is vertical and can not have horizontal items.");
                }
            }
            else if (hasHorizontal && hasVertical)
            {
                throw new ArgumentException("SwipeControl can't have both horizontal items and vertical items set at the same time.");
            }
        }

        private bool HasHorizontalItems(SwipeItemsPlacement excludePlacement = SwipeItemsPlacement.None)
        {
            return (excludePlacement != SwipeItemsPlacement.Left && HasItems(LeftItems)) ||
                (excludePlacement != SwipeItemsPlacement.Right && HasItems(RightItems));
        }

        private bool HasVerticalItems(SwipeItemsPlacement excludePlacement = SwipeItemsPlacement.None)
        {
            return (excludePlacement != SwipeItemsPlacement.Top && HasItems(TopItems)) ||
                (excludePlacement != SwipeItemsPlacement.Bottom && HasItems(BottomItems));
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

        private static bool IsHorizontalCreatedContent(CreatedContent createdContent)
        {
            return createdContent == CreatedContent.Left || createdContent == CreatedContent.Right;
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

        private bool IsWithinSwipeItems(DependencyObject source)
        {
            while (source != null)
            {
                if (ReferenceEquals(source, _swipeContentStackPanel))
                {
                    return true;
                }

                source = GetParent(source);
            }

            return false;
        }

        private bool IsWithinInputEater(DependencyObject source)
        {
            while (source != null)
            {
                if (ReferenceEquals(source, _inputEater))
                {
                    return true;
                }

                source = GetParent(source);
            }

            return false;
        }

        private bool IsWithinThisControl(DependencyObject source)
        {
            while (source != null)
            {
                if (ReferenceEquals(source, this))
                {
                    return true;
                }

                source = GetParent(source);
            }

            return false;
        }

        private static DependencyObject GetParent(DependencyObject source)
        {
            if (source == null)
            {
                return null;
            }

            var parent = VisualTreeHelper.GetParent(source);
            return parent ?? LogicalTreeHelper.GetParent(source);
        }

        private void EnsureClip()
        {
            if (_rootGrid != null)
            {
                _rootGrid.ClipToBounds = true;
            }
        }

        private void UpdateInputEater()
        {
            if (_inputEater == null)
            {
                return;
            }

            _inputEater.Visibility = _isOpen ? Visibility.Visible : Visibility.Collapsed;
            _inputEater.IsHitTestVisible = _isOpen;
        }

        private Brush GetBrush(string resourceKey)
        {
            return TryFindResource(resourceKey) as Brush ??
                _rootGrid?.TryFindResource(resourceKey) as Brush ??
                Application.Current?.TryFindResource(resourceKey) as Brush;
        }

        private Style FindSwipeItemStyle()
        {
            return TryFindResource(SwipeItemStyleKey) as Style ??
                _rootGrid?.TryFindResource(SwipeItemStyleKey) as Style ??
                Application.Current?.TryFindResource(SwipeItemStyleKey) as Style;
        }

        private static TranslateTransform EnsureTranslateTransform(UIElement element)
        {
            if (element.RenderTransform is TranslateTransform translateTransform)
            {
                return translateTransform;
            }

            translateTransform = new TranslateTransform();
            element.RenderTransform = translateTransform;
            return translateTransform;
        }

        private Grid _rootGrid;
        private Grid _swipeContentRoot;
        private StackPanel _swipeContentStackPanel;
        private Grid _contentRoot;
        private ContentPresenterEx _contentPresenter;
        private Grid _inputEater;
        private TranslateTransform _contentTransform;
        private TranslateTransform _swipeContentStackPanelTransform;
        private Style _swipeItemStyle;
        private Window _dismissWindow;
        private SwipeItems _currentItems;
        private Point _dragStartPoint;
        private double _dragStartValue;
        private double _currentValue;
        private bool _templateApplied;
        private bool _isHorizontal = true;
        private bool _isOpen;
        private bool _isPointerDown;
        private bool _isDragging;
        private bool _isInteracting;
        private bool _dragStartedOpen;
        private bool _thresholdReached;
        private CreatedContent _createdContent = CreatedContent.None;
        private CreatedContent _dragCreatedContent = CreatedContent.None;
        private static SwipeControl s_lastInteractedWithSwipeControl;
    }
}
