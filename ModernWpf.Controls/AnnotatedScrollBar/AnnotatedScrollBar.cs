using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = VerticalThumbName, Type = typeof(Border))]
    [TemplatePart(Name = VerticalThumbGhostName, Type = typeof(Border))]
    [TemplatePart(Name = VerticalDecrementRepeatButtonName, Type = typeof(RepeatButton))]
    [TemplatePart(Name = VerticalIncrementRepeatButtonName, Type = typeof(RepeatButton))]
    [TemplatePart(Name = VerticalGridName, Type = typeof(Grid))]
    [TemplatePart(Name = LabelsGridName, Type = typeof(Grid))]
    [TemplatePart(Name = TooltipContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = DetailLabelToolTipName, Type = typeof(ToolTip))]
    public partial class AnnotatedScrollBar : Control, IScrollController
    {
        private const string VerticalThumbName = "PART_VerticalThumb";
        private const string VerticalThumbGhostName = "PART_VerticalThumbGhost";
        private const string VerticalDecrementRepeatButtonName = "PART_VerticalDecrementRepeatButton";
        private const string VerticalIncrementRepeatButtonName = "PART_VerticalIncrementRepeatButton";
        private const string VerticalGridName = "PART_VerticalGrid";
        private const string LabelsGridName = "PART_LabelsGrid";
        private const string TooltipContentPresenterName = "PART_TooltipContentPresenter";
        private const string DetailLabelToolTipName = "PART_DetailLabelToolTip";

        private const double DefaultViewportToSmallChangeRatio = 8.0;
        private const double VelocityNeededPerPixel = 3.688880455092886;
        private const float SmallChangeInertiaDecayRate = 0.975f;

        static AnnotatedScrollBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AnnotatedScrollBar),
                new FrameworkPropertyMetadata(typeof(AnnotatedScrollBar)));
        }

        public AnnotatedScrollBar()
        {
            SetValue(LabelsProperty, new ObservableCollection<AnnotatedScrollBarLabel>());

            SizeChanged += OnSizeChanged;
            IsEnabledChanged += OnIsEnabledChanged;
        }

        public IScrollController ScrollController => this;

        public event TypedEventHandler<AnnotatedScrollBar, AnnotatedScrollBarScrollingEventArgs> Scrolling;

        public event TypedEventHandler<AnnotatedScrollBar, AnnotatedScrollBarDetailLabelRequestedEventArgs> DetailLabelRequested;

        public IScrollControllerPanningInfo PanningInfo
        {
            get
            {
                EnsurePanningInfo();
                return m_panningInfo;
            }
        }

        public bool CanScroll => m_canScroll;

        public bool IsScrollingWithMouse => m_isScrollingWithMouse;

        public event TypedEventHandler<IScrollController, object> CanScrollChanged;

        public event TypedEventHandler<IScrollController, object> IsScrollingWithMouseChanged;

        public event TypedEventHandler<IScrollController, ScrollControllerScrollToRequestedEventArgs> ScrollToRequested;

        public event TypedEventHandler<IScrollController, ScrollControllerScrollByRequestedEventArgs> ScrollByRequested;

        public event TypedEventHandler<IScrollController, ScrollControllerAddScrollVelocityRequestedEventArgs> AddScrollVelocityRequested;

        public override void OnApplyTemplate()
        {
            UnhookHandlers();

            base.OnApplyTemplate();

            m_verticalThumb = GetTemplateChild(VerticalThumbName) as Border;
            m_verticalThumbGhost = GetTemplateChild(VerticalThumbGhostName) as Border;
            m_verticalDecrementRepeatButton = GetTemplateChild(VerticalDecrementRepeatButtonName) as RepeatButton;
            m_verticalIncrementRepeatButton = GetTemplateChild(VerticalIncrementRepeatButtonName) as RepeatButton;
            m_verticalGrid = GetTemplateChild(VerticalGridName) as Grid;
            m_labelsGrid = GetTemplateChild(LabelsGridName) as Grid;
            m_tooltipContentPresenter = GetTemplateChild(TooltipContentPresenterName) as ContentPresenter;
            m_detailLabelToolTip = GetTemplateChild(DetailLabelToolTipName) as ToolTip;

            SetUpInteractionElements();
            HookInputEvents();
            UpdatePositionsOfAnchoredElements();
        }

        public void SetIsScrollable(bool isScrollable)
        {
            m_isScrollable = isScrollable;
            UpdateCanScroll();
        }

        public void SetValues(
            double minOffset,
            double maxOffset,
            double offset,
            double viewportLength)
        {
            if (maxOffset < minOffset)
            {
                throw new ArgumentException("maxOffset cannot be smaller than minOffset.", nameof(maxOffset));
            }

            if (viewportLength < 0.0)
            {
                throw new ArgumentException("viewportLength cannot be negative.", nameof(viewportLength));
            }

            offset = Math.Max(minOffset, Math.Min(maxOffset, offset));
            m_lastOffset = offset;

            Minimum(minOffset);
            Maximum(maxOffset);
            ViewportSize(viewportLength);

            if (m_operationsCount == 0)
            {
                Value(offset);
            }
        }

        public object GetScrollAnimation(
            int correlationId,
            Point startPosition,
            Point endPosition,
            object defaultAnimation)
        {
            return defaultAnimation;
        }

        public void NotifyRequestedScrollCompleted(int correlationId)
        {
            if (m_operationsCount > 0)
            {
                m_operationsCount--;
            }

            if (m_operationsCount == 0 && Value() != m_lastOffset)
            {
                Value(m_lastOffset);
            }
        }

        internal AnnotatedScrollBarScrollingEventArgs RaiseScrolling(
            double scrollOffset,
            AnnotatedScrollBarScrollingEventKind scrollingEventKind)
        {
            var args = new AnnotatedScrollBarScrollingEventArgs(scrollOffset, scrollingEventKind);
            Scrolling?.Invoke(this, args);
            return args;
        }

        internal AnnotatedScrollBarDetailLabelRequestedEventArgs RaiseDetailLabelRequested(double scrollOffset)
        {
            var args = new AnnotatedScrollBarDetailLabelRequestedEventArgs(scrollOffset);
            DetailLabelRequested?.Invoke(this, args);
            return args;
        }

        internal AnnotatedScrollBarScrollingEventArgs ScrollToRatioForTesting(
            double ratio,
            AnnotatedScrollBarScrollingEventKind eventKind)
        {
            var offsetFromTop = Math.Max(0, Math.Min(1, ratio)) * GetVerticalGridHeight();
            AnnotatedScrollBarScrollingEventArgs args = null;
            Scrolling += capture;
            try
            {
                ScrollTo(ConvertOffsetFromTopToScrollOffset(offsetFromTop), eventKind);
            }
            finally
            {
                Scrolling -= capture;
            }

            return args;

            void capture(AnnotatedScrollBar sender, AnnotatedScrollBarScrollingEventArgs e)
            {
                args = e;
            }
        }

        internal AnnotatedScrollBarDetailLabelRequestedEventArgs RequestDetailLabelForRatioForTesting(double ratio)
        {
            var offsetFromTop = Math.Max(0, Math.Min(1, ratio)) * GetVerticalGridHeight();
            return RaiseDetailLabelRequested(ConvertOffsetFromTopToDetailScrollOffset(offsetFromTop));
        }

        internal bool IsPointerOverForTesting => m_isPointerOver;

        private static void OnLabelsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var annotatedScrollBar = (AnnotatedScrollBar)d;
            annotatedScrollBar.UpdateCollectionChangedSubscription(e.OldValue, e.NewValue);
            annotatedScrollBar.ClearLabels();
            annotatedScrollBar.QueueLayoutLabels();
        }

        private static void OnAnnotatedScrollBarPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var annotatedScrollBar = (AnnotatedScrollBar)d;
            if (e.Property == LabelTemplateProperty)
            {
                annotatedScrollBar.ClearLabels();
                annotatedScrollBar.QueueLayoutLabels();
            }
            else if (e.Property == DetailLabelTemplateProperty)
            {
                annotatedScrollBar.m_detailLabelTemplateApplied = false;
            }
        }

        private void ViewportSize(double viewportSize)
        {
            if (viewportSize != m_viewportSize)
            {
                m_viewportSize = viewportSize;
                UpdatePositionsOfAnchoredElements();
            }
        }

        private double ViewportSize()
        {
            return m_viewportSize;
        }

        private void Maximum(double maximum)
        {
            if (maximum != m_maximum)
            {
                m_maximum = maximum;
                UpdatePositionsOfAnchoredElements();
            }
        }

        private double Maximum()
        {
            return m_maximum;
        }

        private void Minimum(double minimum)
        {
            if (minimum != m_minimum)
            {
                m_minimum = minimum;
                UpdatePositionsOfAnchoredElements();
            }
        }

        private double Minimum()
        {
            return m_minimum;
        }

        private void Value(double value)
        {
            if (value != m_value)
            {
                m_value = value;
                UpdateThumbOffset();
            }
        }

        private double Value()
        {
            return m_value;
        }

        private void UpdateCollectionChangedSubscription(object oldValue, object newValue)
        {
            if (oldValue is INotifyCollectionChanged oldObservable)
            {
                oldObservable.CollectionChanged -= OnLabelsCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged newObservable)
            {
                newObservable.CollectionChanged += OnLabelsCollectionChanged;
            }
        }

        private void OnLabelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ClearLabels();
            QueueLayoutLabels();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            UpdatePositionsOfAnchoredElements();
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (!IsEnabled)
            {
                m_isPressed = false;
                m_isPointerOver = false;

                if (m_hasMouseCapture && m_verticalGrid != null)
                {
                    m_verticalGrid.ReleaseMouseCapture();
                    m_hasMouseCapture = false;
                }

                ResetTrackedPointer();
            }

            UpdateCanScroll();
        }

        private void UnhookHandlers()
        {
            if (m_verticalGrid != null)
            {
                m_verticalGrid.MouseEnter -= OnVerticalGridMouseEnter;
                m_verticalGrid.MouseMove -= OnVerticalGridMouseMove;
                m_verticalGrid.MouseLeave -= OnVerticalGridMouseLeave;
                m_verticalGrid.MouseLeftButtonDown -= OnVerticalGridMouseLeftButtonDown;
                m_verticalGrid.LostMouseCapture -= OnVerticalGridLostMouseCapture;
            }

            if (m_mouseLeftButtonUpHandler != null)
            {
                RemoveHandler(MouseLeftButtonUpEvent, m_mouseLeftButtonUpHandler);
                m_mouseLeftButtonUpHandler = null;
            }

            if (m_verticalIncrementRepeatButton != null)
            {
                m_verticalIncrementRepeatButton.Click -= OnIncrementRepeatButtonClick;
            }

            if (m_verticalDecrementRepeatButton != null)
            {
                m_verticalDecrementRepeatButton.Click -= OnDecrementRepeatButtonClick;
            }

            if (m_labelsGrid != null)
            {
                m_labelsGrid.SizeChanged -= OnLabelsGridSizeChanged;
            }

            if (m_detailLabelToolTip != null)
            {
                m_detailLabelToolTip.Opened -= OnDetailLabelToolTipOpened;
            }
        }

        private void HookInputEvents()
        {
            if (m_verticalGrid != null)
            {
                m_verticalGrid.MouseEnter += OnVerticalGridMouseEnter;
                m_verticalGrid.MouseMove += OnVerticalGridMouseMove;
                m_verticalGrid.MouseLeave += OnVerticalGridMouseLeave;
                m_verticalGrid.MouseLeftButtonDown += OnVerticalGridMouseLeftButtonDown;
                m_verticalGrid.LostMouseCapture += OnVerticalGridLostMouseCapture;

                m_mouseLeftButtonUpHandler = OnVerticalGridMouseLeftButtonUp;
                AddHandler(MouseLeftButtonUpEvent, m_mouseLeftButtonUpHandler, true);
            }

            if (m_labelsGrid != null)
            {
                m_labelsGrid.SizeChanged += OnLabelsGridSizeChanged;
            }

            if (m_detailLabelToolTip != null)
            {
                m_detailLabelToolTip.Opened += OnDetailLabelToolTipOpened;
            }
        }

        private void SetUpInteractionElements()
        {
            if (m_verticalIncrementRepeatButton != null)
            {
                m_verticalIncrementRepeatButton.Click += OnIncrementRepeatButtonClick;
            }

            if (m_verticalDecrementRepeatButton != null)
            {
                m_verticalDecrementRepeatButton.Click += OnDecrementRepeatButtonClick;
            }

            EnsurePanningInfo();
            m_panningInfo.PanningFrameworkElement(m_verticalThumb);
            m_panningInfo.SetPanningElementAncestor(m_verticalGrid);
        }

        private void UpdatePositionsOfAnchoredElements()
        {
            UpdateScrollOffsetToLabelOffsetFactor();

            EnsurePanningInfo();
            m_panningInfo.PanningElementOffsetMultiplier(
                m_scrollOffsetToLabelOffsetFactor == 0
                    ? 0
                    : (float)(-1.0 / m_scrollOffsetToLabelOffsetFactor));

            UpdateThumbOffset();
            QueueLayoutLabels();
        }

        private void QueueLayoutLabels()
        {
            if (m_labelsLayoutQueued)
            {
                return;
            }

            m_labelsLayoutQueued = true;
            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    m_labelsLayoutQueued = false;
                    LayoutLabels();
                }));
        }

        private void LayoutLabels()
        {
            if (m_labelsGrid == null)
            {
                return;
            }

            if (m_labelsGrid.Children.Count == 0)
            {
                CreateLabelContainers();
            }
            else
            {
                UpdateLabelContainersOffsets();
            }

            CollapseCollidingAndOutOfBoundsLabels();
        }

        private void CreateLabelContainers()
        {
            if (m_labelsGrid == null)
            {
                return;
            }

            m_labelSizes.Clear();

            foreach (var label in Labels ?? Array.Empty<AnnotatedScrollBarLabel>())
            {
                var labelContentPresenter = new ContentPresenter
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Content = label,
                    ContentTemplate = LabelTemplate
                };

                labelContentPresenter.Measure(InfiniteSize);
                m_labelSizes.Add(labelContentPresenter.DesiredSize.Height);

                var labelVerticalOffset = GetLabelVerticalOffset(label);
                labelContentPresenter.Margin = new Thickness(0, labelVerticalOffset, 0, 0);
                m_labelsGrid.Children.Add(labelContentPresenter);
            }
        }

        private void UpdateLabelContainersOffsets()
        {
            if (m_labelsGrid == null)
            {
                return;
            }

            for (var i = 0; i < m_labelsGrid.Children.Count; i++)
            {
                if (m_labelsGrid.Children[i] is ContentPresenter labelContentPresenter &&
                    labelContentPresenter.Content is AnnotatedScrollBarLabel label)
                {
                    var labelVerticalOffset = GetLabelVerticalOffset(label);
                    labelContentPresenter.Margin = new Thickness(0, labelVerticalOffset, 0, 0);
                }
            }
        }

        private void CollapseCollidingAndOutOfBoundsLabels()
        {
            if (m_labelsGrid == null)
            {
                return;
            }

            var labelsGridHeight = m_labelsGrid.ActualHeight;
            var labelsSize = Labels?.Count ?? 0;
            if (labelsSize == 0 ||
                labelsGridHeight == 0 ||
                labelsSize != m_labelsGrid.Children.Count ||
                labelsSize != m_labelSizes.Count)
            {
                return;
            }

            double previousLabelTopPosition = -1;
            var previousLabelIndex = m_labelsGrid.Children.Count;

            for (var currentLabelIndex = previousLabelIndex - 1; currentLabelIndex >= 0; currentLabelIndex--)
            {
                if (!(m_labelsGrid.Children[currentLabelIndex] is FrameworkElement currentLabel))
                {
                    continue;
                }

                currentLabel.Visibility = Visibility.Visible;

                var currentLabelTopPosition = currentLabel.Margin.Top;
                var currentLabelBottomPosition = currentLabelTopPosition + m_labelSizes[currentLabelIndex];

                var isLabelOutOfBounds =
                    currentLabelTopPosition < 0 ||
                    currentLabelBottomPosition > labelsGridHeight;

                var isLabelColliding =
                    previousLabelTopPosition >= 0 &&
                    previousLabelTopPosition <= currentLabelBottomPosition;

                if (isLabelOutOfBounds)
                {
                    currentLabel.Visibility = Visibility.Collapsed;
                }
                else if (isLabelColliding)
                {
                    var indexToCollapse = currentLabelIndex;
                    if (currentLabelIndex == 0 &&
                        previousLabelIndex < m_labelsGrid.Children.Count - 1)
                    {
                        indexToCollapse = previousLabelIndex;
                    }

                    if (m_labelsGrid.Children[indexToCollapse] is FrameworkElement labelToCollapse)
                    {
                        labelToCollapse.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    previousLabelTopPosition = currentLabelTopPosition;
                    previousLabelIndex = currentLabelIndex;
                }
            }
        }

        private double GetLabelVerticalOffset(AnnotatedScrollBarLabel label)
        {
            return label.ScrollOffset * m_scrollOffsetToLabelOffsetFactor;
        }

        private void UpdateScrollOffsetToLabelOffsetFactor()
        {
            var scrollOffsetToLabelOffsetFactor = 1.0;

            if (m_labelsGrid != null && m_verticalThumb != null)
            {
                var scrollViewScrollableHeight = Maximum() - Minimum();
                scrollViewScrollableHeight = scrollViewScrollableHeight == 0 ? 1 : scrollViewScrollableHeight;
                var labelsHeight = m_labelsGrid.ActualHeight;
                var thumbHeight = m_verticalThumb.ActualHeight;
                scrollOffsetToLabelOffsetFactor =
                    (labelsHeight - thumbHeight) /
                    scrollViewScrollableHeight *
                    (1 - ViewportSize() / (ViewportSize() + scrollViewScrollableHeight));
            }

            m_scrollOffsetToLabelOffsetFactor =
                double.IsNaN(scrollOffsetToLabelOffsetFactor) || double.IsInfinity(scrollOffsetToLabelOffsetFactor)
                    ? 1.0
                    : scrollOffsetToLabelOffsetFactor;
        }

        private void EnsurePanningInfo()
        {
            if (m_panningInfo == null)
            {
                m_panningInfo = new AnnotatedScrollBarPanningInfo();
            }
        }

        private void EnsureSmallChangeValue()
        {
            if (SmallChange == 0.0)
            {
                SmallChange = ViewportSize() / DefaultViewportToSmallChangeRatio;
            }
        }

        private void SetThumbGhostVisibility(Visibility visibility)
        {
            if (m_verticalThumbGhost != null)
            {
                m_verticalThumbGhost.Visibility = visibility;
            }
        }

        private void ShowHoverVisuals(double offsetFromTopToolTip, double offsetFromTopThumbGhost)
        {
            ShowToolTipAtOffset(offsetFromTopToolTip);
            ShowThumbGhostAtOffset(offsetFromTopThumbGhost);
        }

        private void HideHoverVisuals()
        {
            SetToolTipIsOpen(false);
            SetThumbGhostVisibility(Visibility.Collapsed);
        }

        private void SetToolTipIsOpen(bool isOpen)
        {
            if (DetailLabelRequested == null)
            {
                return;
            }

            if (m_detailLabelToolTip != null)
            {
                m_detailLabelToolTip.IsOpen = isOpen;
            }
        }

        private void ShowToolTipAtOffset(double offsetFromTop)
        {
            var data = RaiseDetailLabelRequested(ConvertOffsetFromTopToDetailScrollOffset(offsetFromTop)).Content;
            if (data == null ||
                m_detailLabelToolTip == null ||
                m_tooltipContentPresenter == null ||
                m_verticalGrid == null)
            {
                return;
            }

            m_tooltipContentPresenter.Content = data;

            if (!m_detailLabelTemplateApplied ||
                m_tooltipContentPresenter.ContentTemplate == null)
            {
                if (DetailLabelTemplate != null)
                {
                    m_tooltipContentPresenter.ContentTemplate = DetailLabelTemplate;
                    m_detailLabelTemplateApplied = true;
                }
            }

            m_tooltipContentPresenter.Measure(InfiniteSize);
            var detailLabelToolTipWidth = m_tooltipContentPresenter.DesiredSize.Width;
            var horizontalPosition = (-1 * detailLabelToolTipWidth / 2) + 2;

            m_detailLabelToolTip.PlacementTarget = m_verticalGrid;
            m_detailLabelToolTip.Placement = PlacementMode.Relative;
            m_detailLabelToolTip.HorizontalOffset = horizontalPosition;
            m_detailLabelToolTip.VerticalOffset = offsetFromTop;
            SetToolTipIsOpen(true);
        }

        private void ShowThumbGhostAtOffset(double offsetFromTop)
        {
            if (m_verticalThumbGhost != null)
            {
                m_verticalThumbGhost.Margin = new Thickness(
                    m_verticalThumbGhost.Margin.Left,
                    offsetFromTop,
                    m_verticalThumbGhost.Margin.Right,
                    m_verticalThumbGhost.Margin.Bottom);
                SetThumbGhostVisibility(Visibility.Visible);
            }
        }

        private void UpdateCanScroll()
        {
            var oldCanScroll = m_canScroll;

            m_canScroll = m_isScrollable && IsEnabled;

            if (oldCanScroll != m_canScroll)
            {
                RaiseCanScrollChanged();
            }
        }

        private void OnDetailLabelToolTipOpened(object sender, RoutedEventArgs args)
        {
            if (DetailLabelRequested == null)
            {
                SetToolTipIsOpen(false);
            }
        }

        private void OnIncrementRepeatButtonClick(object sender, RoutedEventArgs args)
        {
            ScrollTo(0, AnnotatedScrollBarScrollingEventKind.IncrementButton);
        }

        private void OnDecrementRepeatButtonClick(object sender, RoutedEventArgs args)
        {
            ScrollTo(0, AnnotatedScrollBarScrollingEventKind.DecrementButton);
        }

        private void ResetTrackedPointer()
        {
            m_isPointerOver = false;
        }

        private void OnVerticalGridMouseEnter(object sender, MouseEventArgs args)
        {
            m_isPointerOver = true;

            var offsetFromTop = args.GetPosition(m_verticalGrid).Y;
            ShowHoverVisuals(offsetFromTop, offsetFromTop);
        }

        private void OnVerticalGridMouseMove(object sender, MouseEventArgs args)
        {
            m_isPointerOver = true;

            var offsetFromTop = ClampOffsetFromTop(args.GetPosition(m_verticalGrid).Y);

            if (m_detailLabelToolTip != null &&
                m_detailLabelToolTip.IsOpen &&
                offsetFromTop == m_lastVerticalGridPointerMovedOffset)
            {
                return;
            }

            m_lastVerticalGridPointerMovedOffset = offsetFromTop;

            ShowHoverVisuals(offsetFromTop, offsetFromTop);

            if (m_isPressed || m_hasMouseCapture)
            {
                ScrollTo(
                    ConvertOffsetFromTopToScrollOffset(offsetFromTop),
                    AnnotatedScrollBarScrollingEventKind.Drag);
            }
        }

        private void OnVerticalGridMouseLeave(object sender, MouseEventArgs args)
        {
            if (m_verticalGrid == null)
            {
                return;
            }

            if (IsOutOfVerticalGridBounds(args.GetPosition(m_verticalGrid)) && !m_hasMouseCapture)
            {
                m_isPressed = false;
                m_isPointerOver = false;
                ResetTrackedPointer();
                HideHoverVisuals();
            }
        }

        private void OnVerticalGridMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            if (m_verticalGrid == null)
            {
                return;
            }

            m_isPressed = true;

            var newScrollOffset = ConvertOffsetFromTopToScrollOffset(args.GetPosition(m_verticalGrid).Y);
            ScrollTo(newScrollOffset, AnnotatedScrollBarScrollingEventKind.Click);

            if (m_verticalGrid.CaptureMouse())
            {
                m_hasMouseCapture = true;
            }

            if (!m_isScrollingWithMouse)
            {
                m_isScrollingWithMouse = true;
                RaiseIsScrollingWithMouseChanged();
            }

            args.Handled = true;
        }

        private void OnVerticalGridMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
        {
            m_isPressed = false;

            if (m_hasMouseCapture && m_verticalGrid != null)
            {
                m_verticalGrid.ReleaseMouseCapture();
                m_hasMouseCapture = false;

                if (m_isScrollingWithMouse)
                {
                    m_isScrollingWithMouse = false;
                    RaiseIsScrollingWithMouseChanged();
                }
            }
        }

        private void OnVerticalGridLostMouseCapture(object sender, MouseEventArgs args)
        {
            m_isPressed = false;

            if (m_verticalGrid != null &&
                IsOutOfVerticalGridBounds(Mouse.GetPosition(m_verticalGrid)))
            {
                m_isPointerOver = false;
                HideHoverVisuals();
            }

            if (m_isScrollingWithMouse)
            {
                m_isScrollingWithMouse = false;
                RaiseIsScrollingWithMouseChanged();
            }

            m_hasMouseCapture = false;
            ResetTrackedPointer();
        }

        private bool IsOutOfVerticalGridBounds(Point point)
        {
            if (m_verticalGrid != null)
            {
                const double tolerance = 1.0;
                var actualWidth = m_verticalGrid.ActualWidth;
                var actualHeight = m_verticalGrid.ActualHeight;
                return point.X < tolerance ||
                    point.X > actualWidth - tolerance ||
                    point.Y < tolerance ||
                    point.Y > actualHeight - tolerance;
            }

            return true;
        }

        private void ScrollTo(double offset, AnnotatedScrollBarScrollingEventKind scrollingEventKind)
        {
            var scrollOffset = offset;
            double changeAmount = 0;

            switch (scrollingEventKind)
            {
                case AnnotatedScrollBarScrollingEventKind.IncrementButton:
                    EnsureSmallChangeValue();
                    changeAmount = -1 * SmallChange;
                    scrollOffset = Value() + changeAmount;
                    scrollOffset = Math.Max(scrollOffset, Minimum());
                    break;

                case AnnotatedScrollBarScrollingEventKind.DecrementButton:
                    EnsureSmallChangeValue();
                    changeAmount = SmallChange;
                    scrollOffset = Value() + changeAmount;
                    scrollOffset = Math.Min(scrollOffset, Maximum());
                    break;
            }

            if (RaiseScrolling(scrollOffset, scrollingEventKind).Cancel)
            {
                return;
            }

            switch (scrollingEventKind)
            {
                case AnnotatedScrollBarScrollingEventKind.Click:
                case AnnotatedScrollBarScrollingEventKind.Drag:
                    RaiseScrollToRequested(scrollOffset);
                    break;

                case AnnotatedScrollBarScrollingEventKind.IncrementButton:
                case AnnotatedScrollBarScrollingEventKind.DecrementButton:
                    if (SharedHelpers.IsAnimationsEnabled)
                    {
                        RaiseAddScrollVelocityRequested(changeAmount);
                    }
                    else
                    {
                        RaiseScrollByRequested(changeAmount);
                    }
                    break;
            }
        }

        private void RaiseCanScrollChanged()
        {
            CanScrollChanged?.Invoke(this, null);
        }

        private void RaiseIsScrollingWithMouseChanged()
        {
            IsScrollingWithMouseChanged?.Invoke(this, null);
        }

        private void RaiseScrollToRequested(double offset)
        {
            if (ScrollToRequested == null)
            {
                return;
            }

            var options = new ScrollingScrollOptions(
                ScrollingAnimationMode.Disabled,
                ScrollingSnapPointsMode.Ignore);
            var args = new ScrollControllerScrollToRequestedEventArgs(offset, options);

            ScrollToRequested(this, args);

            var offsetChangeCorrelationId = args.CorrelationId;
            if (offsetChangeCorrelationId != -1 &&
                offsetChangeCorrelationId != m_lastOffsetChangeCorrelationIdForScrollTo)
            {
                m_lastOffsetChangeCorrelationIdForScrollTo = offsetChangeCorrelationId;
                m_operationsCount++;
            }
        }

        private void RaiseScrollByRequested(double offsetDelta)
        {
            if (ScrollByRequested == null)
            {
                return;
            }

            var options = new ScrollingScrollOptions(
                ScrollingAnimationMode.Disabled,
                ScrollingSnapPointsMode.Ignore);
            var args = new ScrollControllerScrollByRequestedEventArgs(offsetDelta, options);

            ScrollByRequested(this, args);

            var offsetChangeCorrelationId = args.CorrelationId;
            if (offsetChangeCorrelationId != -1 &&
                offsetChangeCorrelationId != m_lastOffsetChangeCorrelationIdForScrollBy)
            {
                m_lastOffsetChangeCorrelationIdForScrollBy = offsetChangeCorrelationId;
                m_operationsCount++;
            }
        }

        private void RaiseAddScrollVelocityRequested(double offsetDelta)
        {
            if (AddScrollVelocityRequested == null)
            {
                return;
            }

            var offsetVelocity = offsetDelta * VelocityNeededPerPixel;
            var args = new ScrollControllerAddScrollVelocityRequestedEventArgs(
                (float)offsetVelocity,
                SmallChangeInertiaDecayRate);

            AddScrollVelocityRequested(this, args);

            var offsetChangeCorrelationId = args.CorrelationId;
            if (offsetChangeCorrelationId != -1 &&
                offsetChangeCorrelationId != m_lastOffsetChangeCorrelationIdForAddScrollVelocity)
            {
                m_lastOffsetChangeCorrelationIdForAddScrollVelocity = offsetChangeCorrelationId;
                m_operationsCount++;
            }
        }

        private void OnLabelsGridSizeChanged(object sender, SizeChangedEventArgs args)
        {
            UpdatePositionsOfAnchoredElements();
        }

        private void ClearLabels()
        {
            if (m_labelsGrid != null)
            {
                m_labelsGrid.Children.Clear();
            }

            m_labelSizes.Clear();
        }

        private void UpdateThumbOffset()
        {
            if (m_verticalThumb != null)
            {
                var offsetFromTop = Value() * m_scrollOffsetToLabelOffsetFactor;
                m_verticalThumb.Margin = new Thickness(
                    m_verticalThumb.Margin.Left,
                    offsetFromTop,
                    m_verticalThumb.Margin.Right,
                    m_verticalThumb.Margin.Bottom);
            }
        }

        private double ConvertOffsetFromTopToScrollOffset(double offsetFromTop)
        {
            return m_scrollOffsetToLabelOffsetFactor == 0
                ? 0
                : offsetFromTop / m_scrollOffsetToLabelOffsetFactor;
        }

        private double ConvertOffsetFromTopToDetailScrollOffset(double offsetFromTop)
        {
            return Math.Min(ConvertOffsetFromTopToScrollOffset(offsetFromTop), Maximum() + ViewportSize());
        }

        private double ClampOffsetFromTop(double offsetFromTop)
        {
            return Math.Max(0, Math.Min(GetVerticalGridHeight(), offsetFromTop));
        }

        private double GetVerticalGridHeight()
        {
            return Math.Max(1, m_verticalGrid?.ActualHeight ?? ActualHeight);
        }

        private static readonly Size InfiniteSize = new Size(
            double.PositiveInfinity,
            double.PositiveInfinity);

        private double m_viewportSize;
        private double m_maximum;
        private double m_minimum;
        private double m_value;
        private double m_scrollOffsetToLabelOffsetFactor = 1.0;

        private Border m_verticalThumb;
        private Border m_verticalThumbGhost;
        private RepeatButton m_verticalDecrementRepeatButton;
        private RepeatButton m_verticalIncrementRepeatButton;
        private Grid m_verticalGrid;
        private Grid m_labelsGrid;
        private ContentPresenter m_tooltipContentPresenter;
        private ToolTip m_detailLabelToolTip;

        private AnnotatedScrollBarPanningInfo m_panningInfo;
        private readonly List<double> m_labelSizes = new List<double>();
        private MouseButtonEventHandler m_mouseLeftButtonUpHandler;

        private bool m_isPressed;
        private bool m_isPointerOver;
        private bool m_hasMouseCapture;
        private bool m_detailLabelTemplateApplied;
        private bool m_canScroll;
        private bool m_isScrollingWithMouse;
        private bool m_isScrollable;
        private bool m_labelsLayoutQueued;
        private int m_operationsCount;
        private double m_lastOffset;
        private double m_lastVerticalGridPointerMovedOffset;
        private int m_lastOffsetChangeCorrelationIdForAddScrollVelocity = -1;
        private int m_lastOffsetChangeCorrelationIdForScrollBy = -1;
        private int m_lastOffsetChangeCorrelationIdForScrollTo = -1;
    }
}
