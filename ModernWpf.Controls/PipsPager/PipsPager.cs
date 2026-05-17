using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = RootPanelName, Type = typeof(StackPanel))]
    [TemplatePart(Name = PreviousButtonName, Type = typeof(Button))]
    [TemplatePart(Name = PipsPagerRepeaterName, Type = typeof(ItemsRepeater))]
    [TemplatePart(Name = PipsPagerScrollViewerName, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = NextButtonName, Type = typeof(Button))]
    public class PipsPager : Control
    {
        private const string RootPanelName = "RootPanel";
        private const string PreviousButtonName = "PreviousPageButton";
        private const string NextButtonName = "NextPageButton";
        private const string PipsPagerRepeaterName = "PipsPagerItemsRepeater";
        private const string PipsPagerScrollViewerName = "PipsPagerScrollViewer";
        private const string PreviousPageButtonVisibleState = "PreviousPageButtonVisible";
        private const string PreviousPageButtonHiddenState = "PreviousPageButtonHidden";
        private const string PreviousPageButtonCollapsedState = "PreviousPageButtonCollapsed";
        private const string PreviousPageButtonEnabledState = "PreviousPageButtonEnabled";
        private const string PreviousPageButtonDisabledState = "PreviousPageButtonDisabled";
        private const string NextPageButtonVisibleState = "NextPageButtonVisible";
        private const string NextPageButtonHiddenState = "NextPageButtonHidden";
        private const string NextPageButtonCollapsedState = "NextPageButtonCollapsed";
        private const string NextPageButtonEnabledState = "NextPageButtonEnabled";
        private const string NextPageButtonDisabledState = "NextPageButtonDisabled";
        private const string HorizontalOrientationViewState = "HorizontalOrientationView";
        private const string VerticalOrientationViewState = "VerticalOrientationView";
        private const string HorizontalPipOrientationState = "HorizontalOrientation";
        private const string VerticalPipOrientationState = "VerticalOrientation";

        static PipsPager()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PipsPager), new FrameworkPropertyMetadata(typeof(PipsPager)));
        }

        public PipsPager()
        {
            SetValue(TemplateSettingsPropertyKey, new PipsPagerTemplateSettings());
        }

        public static readonly DependencyProperty NumberOfPagesProperty =
            DependencyProperty.Register(
                nameof(NumberOfPages),
                typeof(int),
                typeof(PipsPager),
                new FrameworkPropertyMetadata(-1, OnPagerPropertyChanged, CoerceNumberOfPages));

        public int NumberOfPages
        {
            get => (int)GetValue(NumberOfPagesProperty);
            set => SetValue(NumberOfPagesProperty, value);
        }

        public static readonly DependencyProperty SelectedPageIndexProperty =
            DependencyProperty.Register(
                nameof(SelectedPageIndex),
                typeof(int),
                typeof(PipsPager),
                new FrameworkPropertyMetadata(0, OnSelectedPageIndexPropertyChanged, CoerceSelectedPageIndex));

        public int SelectedPageIndex
        {
            get => (int)GetValue(SelectedPageIndexProperty);
            set => SetValue(SelectedPageIndexProperty, value);
        }

        public static readonly DependencyProperty MaxVisiblePipsProperty =
            DependencyProperty.Register(
                nameof(MaxVisiblePips),
                typeof(int),
                typeof(PipsPager),
                new FrameworkPropertyMetadata(5, OnPagerPropertyChanged, CoerceNonNegativeInt));

        public int MaxVisiblePips
        {
            get => (int)GetValue(MaxVisiblePipsProperty);
            set => SetValue(MaxVisiblePipsProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(PipsPager),
                new FrameworkPropertyMetadata(Orientation.Horizontal, OnPagerPropertyChanged));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty PreviousButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(PreviousButtonVisibility),
                typeof(PipsPagerButtonVisibility),
                typeof(PipsPager),
                new FrameworkPropertyMetadata(PipsPagerButtonVisibility.Collapsed, OnPagerPropertyChanged));

        public PipsPagerButtonVisibility PreviousButtonVisibility
        {
            get => (PipsPagerButtonVisibility)GetValue(PreviousButtonVisibilityProperty);
            set => SetValue(PreviousButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty NextButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(NextButtonVisibility),
                typeof(PipsPagerButtonVisibility),
                typeof(PipsPager),
                new FrameworkPropertyMetadata(PipsPagerButtonVisibility.Collapsed, OnPagerPropertyChanged));

        public PipsPagerButtonVisibility NextButtonVisibility
        {
            get => (PipsPagerButtonVisibility)GetValue(NextButtonVisibilityProperty);
            set => SetValue(NextButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty PreviousButtonStyleProperty =
            DependencyProperty.Register(
                nameof(PreviousButtonStyle),
                typeof(Style),
                typeof(PipsPager),
                new FrameworkPropertyMetadata(null, OnPagerPropertyChanged));

        public Style PreviousButtonStyle
        {
            get => (Style)GetValue(PreviousButtonStyleProperty);
            set => SetValue(PreviousButtonStyleProperty, value);
        }

        public static readonly DependencyProperty NextButtonStyleProperty =
            DependencyProperty.Register(
                nameof(NextButtonStyle),
                typeof(Style),
                typeof(PipsPager),
                new FrameworkPropertyMetadata(null, OnPagerPropertyChanged));

        public Style NextButtonStyle
        {
            get => (Style)GetValue(NextButtonStyleProperty);
            set => SetValue(NextButtonStyleProperty, value);
        }

        public static readonly DependencyProperty SelectedPipStyleProperty =
            DependencyProperty.Register(
                nameof(SelectedPipStyle),
                typeof(Style),
                typeof(PipsPager),
                new FrameworkPropertyMetadata(null, OnPagerPropertyChanged));

        public Style SelectedPipStyle
        {
            get => (Style)GetValue(SelectedPipStyleProperty);
            set => SetValue(SelectedPipStyleProperty, value);
        }

        public static readonly DependencyProperty NormalPipStyleProperty =
            DependencyProperty.Register(
                nameof(NormalPipStyle),
                typeof(Style),
                typeof(PipsPager),
                new FrameworkPropertyMetadata(null, OnPagerPropertyChanged));

        public Style NormalPipStyle
        {
            get => (Style)GetValue(NormalPipStyleProperty);
            set => SetValue(NormalPipStyleProperty, value);
        }

        public static readonly DependencyProperty WrapModeProperty =
            DependencyProperty.Register(
                nameof(WrapMode),
                typeof(PipsPagerWrapMode),
                typeof(PipsPager),
                new FrameworkPropertyMetadata(PipsPagerWrapMode.None, OnPagerPropertyChanged));

        public PipsPagerWrapMode WrapMode
        {
            get => (PipsPagerWrapMode)GetValue(WrapModeProperty);
            set => SetValue(WrapModeProperty, value);
        }

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(PipsPagerTemplateSettings),
                typeof(PipsPager),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public PipsPagerTemplateSettings TemplateSettings => (PipsPagerTemplateSettings)GetValue(TemplateSettingsProperty);

        public event TypedEventHandler<PipsPager, PipsPagerSelectedIndexChangedEventArgs> SelectedIndexChanged;

        public override void OnApplyTemplate()
        {
            AutomationProperties.SetName(this, Strings.PipsPagerNameText);

            if (_previousButton != null)
            {
                _previousButton.Click -= OnPreviousButtonClick;
            }

            if (_nextButton != null)
            {
                _nextButton.Click -= OnNextButtonClick;
            }

            if (_pipsPagerRepeater != null)
            {
                _pipsPagerRepeater.ElementPrepared -= OnElementPrepared;
                _pipsPagerRepeater.ElementIndexChanged -= OnElementIndexChanged;
                DependencyPropertyDescriptor.FromProperty(ItemsRepeater.LayoutProperty, typeof(ItemsRepeater))
                    ?.RemoveValueChanged(_pipsPagerRepeater, OnItemsRepeaterLayoutChanged);
            }

            RestoreLayoutVirtualization();

            base.OnApplyTemplate();

            _rootPanel = GetTemplateChild(RootPanelName) as StackPanel;
            _previousButton = GetTemplateChild(PreviousButtonName) as Button;
            _nextButton = GetTemplateChild(NextButtonName) as Button;
            _pipsPagerRepeater = GetTemplateChild(PipsPagerRepeaterName) as ItemsRepeater;
            _pipsPagerScrollViewer = GetTemplateChild(PipsPagerScrollViewerName) as ScrollViewer;

            if (_previousButton != null)
            {
                AutomationProperties.SetName(_previousButton, Strings.PipsPagerPreviousPageButtonText);
                _previousButton.Click += OnPreviousButtonClick;
            }

            if (_nextButton != null)
            {
                AutomationProperties.SetName(_nextButton, Strings.PipsPagerNextPageButtonText);
                _nextButton.Click += OnNextButtonClick;
            }

            if (_pipsPagerRepeater != null)
            {
                _pipsPagerRepeater.ElementPrepared += OnElementPrepared;
                _pipsPagerRepeater.ElementIndexChanged += OnElementIndexChanged;
                DependencyPropertyDescriptor.FromProperty(ItemsRepeater.LayoutProperty, typeof(ItemsRepeater))
                    ?.AddValueChanged(_pipsPagerRepeater, OnItemsRepeaterLayoutChanged);
                UpdateLayoutVirtualization();
            }

            _templateApplied = true;
            _defaultPipSize = GetDesiredPipSize(NormalPipStyle);
            _selectedPipSize = GetDesiredPipSize(SelectedPipStyle);

            OnNavigationButtonVisibilityChanged(PreviousButtonVisibility, PreviousPageButtonCollapsedState, PreviousPageButtonDisabledState);
            OnNavigationButtonVisibilityChanged(NextButtonVisibility, NextPageButtonCollapsedState, NextPageButtonDisabledState);
            UpdatePipsItems(NumberOfPages, MaxVisiblePips);
            OnOrientationChanged();
            OnSelectedPageIndexChanged(_lastSelectedPageIndex);
        }

        public Button ContainerFromIndex(int pageIndex)
        {
            if (pageIndex < 0 || (NumberOfPages >= 0 && pageIndex >= NumberOfPages))
            {
                return null;
            }

            if (_pipsPagerRepeater != null)
            {
                return (_pipsPagerRepeater.TryGetElement(pageIndex) ??
                        _pipsPagerRepeater.GetOrCreateElement(pageIndex)) as Button;
            }

            _pipButtonsByPageIndex.TryGetValue(pageIndex, out var button);
            return button;
        }

        internal Button GetSelectedButton()
        {
            return ContainerFromIndex(SelectedPageIndex);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new PipsPagerAutomationPeer(this);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            var request = default(TraversalRequest);

            if (e.Key == Key.Left || e.Key == Key.Up)
            {
                request = new TraversalRequest(
                    Orientation == System.Windows.Controls.Orientation.Vertical ? FocusNavigationDirection.Up : FocusNavigationDirection.Left);
            }
            else if (e.Key == Key.Right || e.Key == Key.Down)
            {
                request = new TraversalRequest(
                    Orientation == System.Windows.Controls.Orientation.Vertical ? FocusNavigationDirection.Down : FocusNavigationDirection.Right);
            }

            if (request != null)
            {
                MoveFocus(request);
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            _isPointerOver = true;
            UpdateNavigationButtonVisualStates();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _isPointerOver = false;
            UpdateNavigationButtonVisualStates();
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            _isFocused = true;
            UpdateNavigationButtonVisualStates();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            _isFocused = false;
            UpdateNavigationButtonVisualStates();
        }

        private static void OnPagerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pager = (PipsPager)d;
            pager.HandlePropertyChanged(e);
        }

        private static void OnSelectedPageIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pager = (PipsPager)d;
            pager.HandlePropertyChanged(e);
        }

        private static object CoerceNumberOfPages(DependencyObject d, object baseValue)
        {
            return Math.Max(-1, (int)baseValue);
        }

        private static object CoerceNonNegativeInt(DependencyObject d, object baseValue)
        {
            return Math.Max(0, (int)baseValue);
        }

        private static object CoerceSelectedPageIndex(DependencyObject d, object baseValue)
        {
            var pager = (PipsPager)d;
            var index = Math.Max(0, (int)baseValue);

            if (pager.NumberOfPages > 0)
            {
                index = Math.Min(index, pager.NumberOfPages - 1);
            }

            return index;
        }

        private void OnPreviousButtonClick(object sender, RoutedEventArgs e)
        {
            if (NumberOfPages == 0 || NumberOfPages == 1)
            {
                return;
            }

            var newPageIndex = Math.Max(0, SelectedPageIndex - 1);

            if (IsWrapEnabled && NumberOfPages > -1 && SelectedPageIndex == 0)
            {
                newPageIndex = NumberOfPages - 1;
            }

            SelectedPageIndex = newPageIndex;
        }

        private void OnNextButtonClick(object sender, RoutedEventArgs e)
        {
            if (NumberOfPages == 0 || NumberOfPages == 1)
            {
                return;
            }

            var newPageIndex = NumberOfPages > -1
                ? Math.Min(SelectedPageIndex + 1, NumberOfPages - 1)
                : SelectedPageIndex + 1;

            if (IsWrapEnabled && SelectedPageIndex == NumberOfPages - 1)
            {
                newPageIndex = 0;
            }

            SelectedPageIndex = newPageIndex;
        }

        private void OnPipButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                var pageIndex = _pipsPagerRepeater?.GetElementIndex(element) ?? -1;
                if (pageIndex >= 0)
                {
                    SelectedPageIndex = pageIndex;
                }
            }
        }

        private void RaiseSelectedIndexChanged()
        {
            SelectedIndexChanged?.Invoke(this, new PipsPagerSelectedIndexChangedEventArgs());
        }

        private void HandlePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!_templateApplied)
            {
                if (args.Property == NumberOfPagesProperty)
                {
                    CoerceValue(SelectedPageIndexProperty);
                }

                return;
            }

            if (args.Property == NumberOfPagesProperty)
            {
                OnNumberOfPagesChanged();
            }
            else if (args.Property == SelectedPageIndexProperty)
            {
                OnSelectedPageIndexChanged((int)args.OldValue);
            }
            else if (args.Property == MaxVisiblePipsProperty)
            {
                OnMaxVisiblePipsChanged();
            }
            else if (args.Property == WrapModeProperty)
            {
                OnWrapModeChanged();
            }
            else if (args.Property == PreviousButtonVisibilityProperty)
            {
                OnNavigationButtonVisibilityChanged(PreviousButtonVisibility, PreviousPageButtonCollapsedState, PreviousPageButtonDisabledState);
            }
            else if (args.Property == NextButtonVisibilityProperty)
            {
                OnNavigationButtonVisibilityChanged(NextButtonVisibility, NextPageButtonCollapsedState, NextPageButtonDisabledState);
            }
            else if (args.Property == NormalPipStyleProperty)
            {
                _defaultPipSize = GetDesiredPipSize(NormalPipStyle);
                SetScrollViewerMaxSize();
                UpdateSelectedPip(SelectedPageIndex);
            }
            else if (args.Property == SelectedPipStyleProperty)
            {
                _selectedPipSize = GetDesiredPipSize(SelectedPipStyle);
                SetScrollViewerMaxSize();
                UpdateSelectedPip(SelectedPageIndex);
            }
            else if (args.Property == OrientationProperty)
            {
                OnOrientationChanged();
            }
        }

        private void OnMaxVisiblePipsChanged()
        {
            if (NumberOfPages < 0)
            {
                UpdatePipsItems(NumberOfPages, MaxVisiblePips);
            }

            SetScrollViewerMaxSize();
            UpdateSelectedPip(SelectedPageIndex);
            UpdateNavigationButtonVisualStates();
        }

        private void OnNumberOfPagesChanged()
        {
            var numberOfPages = NumberOfPages;
            var selectedPageIndex = SelectedPageIndex;
            UpdateSizeOfSetForElements(numberOfPages, TemplateSettings.PipsPagerItems.Count);
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

        private void OnSelectedPageIndexChanged(int oldValue)
        {
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
                _lastSelectedPageIndex = oldValue;

                if (FrameworkElementAutomationPeer.FromElement(this) is PipsPagerAutomationPeer peer)
                {
                    peer.RaiseSelectionChanged();
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

        private void OnWrapModeChanged()
        {
            UpdateLayoutVirtualization();
            UpdateNavigationButtonVisualStates();
        }

        private void OnOrientationChanged()
        {
            VisualStateManager.GoToState(
                this,
                Orientation == System.Windows.Controls.Orientation.Horizontal ? HorizontalOrientationViewState : VerticalOrientationViewState,
                false);

            if (_pipsPagerRepeater?.Layout is StackLayout stackLayout)
            {
                stackLayout.Orientation = Orientation;
            }

            if (_pipsPagerRepeater?.ItemsSourceView != null)
            {
                var itemCount = _pipsPagerRepeater.ItemsSourceView.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    if (_pipsPagerRepeater.TryGetElement(i) is Control pip)
                    {
                        UpdatePipOrientation(pip);
                    }
                }
            }

            _defaultPipSize = GetDesiredPipSize(NormalPipStyle);
            _selectedPipSize = GetDesiredPipSize(SelectedPipStyle);
            SetScrollViewerMaxSize();
            GetSelectedButton()?.BringIntoView();
        }

        private void UpdateNavigationButtonVisualStates()
        {
            UpdateIndividualNavigationButtonVisualState(
                SelectedPageIndex == 0,
                PreviousButtonVisibility,
                PreviousPageButtonVisibleState,
                PreviousPageButtonHiddenState,
                PreviousPageButtonEnabledState,
                PreviousPageButtonDisabledState);
            UpdateIndividualNavigationButtonVisualState(
                SelectedPageIndex == NumberOfPages - 1,
                NextButtonVisibility,
                NextPageButtonVisibleState,
                NextPageButtonHiddenState,
                NextPageButtonEnabledState,
                NextPageButtonDisabledState);
        }

        private void UpdateIndividualNavigationButtonVisualState(
            bool hiddenOnEdgeCondition,
            PipsPagerButtonVisibility buttonVisibility,
            string visibleStateName,
            string hiddenStateName,
            string enabledStateName,
            string disabledStateName)
        {
            var isGenerallyVisible = (!hiddenOnEdgeCondition || (IsWrapEnabled && NumberOfPages > 1)) &&
                NumberOfPages != 0 &&
                MaxVisiblePips > 0;

            if (buttonVisibility != PipsPagerButtonVisibility.Collapsed)
            {
                if ((buttonVisibility == PipsPagerButtonVisibility.Visible || _isPointerOver || _isFocused) && isGenerallyVisible)
                {
                    VisualStateManager.GoToState(this, visibleStateName, false);
                    VisualStateManager.GoToState(this, enabledStateName, false);
                }
                else
                {
                    VisualStateManager.GoToState(this, isGenerallyVisible ? enabledStateName : disabledStateName, false);
                    VisualStateManager.GoToState(this, hiddenStateName, false);
                }
            }
        }

        private void OnNavigationButtonVisibilityChanged(
            PipsPagerButtonVisibility visibility,
            string collapsedStateName,
            string disabledStateName)
        {
            if (visibility == PipsPagerButtonVisibility.Collapsed)
            {
                VisualStateManager.GoToState(this, collapsedStateName, false);
                VisualStateManager.GoToState(this, disabledStateName, false);
            }
            else
            {
                UpdateNavigationButtonVisualStates();
            }
        }

        private void UpdateSelectedPip(int index)
        {
            if (NumberOfPages == 0 || MaxVisiblePips <= 0 || _pipsPagerRepeater == null)
            {
                return;
            }

            _pipsPagerRepeater.UpdateLayout();

            if (_pipsPagerRepeater.TryGetElement(_lastSelectedPageIndex) is FrameworkElement oldPip)
            {
                ApplyStyleToPipAndUpdateOrientation(oldPip, NormalPipStyle);
            }

            if (_pipsPagerRepeater.GetOrCreateElement(index) is FrameworkElement selectedPip)
            {
                ApplyStyleToPipAndUpdateOrientation(selectedPip, SelectedPipStyle);
                selectedPip.BringIntoView();
            }
        }

        private void OnElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is FrameworkElement element)
            {
                var index = args.Index;
                var style = index == SelectedPageIndex ? SelectedPipStyle : NormalPipStyle;
                ApplyStyleToPipAndUpdateOrientation(element, style);

                AutomationProperties.SetName(element, Strings.PipsPagerPageText + " " + (index + 1));
#if NET48_OR_NEWER
                AutomationProperties.SetPositionInSet(element, index + 1);
                AutomationProperties.SetSizeOfSet(element, NumberOfPages);
#endif

                if (element is Button button)
                {
                    button.Tag = index;
                    button.Click -= OnPipButtonClick;
                    button.Click += OnPipButtonClick;
                    _pipButtonsByPageIndex[index] = button;
                }
            }
        }

        private void OnElementIndexChanged(ItemsRepeater sender, ItemsRepeaterElementIndexChangedEventArgs args)
        {
            if (args.Element is FrameworkElement element)
            {
                var newIndex = args.NewIndex;
                AutomationProperties.SetName(element, Strings.PipsPagerPageText + " " + (newIndex + 1));
#if NET48_OR_NEWER
                AutomationProperties.SetPositionInSet(element, newIndex + 1);
#endif
                if (element is Button button)
                {
                    button.Tag = newIndex;
                    _pipButtonsByPageIndex.Remove(args.OldIndex);
                    _pipButtonsByPageIndex[newIndex] = button;
                }
            }
        }

        private void ApplyStyleToPipAndUpdateOrientation(FrameworkElement pip, Style style)
        {
            if (style != null)
            {
                pip.Style = style;
            }

            if (pip is Control control)
            {
                control.ApplyTemplate();
                UpdatePipOrientation(control);
            }
        }

        private void UpdatePipOrientation(Control pip)
        {
            VisualStateManager.GoToState(
                pip,
                Orientation == System.Windows.Controls.Orientation.Vertical ? VerticalPipOrientationState : HorizontalPipOrientationState,
                false);
        }

        private void UpdatePipsItems(int numberOfPages, int maxVisiblePips)
        {
            var items = TemplateSettings.PipsPagerItems;
            var pipsListSize = items.Count;

            if (numberOfPages == 0 || maxVisiblePips == 0)
            {
                items.Clear();
            }
            else if (numberOfPages < 0)
            {
                var minNumberOfElements = Math.Max(SelectedPageIndex + 1, Math.Max(0, maxVisiblePips));
                if (minNumberOfElements > pipsListSize)
                {
                    for (var i = pipsListSize; i < minNumberOfElements; i++)
                    {
                        items.Add(i + 1);
                    }
                }
                else if (SelectedPageIndex == pipsListSize - 1)
                {
                    items.Add(pipsListSize + 1);
                }
            }
            else if (pipsListSize < numberOfPages)
            {
                for (var i = pipsListSize; i < numberOfPages; i++)
                {
                    items.Add(i + 1);
                }
            }
            else
            {
                for (var i = pipsListSize; i > numberOfPages; i--)
                {
                    items.RemoveAt(items.Count - 1);
                }
            }
        }

        private Size GetDesiredPipSize(Style style)
        {
            if (style == null)
            {
                return new Size();
            }

            if (_pipsPagerRepeater?.ItemTemplate is DataTemplate itemTemplate &&
                itemTemplate.LoadContent() is FrameworkElement element)
            {
                ApplyStyleToPipAndUpdateOrientation(element, style);
                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                return element.DesiredSize;
            }

            return new Size();
        }

        private void SetScrollViewerMaxSize()
        {
            if (_pipsPagerScrollViewer == null)
            {
                return;
            }

            if (Orientation == System.Windows.Controls.Orientation.Horizontal)
            {
                var scrollViewerWidth = CalculateScrollViewerSize(_defaultPipSize.Width, _selectedPipSize.Width, NumberOfPages, MaxVisiblePips);
                _pipsPagerScrollViewer.MaxWidth = scrollViewerWidth;
                _pipsPagerScrollViewer.MaxHeight = Math.Max(_defaultPipSize.Height, _selectedPipSize.Height);
            }
            else
            {
                var scrollViewerHeight = CalculateScrollViewerSize(_defaultPipSize.Height, _selectedPipSize.Height, NumberOfPages, MaxVisiblePips);
                _pipsPagerScrollViewer.MaxHeight = scrollViewerHeight;
                _pipsPagerScrollViewer.MaxWidth = Math.Max(_defaultPipSize.Width, _selectedPipSize.Width);
            }
        }

        private static double CalculateScrollViewerSize(double defaultPipSize, double selectedPipSize, int numberOfPages, int maxVisiblePips)
        {
            maxVisiblePips = Math.Max(0, maxVisiblePips);

            int numberOfPagesToDisplay;
            if (maxVisiblePips == 0 || numberOfPages == 0)
            {
                return 0;
            }
            else if (numberOfPages > 0)
            {
                numberOfPagesToDisplay = Math.Min(maxVisiblePips, numberOfPages);
            }
            else
            {
                numberOfPagesToDisplay = maxVisiblePips;
            }

            return defaultPipSize * (numberOfPagesToDisplay - 1) + selectedPipSize;
        }

        private void UpdateSizeOfSetForElements(int numberOfPages, int numberOfItems)
        {
#if NET48_OR_NEWER
            if (_pipsPagerRepeater == null)
            {
                return;
            }

            for (var i = 0; i < numberOfItems; i++)
            {
                if (_pipsPagerRepeater.TryGetElement(i) is UIElement pip)
                {
                    AutomationProperties.SetSizeOfSet(pip, numberOfPages);
                }
            }
#endif
        }

        private void OnItemsRepeaterLayoutChanged(object sender, EventArgs args)
        {
            RestoreLayoutVirtualization();
            UpdateLayoutVirtualization();
        }

        private void RestoreLayoutVirtualization()
        {
            if (_itemsRepeaterStackLayout != null)
            {
                _itemsRepeaterStackLayout.IsVirtualizationEnabled = _cachedIsVirtualizationEnabledFlag;
                _itemsRepeaterStackLayout = null;
            }

            _cachedIsVirtualizationEnabledFlag = true;
        }

        private void UpdateLayoutVirtualization()
        {
            if (_pipsPagerRepeater?.Layout is StackLayout stackLayout)
            {
                if (_itemsRepeaterStackLayout == null)
                {
                    _cachedIsVirtualizationEnabledFlag = stackLayout.IsVirtualizationEnabled;
                    _itemsRepeaterStackLayout = stackLayout;
                }

                stackLayout.IsVirtualizationEnabled = !IsWrapEnabled;
            }
        }

        private bool IsWrapEnabled => WrapMode == PipsPagerWrapMode.Wrap;

        private StackPanel _rootPanel;
        private Button _previousButton;
        private Button _nextButton;
        private ItemsRepeater _pipsPagerRepeater;
        private ScrollViewer _pipsPagerScrollViewer;
        private StackLayout _itemsRepeaterStackLayout;
        private readonly Dictionary<int, Button> _pipButtonsByPageIndex = new Dictionary<int, Button>();
        private Size _defaultPipSize;
        private Size _selectedPipSize;
        private int _lastSelectedPageIndex;
        private bool _cachedIsVirtualizationEnabledFlag = true;
        private bool _isPointerOver;
        private bool _isFocused;
        private bool _templateApplied;
    }
}
