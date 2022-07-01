using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static CppWinRTHelpers;
using static ModernWpf.ResourceAccessor;
using ButtonVisibility = ModernWpf.Controls.PipsPagerButtonVisibility;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents a control that enables navigation within linearly paginated content using a configurable collection of glyphs, each of which represents a single "page" within a limitless range.
    /// </summary>
    public partial class PipsPager : Control, IControlProtected
    {
        private const string s_pipButtonHandlersPropertyName = "PipButtonHandlers";

        private const string c_previousPageButtonVisibleVisualState = "PreviousPageButtonVisible";
        private const string c_previousPageButtonHiddenVisualState = "PreviousPageButtonHidden";
        private const string c_previousPageButtonCollapsedVisualState = "PreviousPageButtonCollapsed";

        private const string c_previousPageButtonEnabledVisualState = "PreviousPageButtonEnabled";
        private const string c_previousPageButtonDisabledVisualState = "PreviousPageButtonDisabled";

        private const string c_nextPageButtonVisibleVisualState = "NextPageButtonVisible";
        private const string c_nextPageButtonHiddenVisualState = "NextPageButtonHidden";
        private const string c_nextPageButtonCollapsedVisualState = "NextPageButtonCollapsed";

        private const string c_nextPageButtonEnabledVisualState = "NextPageButtonEnabled";
        private const string c_nextPageButtonDisabledVisualState = "NextPageButtonDisabled";

        private const string c_previousPageButtonName = "PreviousPageButton";
        private const string c_nextPageButtonName = "NextPageButton";

        private const string c_pipsPagerRepeaterName = "PipsPagerItemsRepeater";
        private const string c_pipsPagerScrollViewerName = "PipsPagerScrollViewer";

        private const string c_pipsPagerVerticalOrientationButtonWidthPropertyName = "PipsPagerVerticalOrientationButtonWidth";
        private const string c_pipsPagerVerticalOrientationButtonHeightPropertyName = "PipsPagerVerticalOrientationButtonHeight";

        private const string c_pipsPagerHorizontalOrientationButtonWidthPropertyName = "PipsPagerHorizontalOrientationButtonWidth";
        private const string c_pipsPagerHorizontalOrientationButtonHeightPropertyName = "PipsPagerHorizontalOrientationButtonHeight";

        private const string c_pipsPagerHorizontalOrientationVisualState = "HorizontalOrientationView";
        private const string c_pipsPagerVerticalOrientationVisualState = "VerticalOrientationView";

        private const string c_pipsPagerButtonVerticalOrientationVisualState = "VerticalOrientation";
        private const string c_pipsPagerButtonHorizontalOrientationVisualState = "HorizontalOrientation";

        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(PipsPager));

        static PipsPager()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PipsPager), new FrameworkPropertyMetadata(typeof(PipsPager)));
        }

        public PipsPager()
        {
            m_pipsPagerItems = new ObservableCollection<int>();
            var templateSettings = new PipsPagerTemplateSettings();
            templateSettings.SetValue(PipsPagerTemplateSettings.PipsPagerItemsPropertyKey, m_pipsPagerItems);
            SetValue(TemplateSettingsPropertyKey, templateSettings);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new PipsPagerAutomationPeer(this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            AutomationProperties.SetName(this, ResourceAccessor.GetLocalizedStringResource(SR_PipsPagerNameText));

            {
                if (m_previousPageButton != null) { m_previousPageButton.Click -= OnPreviousButtonClicked; }
                Button button = GetTemplateChildT<Button>(c_previousPageButtonName, this);
                m_previousPageButton = button;
                if (button != null)
                {
                    AutomationProperties.SetName(button, ResourceAccessor.GetLocalizedStringResource(SR_PipsPagerPreviousPageButtonText));
                    button.Click += OnPreviousButtonClicked;
                }
            }

            {
                if (m_nextPageButton != null) { m_nextPageButton.Click -= OnNextButtonClicked; }
                Button button = GetTemplateChildT<Button>(c_nextPageButtonName, this);
                m_nextPageButton = button;
                if (button != null)
                {
                    AutomationProperties.SetName(button, ResourceAccessor.GetLocalizedStringResource(SR_PipsPagerNextPageButtonText));
                    button.Click += OnNextButtonClicked;
                }
            }

            {
                if (m_pipsPagerRepeater != null)
                {
                    m_pipsPagerRepeater.ElementPrepared -= OnElementPrepared;
                    m_pipsPagerRepeater.GotFocus -= OnPipsAreaGettingFocus;
                    m_pipsPagerRepeater.RequestBringIntoView -= OnPipsAreaBringIntoViewRequested;
                }
                ItemsRepeater repeater = GetTemplateChildT<ItemsRepeater>(c_pipsPagerRepeaterName, this);
                m_pipsPagerRepeater = repeater;
                if (repeater != null)
                {
                    repeater.ElementPrepared += OnElementPrepared;
                    repeater.GotFocus += OnPipsAreaGettingFocus;
                    repeater.RequestBringIntoView += OnPipsAreaBringIntoViewRequested;
                }
            }

            {
                if (m_pipsPagerScrollViewer != null)
                {
                    m_pipsPagerScrollViewer.RequestBringIntoView -= OnScrollViewerBringIntoViewRequested;
                }
                ScrollViewerEx scrollViewer = GetTemplateChildT<ScrollViewerEx>(c_pipsPagerScrollViewerName, this);
                m_pipsPagerScrollViewer = scrollViewer;
                if (scrollViewer != null)
                {
                    scrollViewer.RequestBringIntoView += OnScrollViewerBringIntoViewRequested;
                }
            }

            m_defaultPipSize = GetDesiredPipSize(NormalPipStyle);
            m_selectedPipSize = GetDesiredPipSize(SelectedPipStyle);
            OnNavigationButtonVisibilityChanged(PreviousButtonVisibility, c_previousPageButtonCollapsedVisualState, c_previousPageButtonDisabledVisualState);
            OnNavigationButtonVisibilityChanged(NextButtonVisibility, c_nextPageButtonCollapsedVisualState, c_nextPageButtonDisabledVisualState);
            UpdatePipsItems(NumberOfPages, MaxVisiblePips);
            OnOrientationChanged();
            OnSelectedPageIndexChanged(m_lastSelectedPageIndex);
        }

        void RaiseSelectedIndexChanged()
        {
            var args = new PipsPagerSelectedIndexChangedEventArgs();
            SelectedIndexChanged?.Invoke(this, args);
        }

        Size GetDesiredPipSize(Style style)
        {
            var repeater = m_pipsPagerRepeater;
            if (repeater != null)
            {
                if (repeater.ItemTemplate is DataTemplate itemTemplate)
                {
                    if (itemTemplate.LoadContent() is FrameworkElement element)
                    {
                        ApplyStyleToPipAndUpdateOrientation(element, style);
                        element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        return element.DesiredSize;
                    }
                }
            }

            return new Size(0.0, 0.0);
        }

        void OnKeyDown(object sender, KeyEventArgs args)
        {
            FocusNavigationDirection previousPipDirection = new FocusNavigationDirection();
            FocusNavigationDirection nextPipDirection = new FocusNavigationDirection();
            if (Orientation == Orientation.Vertical)
            {
                previousPipDirection = FocusNavigationDirection.Up;
                nextPipDirection = FocusNavigationDirection.Down;
            }
            else
            {
                previousPipDirection = FocusNavigationDirection.Left;
                nextPipDirection = FocusNavigationDirection.Right;
            }

            if (args.Key == Key.Left || args.Key == Key.Up)
            {
                //FocusManager.TryMoveFocus(previousPipDirection);
                args.Handled = true;
            }
            else if (args.Key == Key.Right || args.Key == Key.Down)
            {
                //FocusManager.TryMoveFocus(nextPipDirection);
                args.Handled = true;
            }
            // Call for all other presses
            base.OnKeyDown(args);
        }

        void UpdateIndividualNavigationButtonVisualState(bool hiddenOnEdgeCondition, ButtonVisibility visibility, string visibleStateName, string hiddenStateName, string enabledStateName, string disabledStateName)
        {
            var ifGenerallyVisible = !hiddenOnEdgeCondition && NumberOfPages != 0 && MaxVisiblePips > 0;
            if (visibility != ButtonVisibility.Collapsed)
            {
                if ((visibility == ButtonVisibility.Visible || m_isPointerOver || m_isFocused) && ifGenerallyVisible)
                {
                    VisualStateManager.GoToState(this, visibleStateName, false);
                    VisualStateManager.GoToState(this, enabledStateName, false);
                }
                else
                {
                    if (!ifGenerallyVisible)
                    {
                        VisualStateManager.GoToState(this, disabledStateName, false);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, enabledStateName, false);
                    }
                    VisualStateManager.GoToState(this, hiddenStateName, false);
                }
            }
        }

        void UpdateNavigationButtonVisualStates()
        {
            int selectedPageIndex = SelectedPageIndex;
            int numberOfPages = NumberOfPages;

            var ifPreviousButtonHiddenOnEdge = selectedPageIndex == 0;
            UpdateIndividualNavigationButtonVisualState(ifPreviousButtonHiddenOnEdge, PreviousButtonVisibility, c_previousPageButtonVisibleVisualState, c_previousPageButtonHiddenVisualState, c_previousPageButtonEnabledVisualState, c_previousPageButtonDisabledVisualState);

            var ifNextButtonHiddenOnEdge = selectedPageIndex == numberOfPages - 1;
            UpdateIndividualNavigationButtonVisualState(ifNextButtonHiddenOnEdge, NextButtonVisibility, c_nextPageButtonVisibleVisualState, c_nextPageButtonHiddenVisualState, c_nextPageButtonEnabledVisualState, c_nextPageButtonDisabledVisualState);
        }

        void ScrollToCenterOfViewport(UIElement sender, int index)
        {
            /* Vertical and Horizontal AligmentsRatio are not available until Win Version 1803 (sdk version 17134) */
            //if (SharedHelpers.IsBringIntoViewOptionsVerticalAlignmentRatioAvailable())
            //{
            //    BringIntoViewOptions options = new BringIntoViewOptions();
            //    if (Orientation() == winrt.Orientation.Horizontal)
            //    {
            //        options.HorizontalAlignmentRatio(0.5);
            //    }
            //    else
            //    {
            //        options.VerticalAlignmentRatio(0.5);
            //    }
            //    options.AnimationDesired(true);
            //    sender.StartBringIntoView(options);
            //}
            var scrollViewer = m_pipsPagerScrollViewer;
            if (scrollViewer != null)
            {
                double pipSize;
                Action<double> changeViewFunc;
                if (Orientation == Orientation.Horizontal)
                {
                    pipSize = m_defaultPipSize.Width;
                    changeViewFunc = (double offset) =>
                    {
                        scrollViewer.ChangeView(offset, null, null, true);
                    };
                }
                else
                {
                    pipSize = m_defaultPipSize.Height;
                    changeViewFunc = (double offset) =>
                    {
                        scrollViewer.ChangeView(null, offset, null, true);
                    };
                }
                int maxVisualIndicators = MaxVisiblePips;
                /* This line makes sure that while having even # of indicators the scrolling will be done correctly */
                int offSetChangeForEvenSizeWindow = maxVisualIndicators % 2 == 0 && index > m_lastSelectedPageIndex ? 1 : 0;
                int offSetNumOfElements = index + offSetChangeForEvenSizeWindow - maxVisualIndicators / 2;
                double offset = Math.Max(0.0, offSetNumOfElements * pipSize);
                changeViewFunc(offset);
            }
        }

        void UpdateSelectedPip(int index)
        {
            if (NumberOfPages != 0 && MaxVisiblePips > 0)
            {
                var repeater = m_pipsPagerRepeater;
                if (repeater != null)
                {
                    repeater.UpdateLayout();
                    {
                        if (repeater.TryGetElement(m_lastSelectedPageIndex) is FrameworkElement pip)
                        {
                            ApplyStyleToPipAndUpdateOrientation(pip, NormalPipStyle);
                        }
                    }
                    {
                        if (repeater.GetOrCreateElement(index) is FrameworkElement pip)
                        {
                            ApplyStyleToPipAndUpdateOrientation(pip, SelectedPipStyle);
                            ScrollToCenterOfViewport(pip, index);
                        }
                    }
                }
            }
        }

        double CalculateScrollViewerSize(double defaultPipSize, double selectedPipSize, int numberOfPages, int maxVisualIndicators)
        {

            var numberOfPagesToDisplay = 0;
            maxVisualIndicators = Math.Max(0, maxVisualIndicators);
            if (maxVisualIndicators == 0 || numberOfPages == 0)
            {
                return 0;
            }
            else if (numberOfPages > 0)
            {
                numberOfPagesToDisplay = Math.Min(maxVisualIndicators, numberOfPages);
            }
            else
            {
                numberOfPagesToDisplay = maxVisualIndicators;
            }
            return defaultPipSize * (numberOfPagesToDisplay - 1) + selectedPipSize;
        }

        void SetScrollViewerMaxSize()
        {
            var scrollViewer = m_pipsPagerScrollViewer;
            if (scrollViewer != null)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    var scrollViewerWidth = CalculateScrollViewerSize(m_defaultPipSize.Width, m_selectedPipSize.Width, NumberOfPages, MaxVisiblePips);
                    scrollViewer.MaxWidth = scrollViewerWidth;
                    scrollViewer.MaxHeight = Math.Max(m_defaultPipSize.Height, m_selectedPipSize.Height);
                }
                else
                {
                    var scrollViewerHeight = CalculateScrollViewerSize(m_defaultPipSize.Height, m_selectedPipSize.Height, NumberOfPages, MaxVisiblePips);
                    scrollViewer.MaxHeight = scrollViewerHeight;
                    scrollViewer.MaxWidth = Math.Max(m_defaultPipSize.Width, m_selectedPipSize.Width);
                }
            }
        }

        void UpdatePipsItems(int numberOfPages, int maxVisualIndicators)
        {
            var pipsListSize = m_pipsPagerItems.Count;

            if (numberOfPages == 0 || maxVisualIndicators == 0)
            {
                m_pipsPagerItems.Clear();
            }
            /* Inifinite number of pages case */
            else if (numberOfPages < 0)
            {
                /* Treat negative max visual indicators as 0*/
                var minNumberOfElements = Math.Max(SelectedPageIndex + 1, Math.Max(0, maxVisualIndicators));
                if (minNumberOfElements > pipsListSize)
                {
                    for (int i = pipsListSize; i < minNumberOfElements; i++)
                    {
                        m_pipsPagerItems.Add(i + 1);
                    }
                }
                else if (SelectedPageIndex == pipsListSize - 1)
                {
                    m_pipsPagerItems.Add(pipsListSize + 1);
                }
            }
            else if (pipsListSize < numberOfPages)
            {
                for (int i = pipsListSize; i < numberOfPages; i++)
                {
                    m_pipsPagerItems.Add(i + 1);
                }
            }
            else
            {
                for (int i = numberOfPages; i < pipsListSize; i++)
                {
                    m_pipsPagerItems.RemoveAt(m_pipsPagerItems.Count - 1);
                }
            }
        }

        void OnElementPrepared(object sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is FrameworkElement element)
            {
                var index = args.Index;
                var style = index == SelectedPageIndex ? SelectedPipStyle : NormalPipStyle;
                ApplyStyleToPipAndUpdateOrientation(element, style);

                AutomationProperties.SetName(element, ResourceAccessor.GetLocalizedStringResource(SR_PipsPagerPageText) + " " + (index + 1).ToString());
                //AutomationProperties.SetPositionInSet(element, index + 1);
                //AutomationProperties.SetSizeOfSet(element, NumberOfPages);

                if (element is ButtonBase pip)
                {
                    pip.Click += (sender, args) =>
                    {
                        var repeater = m_pipsPagerRepeater;
                        if (repeater != null)
                        {
                            if (sender is Button button)
                            {
                                SelectedPageIndex = repeater.GetElementIndex(button);
                            }
                        }
                    };
                }
            }
        }

        public void OnElementIndexChanged(ItemsRepeater UnnamedParameter, ItemsRepeaterElementIndexChangedEventArgs args)
        {
            var pip = args.Element;
            if (pip != null)
            {
                var newIndex = args.NewIndex;
                AutomationProperties.SetName(pip, ResourceAccessor.GetLocalizedStringResource(SR_PipsPagerPageText) + " " + (newIndex + 1).ToString());
                //AutomationProperties.SetPositionInSet(pip, newIndex + 1);
            }
        }

        void OnMaxVisiblePipsChanged()
        {
            var numberOfPages = NumberOfPages;
            if (numberOfPages < 0)
            {
                UpdatePipsItems(numberOfPages, MaxVisiblePips);
            }
            SetScrollViewerMaxSize();
            UpdateSelectedPip(SelectedPageIndex);
            UpdateNavigationButtonVisualStates();
        }

        void OnNumberOfPagesChanged()
        {
            int numberOfPages = NumberOfPages;
            int selectedPageIndex = SelectedPageIndex;
            UpdateSizeOfSetForElements(numberOfPages, m_pipsPagerItems.Count);
            UpdatePipsItems(numberOfPages, MaxVisiblePips);
            SetScrollViewerMaxSize();
            if (SelectedPageIndex > numberOfPages - 1 && numberOfPages > -1)
            {
                SelectedPageIndex = numberOfPages - 1;
            }
            else
            {
                UpdateSelectedPip(selectedPageIndex);
                UpdateNavigationButtonVisualStates();
            }
        }

        void OnSelectedPageIndexChanged(int oldValue)
        {
            // If we don't have any pages, there is nothing we should do.
            // Ensure that SelectedPageIndex will end up in the valid range of values
            // Special case is NumberOfPages being 0, in that case, don't verify upperbound restrictions
            if (SelectedPageIndex > NumberOfPages - 1 && NumberOfPages > 0)
            {
                SelectedPageIndex = NumberOfPages - 1;
            }
            else if (SelectedPageIndex < 0)
            {
                SelectedPageIndex = 0;
            }
            else
            {
                // Now handle the value changes
                m_lastSelectedPageIndex = oldValue;

                // Fire value property change for UIA
                if (FrameworkElementAutomationPeer.FromElement(this) is PipsPagerAutomationPeer peer)
                {
                    peer.RaiseSelectionChanged(m_lastSelectedPageIndex, SelectedPageIndex);
                }
                if (NumberOfPages < 0)
                {
                    UpdatePipsItems(NumberOfPages, MaxVisiblePips);
                }
                UpdateSelectedPip(SelectedPageIndex);
                UpdateNavigationButtonVisualStates();
                RaiseSelectedIndexChanged();
            }
        }

        void OnOrientationChanged()
        {
            if (Orientation == Orientation.Horizontal)
            {
                VisualStateManager.GoToState(this, c_pipsPagerHorizontalOrientationVisualState, false);
            }
            else
            {
                VisualStateManager.GoToState(this, c_pipsPagerVerticalOrientationVisualState, false);
            }
            var repeater = m_pipsPagerRepeater;
            if (repeater != null)
            {
                var itemsSourceView = repeater.ItemsSourceView;
                if (itemsSourceView != null)
                {
                    var itemCount = itemsSourceView.Count;
                    for (int i = 0; i < itemCount; i++)
                    {
                        if (repeater.TryGetElement(i) is Control pip)
                        {
                            UpdatePipOrientation(pip);
                        }
                    }
                }
            }
            m_defaultPipSize = GetDesiredPipSize(NormalPipStyle);
            m_selectedPipSize = GetDesiredPipSize(SelectedPipStyle);
            SetScrollViewerMaxSize();
            var selectedPip = GetSelectedItem();
            if (selectedPip != null)
            {
                ScrollToCenterOfViewport(selectedPip, SelectedPageIndex);
            }
        }

        void ApplyStyleToPipAndUpdateOrientation(FrameworkElement pip, Style style)
        {
            pip.Style = style;
            if (pip is Control control)
            {
                control.ApplyTemplate();
                UpdatePipOrientation(control);
            }
        }

        void UpdatePipOrientation(Control pip)
        {
            if (Orientation == Orientation.Horizontal)
            {
                VisualStateManager.GoToState(pip, c_pipsPagerButtonHorizontalOrientationVisualState, false);
            }
            else
            {
                VisualStateManager.GoToState(pip, c_pipsPagerButtonVerticalOrientationVisualState, false);
            }
        }

        void OnNavigationButtonVisibilityChanged(ButtonVisibility visibility, string collapsedStateName, string disabledStateName)
        {
            if (visibility == ButtonVisibility.Collapsed)
            {
                VisualStateManager.GoToState(this, collapsedStateName, false);
                VisualStateManager.GoToState(this, disabledStateName, false);
            }
            else
            {
                UpdateNavigationButtonVisualStates();
            }
        }

        void OnPreviousButtonClicked(object sender, RoutedEventArgs e)
        {
            // In this method, SelectedPageIndex is always greater than 0.
            SelectedPageIndex = SelectedPageIndex - 1;
        }

        void OnNextButtonClicked(object sender, RoutedEventArgs e)
        {
            // In this method, SelectedPageIndex is always less than maximum.
            SelectedPageIndex = SelectedPageIndex + 1;
        }

        void OnGotFocus(RoutedEventArgs args)
        {
            if (args.OriginalSource is Button btn)
            {
                // If the element inside the Pager is already keyboard focused
                // and the user will use the mouse to focus on something else
                // the LostFocus will not be triggered on keyboard focused element
                // while GotFocus will be triggered on the new mouse focused element.
                // We account for this scenario and update m_isFocused in case
                // user will use mouse while being in keyboard focus.
                if (btn.IsFocused)
                {
                    m_isFocused = true;
                    UpdateNavigationButtonVisualStates();
                }
                else
                {
                    m_isFocused = false;
                }
            }
        }

        void OnPipsAreaGettingFocus(object sender, RoutedEventArgs args)
        {
            var repeater = m_pipsPagerRepeater;
            //if (repeater != null)
            //{
            //    // Easiest way to check if focus change came from within:
            //    // Check if element is child of repeater by getting index and checking for -1
            //    // If it is -1, focus came from outside and we want to get to selected element.
            //    if (args.OldFocusedElement is UIElement oldFocusedElement)
            //    {
            //        if (repeater.GetElementIndex(oldFocusedElement) == -1)
            //        {
            //            if (repeater.GetOrCreateElement(SelectedPageIndex) is UIElement realizedElement)
            //            {
            //                if (args is IGettingFocusEventArgs2 argsAsIGettingFocusEventArgs2)
            //                {
            //                    if (argsAsIGettingFocusEventArgs2.TrySetNewFocusedElement(realizedElement))
            //                    {
            //                        args.Handled(true);
            //                    }
            //                }
            //                else
            //                {
            //                    DispatcherHelper.RunOnUIThread(Dispatcher, () =>
            //                    {
            //                        realizedElement.Focus();
            //                    });
            args.Handled = true;
            //                }
            //            }
            //        }
            //    }
            //}
        }

        protected override void OnLostFocus(RoutedEventArgs args)
        {
            m_isFocused = false;
            UpdateNavigationButtonVisualStates();
        }

        protected override void OnMouseEnter(MouseEventArgs args)
        {
            base.OnMouseEnter(args);
            m_isPointerOver = true;
            UpdateNavigationButtonVisualStates();
        }

        protected override void OnMouseLeave(MouseEventArgs args)
        {
            // We can get a spurious Exited and then Entered if the button
            // that is being clicked on hides itself. In order to avoid switching
            // visual states in this case, we check if the pointer is over the
            // control bounds when we get the exited event.
            if (IsOutOfControlBounds(args.GetPosition(this)))
            {
                m_isPointerOver = false;
                UpdateNavigationButtonVisualStates();
            }
            else
            {
                args.Handled = true;
            }
            base.OnMouseLeave(args);
        }
        
        bool IsOutOfControlBounds(Point point)
        {
            // This is a conservative check. It is okay to say we are
            // out of the bounds when close to the edge to account for rounding.
            var tolerance = 1.0;
            var actualWidth = ActualWidth;
            var actualHeight = ActualHeight;
            return point.X < tolerance || point.X > actualWidth - tolerance || point.Y < tolerance || point.Y > actualHeight - tolerance;
        }

        void OnPipsAreaBringIntoViewRequested(object sender, RequestBringIntoViewEventArgs args)
        {
            //if ((Orientation == Orientation.Vertical && Math.IsNaN(args.VerticalAlignmentRatio)) || (Orientation == Orientation.Horizontal && Math.IsNaN(args.HorizontalAlignmentRatio)))
            //{
                args.Handled = true;
            //}
        }

        void OnScrollViewerBringIntoViewRequested(object sender, RequestBringIntoViewEventArgs args)
        {
            args.Handled = true;
        }

        void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            DependencyProperty property = args.Property;
            if (this.Template != null)
            {
                if (property == NumberOfPagesProperty)
                {
                    OnNumberOfPagesChanged();
                }
                else if (property == SelectedPageIndexProperty)
                {
                    OnSelectedPageIndexChanged((int)args.OldValue);
                }
                else if (property == MaxVisiblePipsProperty)
                {
                    OnMaxVisiblePipsChanged();
                }
                else if (property == PreviousButtonVisibilityProperty)
                {
                    OnNavigationButtonVisibilityChanged(PreviousButtonVisibility, c_previousPageButtonCollapsedVisualState, c_previousPageButtonDisabledVisualState);
                }
                else if (property == NextButtonVisibilityProperty)
                {
                    OnNavigationButtonVisibilityChanged(NextButtonVisibility, c_nextPageButtonCollapsedVisualState, c_nextPageButtonDisabledVisualState);
                }
                else if (property == NormalPipStyleProperty)
                {
                    m_defaultPipSize = GetDesiredPipSize(NormalPipStyle);
                    SetScrollViewerMaxSize();
                    UpdateSelectedPip(SelectedPageIndex);
                }
                else if (property == SelectedPipStyleProperty)
                {
                    m_selectedPipSize = GetDesiredPipSize(SelectedPipStyle);
                    SetScrollViewerMaxSize();
                    UpdateSelectedPip(SelectedPageIndex);
                }
                else if (property == OrientationProperty)
                {
                    OnOrientationChanged();
                }
            }
        }

        UIElement GetSelectedItem()
        {
            var repeater = m_pipsPagerRepeater;
            if (repeater != null)
            {
                return repeater.TryGetElement(SelectedPageIndex);
            }
            return null;
        }

        void UpdateSizeOfSetForElements(int numberOfPages, int numberOfItems)
        {
            var repeater = m_pipsPagerRepeater;
            if (repeater != null)
            {
                for (int i = 0; i < numberOfItems; i++)
                {
                    var pip = repeater.TryGetElement(i);
                    if (pip != null)
                    {
                        //AutomationProperties.SetSizeOfSet(pip, numberOfPages);
                    }
                }
            }
        }

        DependencyObject IControlProtected.GetTemplateChild(string childName)
        {
            return GetTemplateChild(childName);
        }

        /* Refs */
        ItemsRepeater m_pipsPagerRepeater;
        ScrollViewerEx m_pipsPagerScrollViewer;
        Button m_previousPageButton;
        Button m_nextPageButton;

        /* Items */
        ObservableCollection<int> m_pipsPagerItems;

        /* Additional variables class variables*/
        Size m_defaultPipSize = new Size(0.0, 0.0);
        Size m_selectedPipSize = new Size(0.0, 0.0);
        int m_lastSelectedPageIndex = -1;
        bool m_isPointerOver = false;
        bool m_isFocused = false;
    }
}
