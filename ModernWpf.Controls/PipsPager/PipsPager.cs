using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = RootPanelName, Type = typeof(StackPanel))]
    [TemplatePart(Name = PipsPanelName, Type = typeof(StackPanel))]
    [TemplatePart(Name = PreviousButtonName, Type = typeof(Button))]
    [TemplatePart(Name = NextButtonName, Type = typeof(Button))]
    public class PipsPager : Control
    {
        private const string RootPanelName = "PART_RootPanel";
        private const string PipsPanelName = "PART_PipsPanel";
        private const string PreviousButtonName = "PART_PreviousButton";
        private const string NextButtonName = "PART_NextButton";
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

        static PipsPager()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PipsPager), new FrameworkPropertyMetadata(typeof(PipsPager)));
        }

        public PipsPager()
        {
            SetValue(TemplateSettingsPropertyKey, new PipsPagerTemplateSettings());
            Loaded += OnLoaded;
            UpdatePipsPagerItems();
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
            if (_previousButton != null)
            {
                _previousButton.Click -= OnPreviousButtonClick;
            }

            if (_nextButton != null)
            {
                _nextButton.Click -= OnNextButtonClick;
            }

            base.OnApplyTemplate();

            _rootPanel = GetTemplateChild(RootPanelName) as StackPanel;
            _pipsPanel = GetTemplateChild(PipsPanelName) as StackPanel;
            _previousButton = GetTemplateChild(PreviousButtonName) as Button;
            _nextButton = GetTemplateChild(NextButtonName) as Button;

            if (_previousButton != null)
            {
                _previousButton.Click += OnPreviousButtonClick;
            }

            if (_nextButton != null)
            {
                _nextButton.Click += OnNextButtonClick;
            }

            UpdateVisuals();
        }

        public Button ContainerFromIndex(int pageIndex)
        {
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

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateNavigationButtonVisualStates();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateNavigationButtonVisualStates();
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            UpdateNavigationButtonVisualStates();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            UpdateNavigationButtonVisualStates();
        }

        private static void OnPagerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pager = (PipsPager)d;
            if (e.Property == NumberOfPagesProperty)
            {
                pager.CoerceValue(SelectedPageIndexProperty);
            }

            pager.UpdateVisuals();
        }

        private static void OnSelectedPageIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pager = (PipsPager)d;
            pager.UpdateVisuals();
            pager.RaiseSelectedIndexChanged();
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_hasRaisedInitialSelectedIndexChanged)
            {
                _hasRaisedInitialSelectedIndexChanged = true;
                RaiseSelectedIndexChanged();
            }
        }

        private void OnPreviousButtonClick(object sender, RoutedEventArgs e)
        {
            if (SelectedPageIndex > 0)
            {
                SelectedPageIndex--;
            }
        }

        private void OnNextButtonClick(object sender, RoutedEventArgs e)
        {
            if (NumberOfPages < 0 || SelectedPageIndex < NumberOfPages - 1)
            {
                SelectedPageIndex++;
            }
        }

        private void OnPipButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int pageIndex)
            {
                SelectedPageIndex = pageIndex;
            }
        }

        private void RaiseSelectedIndexChanged()
        {
            SelectedIndexChanged?.Invoke(this, new PipsPagerSelectedIndexChangedEventArgs());
        }

        private void UpdateVisuals()
        {
            CoerceValue(SelectedPageIndexProperty);
            UpdatePipsPagerItems();
            UpdateRootPanel();
            UpdateNavigationButton(_previousButton, PreviousButtonStyle);
            UpdateNavigationButton(_nextButton, NextButtonStyle);
            UpdateNavigationButtonVisualStates();
            UpdatePipButtons();
        }

        private void UpdateRootPanel()
        {
            if (_pipsPanel != null)
            {
                _pipsPanel.Orientation = Orientation;
            }

            VisualStateManager.GoToState(
                this,
                Orientation == System.Windows.Controls.Orientation.Horizontal ? HorizontalOrientationViewState : VerticalOrientationViewState,
                false);
        }

        private void UpdateNavigationButton(Button button, Style style)
        {
            if (button == null)
            {
                return;
            }

            if (style != null)
            {
                button.Style = style;
            }
        }

        private void UpdateNavigationButtonVisualStates()
        {
            UpdateNavigationButtonVisualStates(
                PreviousButtonVisibility,
                SelectedPageIndex > 0,
                PreviousPageButtonVisibleState,
                PreviousPageButtonHiddenState,
                PreviousPageButtonCollapsedState,
                PreviousPageButtonEnabledState,
                PreviousPageButtonDisabledState);
            UpdateNavigationButtonVisualStates(
                NextButtonVisibility,
                NumberOfPages < 0 || SelectedPageIndex < NumberOfPages - 1,
                NextPageButtonVisibleState,
                NextPageButtonHiddenState,
                NextPageButtonCollapsedState,
                NextPageButtonEnabledState,
                NextPageButtonDisabledState);
        }

        private void UpdateNavigationButtonVisualStates(
            PipsPagerButtonVisibility buttonVisibility,
            bool isPageNavigationAvailable,
            string visibleStateName,
            string hiddenStateName,
            string collapsedStateName,
            string enabledStateName,
            string disabledStateName)
        {
            if (buttonVisibility == PipsPagerButtonVisibility.Collapsed)
            {
                VisualStateManager.GoToState(this, collapsedStateName, false);
                VisualStateManager.GoToState(this, disabledStateName, false);
                return;
            }

            bool isGenerallyVisible = isPageNavigationAvailable && NumberOfPages != 0 && MaxVisiblePips > 0;
            bool shouldShow = isGenerallyVisible &&
                (buttonVisibility == PipsPagerButtonVisibility.Visible || IsMouseOver || IsKeyboardFocusWithin);

            VisualStateManager.GoToState(this, shouldShow ? visibleStateName : hiddenStateName, false);
            VisualStateManager.GoToState(this, isGenerallyVisible ? enabledStateName : disabledStateName, false);
        }

        private void UpdatePipButtons()
        {
            if (_pipsPanel == null)
            {
                return;
            }

            foreach (var button in _pipButtonsByPageIndex.Values)
            {
                button.Click -= OnPipButtonClick;
            }

            _pipButtonsByPageIndex.Clear();
            _pipsPanel.Children.Clear();

            var sizeOfSet = NumberOfPages > 0 ? NumberOfPages : TemplateSettings.PipsPagerItems.Count;
            foreach (var pageIndex in TemplateSettings.PipsPagerItems)
            {
                var button = new Button
                {
                    Content = (pageIndex + 1).ToString(),
                    Tag = pageIndex,
                    MinWidth = 20,
                    MinHeight = 20,
                    Margin = new Thickness(2),
                    Padding = new Thickness(0),
                    Focusable = true,
                    FontWeight = pageIndex == SelectedPageIndex ? FontWeights.Bold : FontWeights.Normal
                };

                var style = pageIndex == SelectedPageIndex ? SelectedPipStyle : NormalPipStyle;
                if (style != null)
                {
                    button.Style = style;
                }

                AutomationProperties.SetName(button, $"Page {pageIndex + 1}");
#if NET48_OR_NEWER
                AutomationProperties.SetPositionInSet(button, pageIndex + 1);
                AutomationProperties.SetSizeOfSet(button, sizeOfSet);
#endif
                button.Click += OnPipButtonClick;
                _pipButtonsByPageIndex[pageIndex] = button;
                _pipsPanel.Children.Add(button);
            }
        }

        private void UpdatePipsPagerItems()
        {
            var items = TemplateSettings.PipsPagerItems;
            items.Clear();

            var visibleCount = GetVisiblePipCount();
            if (visibleCount == 0)
            {
                return;
            }

            var startIndex = Math.Max(0, SelectedPageIndex - visibleCount / 2);
            if (NumberOfPages > 0)
            {
                startIndex = Math.Min(startIndex, NumberOfPages - visibleCount);
            }

            for (var i = 0; i < visibleCount; i++)
            {
                items.Add(startIndex + i);
            }
        }

        private int GetVisiblePipCount()
        {
            if (MaxVisiblePips <= 0 || NumberOfPages == 0)
            {
                return 0;
            }

            return NumberOfPages > 0
                ? Math.Min(MaxVisiblePips, NumberOfPages)
                : MaxVisiblePips;
        }

        private StackPanel _rootPanel;
        private StackPanel _pipsPanel;
        private Button _previousButton;
        private Button _nextButton;
        private readonly Dictionary<int, Button> _pipButtonsByPageIndex = new Dictionary<int, Button>();
        private bool _hasRaisedInitialSelectedIndexChanged;
    }
}
