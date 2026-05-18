using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Automation.Peers;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = RootGridName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ComboBoxName, Type = typeof(ComboBox))]
    [TemplatePart(Name = NumberBoxName, Type = typeof(NumberBox))]
    [TemplatePart(Name = NumberPanelRepeaterName, Type = typeof(ItemsRepeater))]
    [TemplatePart(Name = NumberPanelIndicatorName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = FirstPageButtonName, Type = typeof(Button))]
    [TemplatePart(Name = PreviousPageButtonName, Type = typeof(Button))]
    [TemplatePart(Name = NextPageButtonName, Type = typeof(Button))]
    [TemplatePart(Name = LastPageButtonName, Type = typeof(Button))]
    public class PagerControl : Control
    {
        private const string NumberBoxVisibleVisualState = "NumberBoxVisible";
        private const string ComboBoxVisibleVisualState = "ComboBoxVisible";
        private const string NumberPanelVisibleVisualState = "NumberPanelVisible";

        private const string FirstPageButtonVisibleVisualState = "FirstPageButtonVisible";
        private const string FirstPageButtonCollapsedVisualState = "FirstPageButtonCollapsed";
        private const string FirstPageButtonHiddenVisualState = "FirstPageButtonHidden";
        private const string FirstPageButtonEnabledVisualState = "FirstPageButtonEnabled";
        private const string FirstPageButtonDisabledVisualState = "FirstPageButtonDisabled";

        private const string PreviousPageButtonVisibleVisualState = "PreviousPageButtonVisible";
        private const string PreviousPageButtonCollapsedVisualState = "PreviousPageButtonCollapsed";
        private const string PreviousPageButtonHiddenVisualState = "PreviousPageButtonHidden";
        private const string PreviousPageButtonEnabledVisualState = "PreviousPageButtonEnabled";
        private const string PreviousPageButtonDisabledVisualState = "PreviousPageButtonDisabled";

        private const string NextPageButtonVisibleVisualState = "NextPageButtonVisible";
        private const string NextPageButtonCollapsedVisualState = "NextPageButtonCollapsed";
        private const string NextPageButtonHiddenVisualState = "NextPageButtonHidden";
        private const string NextPageButtonEnabledVisualState = "NextPageButtonEnabled";
        private const string NextPageButtonDisabledVisualState = "NextPageButtonDisabled";

        private const string LastPageButtonVisibleVisualState = "LastPageButtonVisible";
        private const string LastPageButtonCollapsedVisualState = "LastPageButtonCollapsed";
        private const string LastPageButtonHiddenVisualState = "LastPageButtonHidden";
        private const string LastPageButtonEnabledVisualState = "LastPageButtonEnabled";
        private const string LastPageButtonDisabledVisualState = "LastPageButtonDisabled";

        private const string FiniteItemsModeState = "FiniteItems";
        private const string InfiniteItemsModeState = "InfiniteItems";

        private const string RootGridName = "RootGrid";
        private const string ComboBoxName = "ComboBoxDisplay";
        private const string NumberBoxName = "NumberBoxDisplay";
        private const string NumberPanelRepeaterName = "NumberPanelItemsRepeater";
        private const string NumberPanelIndicatorName = "NumberPanelCurrentPageIndicator";
        private const string FirstPageButtonName = "FirstPageButton";
        private const string PreviousPageButtonName = "PreviousPageButton";
        private const string NextPageButtonName = "NextPageButton";
        private const string LastPageButtonName = "LastPageButton";

        private const string TemplateNumberPanelButtonStyleName = "PagerControlTemplateNumberPanelButtonStyle";
        private const string NumberPanelButtonStyleName = "PagerControlNumberPanelButtonStyle";
        private const int AutoDisplayModeNumberOfPagesThreshold = 10;
        private const int InfiniteModeComboBoxItemsIncrement = 100;

        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(PagerControl));

        static PagerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PagerControl), new FrameworkPropertyMetadata(typeof(PagerControl)));
        }

        public PagerControl()
        {
            m_comboBoxEntries = new ObservableCollection<object>();
            m_numberPanelElements = new ObservableCollection<object>();
            SetValue(TemplateSettingsPropertyKey, new PagerControlTemplateSettings(m_comboBoxEntries, m_numberPanelElements));
        }

        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register(
                nameof(DisplayMode),
                typeof(PagerControlDisplayMode),
                typeof(PagerControl),
                new FrameworkPropertyMetadata(PagerControlDisplayMode.Auto, OnPagerPropertyChanged));

        public PagerControlDisplayMode DisplayMode
        {
            get => (PagerControlDisplayMode)GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }

        public static readonly DependencyProperty NumberOfPagesProperty =
            DependencyProperty.Register(
                nameof(NumberOfPages),
                typeof(int),
                typeof(PagerControl),
                new FrameworkPropertyMetadata(0, OnNumberOfPagesPropertyChanged, CoerceNumberOfPages));

        public int NumberOfPages
        {
            get => (int)GetValue(NumberOfPagesProperty);
            set => SetValue(NumberOfPagesProperty, value);
        }

        public static readonly DependencyProperty FirstButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(FirstButtonVisibility),
                typeof(PagerControlButtonVisibility),
                typeof(PagerControl),
                new FrameworkPropertyMetadata(PagerControlButtonVisibility.Visible, OnPagerPropertyChanged));

        public PagerControlButtonVisibility FirstButtonVisibility
        {
            get => (PagerControlButtonVisibility)GetValue(FirstButtonVisibilityProperty);
            set => SetValue(FirstButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty PreviousButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(PreviousButtonVisibility),
                typeof(PagerControlButtonVisibility),
                typeof(PagerControl),
                new FrameworkPropertyMetadata(PagerControlButtonVisibility.Visible, OnPagerPropertyChanged));

        public PagerControlButtonVisibility PreviousButtonVisibility
        {
            get => (PagerControlButtonVisibility)GetValue(PreviousButtonVisibilityProperty);
            set => SetValue(PreviousButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty NextButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(NextButtonVisibility),
                typeof(PagerControlButtonVisibility),
                typeof(PagerControl),
                new FrameworkPropertyMetadata(PagerControlButtonVisibility.Visible, OnPagerPropertyChanged));

        public PagerControlButtonVisibility NextButtonVisibility
        {
            get => (PagerControlButtonVisibility)GetValue(NextButtonVisibilityProperty);
            set => SetValue(NextButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty LastButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(LastButtonVisibility),
                typeof(PagerControlButtonVisibility),
                typeof(PagerControl),
                new FrameworkPropertyMetadata(PagerControlButtonVisibility.Visible, OnPagerPropertyChanged));

        public PagerControlButtonVisibility LastButtonVisibility
        {
            get => (PagerControlButtonVisibility)GetValue(LastButtonVisibilityProperty);
            set => SetValue(LastButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty FirstButtonCommandProperty =
            DependencyProperty.Register(
                nameof(FirstButtonCommand),
                typeof(ICommand),
                typeof(PagerControl),
                null);

        public ICommand FirstButtonCommand
        {
            get => (ICommand)GetValue(FirstButtonCommandProperty);
            set => SetValue(FirstButtonCommandProperty, value);
        }

        public static readonly DependencyProperty PreviousButtonCommandProperty =
            DependencyProperty.Register(
                nameof(PreviousButtonCommand),
                typeof(ICommand),
                typeof(PagerControl),
                null);

        public ICommand PreviousButtonCommand
        {
            get => (ICommand)GetValue(PreviousButtonCommandProperty);
            set => SetValue(PreviousButtonCommandProperty, value);
        }

        public static readonly DependencyProperty NextButtonCommandProperty =
            DependencyProperty.Register(
                nameof(NextButtonCommand),
                typeof(ICommand),
                typeof(PagerControl),
                null);

        public ICommand NextButtonCommand
        {
            get => (ICommand)GetValue(NextButtonCommandProperty);
            set => SetValue(NextButtonCommandProperty, value);
        }

        public static readonly DependencyProperty LastButtonCommandProperty =
            DependencyProperty.Register(
                nameof(LastButtonCommand),
                typeof(ICommand),
                typeof(PagerControl),
                null);

        public ICommand LastButtonCommand
        {
            get => (ICommand)GetValue(LastButtonCommandProperty);
            set => SetValue(LastButtonCommandProperty, value);
        }

        public static readonly DependencyProperty PagerInputCommandProperty =
            DependencyProperty.Register(
                nameof(PagerInputCommand),
                typeof(ICommand),
                typeof(PagerControl),
                null);

        public ICommand PagerInputCommand
        {
            get => (ICommand)GetValue(PagerInputCommandProperty);
            set => SetValue(PagerInputCommandProperty, value);
        }

        public static readonly DependencyProperty FirstButtonStyleProperty =
            DependencyProperty.Register(
                nameof(FirstButtonStyle),
                typeof(Style),
                typeof(PagerControl),
                null);

        public Style FirstButtonStyle
        {
            get => (Style)GetValue(FirstButtonStyleProperty);
            set => SetValue(FirstButtonStyleProperty, value);
        }

        public static readonly DependencyProperty PreviousButtonStyleProperty =
            DependencyProperty.Register(
                nameof(PreviousButtonStyle),
                typeof(Style),
                typeof(PagerControl),
                null);

        public Style PreviousButtonStyle
        {
            get => (Style)GetValue(PreviousButtonStyleProperty);
            set => SetValue(PreviousButtonStyleProperty, value);
        }

        public static readonly DependencyProperty NextButtonStyleProperty =
            DependencyProperty.Register(
                nameof(NextButtonStyle),
                typeof(Style),
                typeof(PagerControl),
                null);

        public Style NextButtonStyle
        {
            get => (Style)GetValue(NextButtonStyleProperty);
            set => SetValue(NextButtonStyleProperty, value);
        }

        public static readonly DependencyProperty LastButtonStyleProperty =
            DependencyProperty.Register(
                nameof(LastButtonStyle),
                typeof(Style),
                typeof(PagerControl),
                null);

        public Style LastButtonStyle
        {
            get => (Style)GetValue(LastButtonStyleProperty);
            set => SetValue(LastButtonStyleProperty, value);
        }

        public static readonly DependencyProperty ButtonPanelAlwaysShowFirstLastPageIndexProperty =
            DependencyProperty.Register(
                nameof(ButtonPanelAlwaysShowFirstLastPageIndex),
                typeof(bool),
                typeof(PagerControl),
                new FrameworkPropertyMetadata(true, OnPagerPropertyChanged));

        public bool ButtonPanelAlwaysShowFirstLastPageIndex
        {
            get => (bool)GetValue(ButtonPanelAlwaysShowFirstLastPageIndexProperty);
            set => SetValue(ButtonPanelAlwaysShowFirstLastPageIndexProperty, value);
        }

        public static readonly DependencyProperty SelectedPageIndexProperty =
            DependencyProperty.Register(
                nameof(SelectedPageIndex),
                typeof(int),
                typeof(PagerControl),
                new FrameworkPropertyMetadata(0, OnSelectedPageIndexPropertyChanged, CoerceSelectedPageIndex));

        public int SelectedPageIndex
        {
            get => (int)GetValue(SelectedPageIndexProperty);
            set => SetValue(SelectedPageIndexProperty, value);
        }

        public static readonly DependencyProperty PrefixTextProperty =
            DependencyProperty.Register(
                nameof(PrefixText),
                typeof(string),
                typeof(PagerControl),
                new FrameworkPropertyMetadata(string.Empty));

        public string PrefixText
        {
            get => (string)GetValue(PrefixTextProperty);
            set => SetValue(PrefixTextProperty, value);
        }

        public static readonly DependencyProperty SuffixTextProperty =
            DependencyProperty.Register(
                nameof(SuffixText),
                typeof(string),
                typeof(PagerControl),
                new FrameworkPropertyMetadata(string.Empty));

        public string SuffixText
        {
            get => (string)GetValue(SuffixTextProperty);
            set => SetValue(SuffixTextProperty, value);
        }

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(PagerControlTemplateSettings),
                typeof(PagerControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TemplateSettingsProperty = TemplateSettingsPropertyKey.DependencyProperty;

        public PagerControlTemplateSettings TemplateSettings => (PagerControlTemplateSettings)GetValue(TemplateSettingsProperty);

        public event TypedEventHandler<PagerControl, PagerControlSelectedIndexChangedEventArgs> SelectedIndexChanged;

        public override void OnApplyTemplate()
        {
            UnhookTemplateEvents();

            base.OnApplyTemplate();

            if (string.IsNullOrEmpty(PrefixText))
            {
                PrefixText = ResourceAccessor.GetLocalizedStringResource(SR_PagerControlPrefixTextName);
            }

            if (string.IsNullOrEmpty(SuffixText))
            {
                SuffixText = ResourceAccessor.GetLocalizedStringResource(SR_PagerControlSuffixTextName);
            }

            m_rootGrid = GetTemplateChild(RootGridName) as FrameworkElement;
            if (m_rootGrid != null)
            {
                m_rootGrid.KeyDown += OnRootGridKeyDown;
            }

            m_firstPageButton = GetTemplateChild(FirstPageButtonName) as Button;
            if (m_firstPageButton != null)
            {
                AutomationProperties.SetName(m_firstPageButton, ResourceAccessor.GetLocalizedStringResource(SR_PagerControlFirstPageButtonTextName));
                m_firstPageButton.Click += FirstButtonClicked;
            }

            m_previousPageButton = GetTemplateChild(PreviousPageButtonName) as Button;
            if (m_previousPageButton != null)
            {
                AutomationProperties.SetName(m_previousPageButton, ResourceAccessor.GetLocalizedStringResource(SR_PagerControlPreviousPageButtonTextName));
                m_previousPageButton.Click += PreviousButtonClicked;
            }

            m_nextPageButton = GetTemplateChild(NextPageButtonName) as Button;
            if (m_nextPageButton != null)
            {
                AutomationProperties.SetName(m_nextPageButton, ResourceAccessor.GetLocalizedStringResource(SR_PagerControlNextPageButtonTextName));
                m_nextPageButton.Click += NextButtonClicked;
            }

            m_lastPageButton = GetTemplateChild(LastPageButtonName) as Button;
            if (m_lastPageButton != null)
            {
                AutomationProperties.SetName(m_lastPageButton, ResourceAccessor.GetLocalizedStringResource(SR_PagerControlLastPageButtonTextName));
                m_lastPageButton.Click += LastButtonClicked;
            }

            m_comboBox = GetTemplateChild(ComboBoxName) as ComboBox;
            if (m_comboBox != null)
            {
                FillComboBoxCollectionToSize(NumberOfPages);
                m_comboBox.SelectedIndex = SelectedPageIndex - 1;
                AutomationProperties.SetName(m_comboBox, ResourceAccessor.GetLocalizedStringResource(SR_PagerControlPageTextName));
                m_comboBox.SelectionChanged += ComboBoxSelectionChanged;
            }

            m_numberBox = GetTemplateChild(NumberBoxName) as NumberBox;
            if (m_numberBox != null)
            {
                m_numberBox.Value = SelectedPageIndex + 1;
                AutomationProperties.SetName(m_numberBox, ResourceAccessor.GetLocalizedStringResource(SR_PagerControlPageTextName));
                m_numberBox.ValueChanged += NumberBoxValueChanged;
            }

            m_numberPanelRepeater = GetTemplateChild(NumberPanelRepeaterName) as ItemsRepeater;
            m_selectedPageIndicator = GetTemplateChild(NumberPanelIndicatorName) as FrameworkElement;

            m_templateApplied = true;

            OnDisplayModeChanged();
            UpdateOnEdgeButtonVisualStates();
            OnNumberOfPagesChanged(0);

            OnButtonVisibilityChanged(
                FirstButtonVisibility,
                FirstPageButtonVisibleVisualState,
                FirstPageButtonCollapsedVisualState,
                FirstPageButtonHiddenVisualState,
                0);
            OnButtonVisibilityChanged(
                PreviousButtonVisibility,
                PreviousPageButtonVisibleVisualState,
                PreviousPageButtonCollapsedVisualState,
                PreviousPageButtonHiddenVisualState,
                0);
            OnButtonVisibilityChanged(
                NextButtonVisibility,
                NextPageButtonVisibleVisualState,
                NextPageButtonCollapsedVisualState,
                NextPageButtonHiddenVisualState,
                NumberOfPages - 1);
            OnButtonVisibilityChanged(
                LastButtonVisibility,
                LastPageButtonVisibleVisualState,
                LastPageButtonCollapsedVisualState,
                LastPageButtonHiddenVisualState,
                NumberOfPages - 1);

            OnSelectedPageIndexChange(-1);
        }

        internal Button ContainerFromPageIndex(int pageIndex)
        {
            if (m_numberPanelRepeater != null)
            {
                return (m_numberPanelRepeater.TryGetElement(pageIndex) ??
                        m_numberPanelRepeater.GetOrCreateElement(pageIndex)) as Button;
            }

            return null;
        }

        internal Button GetSelectedButton()
        {
            return ContainerFromPageIndex(SelectedPageIndex);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new PagerControlAutomationPeer(this);
        }

        private static void OnPagerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PagerControl)d).HandlePropertyChanged(e);
        }

        private static void OnNumberOfPagesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pager = (PagerControl)d;
            pager.CoerceValue(SelectedPageIndexProperty);
            pager.HandlePropertyChanged(e);
        }

        private static void OnSelectedPageIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PagerControl)d).HandlePropertyChanged(e);
        }

        private static object CoerceNumberOfPages(DependencyObject d, object baseValue)
        {
            return Math.Max(-1, (int)baseValue);
        }

        private static object CoerceSelectedPageIndex(DependencyObject d, object baseValue)
        {
            var pager = (PagerControl)d;
            var index = Math.Max(0, (int)baseValue);

            if (pager.NumberOfPages > 0)
            {
                index = Math.Min(index, pager.NumberOfPages - 1);
            }

            return index;
        }

        private void HandlePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!m_templateApplied)
            {
                return;
            }

            if (args.Property == FirstButtonVisibilityProperty)
            {
                OnButtonVisibilityChanged(
                    FirstButtonVisibility,
                    FirstPageButtonVisibleVisualState,
                    FirstPageButtonCollapsedVisualState,
                    FirstPageButtonHiddenVisualState,
                    0);
            }
            else if (args.Property == PreviousButtonVisibilityProperty)
            {
                OnButtonVisibilityChanged(
                    PreviousButtonVisibility,
                    PreviousPageButtonVisibleVisualState,
                    PreviousPageButtonCollapsedVisualState,
                    PreviousPageButtonHiddenVisualState,
                    0);
            }
            else if (args.Property == NextButtonVisibilityProperty)
            {
                OnButtonVisibilityChanged(
                    NextButtonVisibility,
                    NextPageButtonVisibleVisualState,
                    NextPageButtonCollapsedVisualState,
                    NextPageButtonHiddenVisualState,
                    NumberOfPages - 1);
            }
            else if (args.Property == LastButtonVisibilityProperty)
            {
                OnButtonVisibilityChanged(
                    LastButtonVisibility,
                    LastPageButtonVisibleVisualState,
                    LastPageButtonCollapsedVisualState,
                    LastPageButtonHiddenVisualState,
                    NumberOfPages - 1);
            }
            else if (args.Property == DisplayModeProperty)
            {
                OnDisplayModeChanged();
                UpdateTemplateSettingElementLists();
            }
            else if (args.Property == NumberOfPagesProperty)
            {
                OnNumberOfPagesChanged((int)args.OldValue);
            }
            else if (args.Property == SelectedPageIndexProperty)
            {
                OnSelectedPageIndexChange((int)args.OldValue);
            }
            else if (args.Property == ButtonPanelAlwaysShowFirstLastPageIndexProperty)
            {
                UpdateNumberPanel(NumberOfPages);
            }
        }

        private void OnDisplayModeChanged()
        {
            var displayMode = DisplayMode;

            if (displayMode == PagerControlDisplayMode.ButtonPanel)
            {
                VisualStateManager.GoToState(this, NumberPanelVisibleVisualState, false);
            }
            else if (displayMode == PagerControlDisplayMode.ComboBox)
            {
                VisualStateManager.GoToState(this, ComboBoxVisibleVisualState, false);
            }
            else if (displayMode == PagerControlDisplayMode.NumberBox)
            {
                VisualStateManager.GoToState(this, NumberBoxVisibleVisualState, false);
            }
            else
            {
                UpdateDisplayModeAutoState();
            }
        }

        private void UpdateDisplayModeAutoState()
        {
            var numberOfPages = NumberOfPages;
            if (numberOfPages > -1)
            {
                VisualStateManager.GoToState(
                    this,
                    numberOfPages < AutoDisplayModeNumberOfPagesThreshold ? ComboBoxVisibleVisualState : NumberBoxVisibleVisualState,
                    false);
            }
            else
            {
                VisualStateManager.GoToState(this, NumberBoxVisibleVisualState, false);
            }
        }

        private void OnNumberOfPagesChanged(int oldValue)
        {
            m_lastNumberOfPagesCount = oldValue;
            var numberOfPages = NumberOfPages;
            if (numberOfPages < SelectedPageIndex && numberOfPages > -1)
            {
                SelectedPageIndex = numberOfPages - 1;
            }

            UpdateTemplateSettingElementLists();

            if (DisplayMode == PagerControlDisplayMode.Auto)
            {
                UpdateDisplayModeAutoState();
            }

            if (numberOfPages > -1)
            {
                VisualStateManager.GoToState(this, FiniteItemsModeState, false);
                if (m_numberBox != null)
                {
                    m_numberBox.Maximum = numberOfPages;
                }
            }
            else
            {
                VisualStateManager.GoToState(this, InfiniteItemsModeState, false);
                if (m_numberBox != null)
                {
                    m_numberBox.Maximum = double.PositiveInfinity;
                }
            }

            UpdateOnEdgeButtonVisualStates();
        }

        private void OnSelectedPageIndexChange(int oldValue)
        {
            if (SelectedPageIndex > NumberOfPages - 1 && NumberOfPages > 0)
            {
                SelectedPageIndex = NumberOfPages - 1;
            }
            else if (SelectedPageIndex < 0)
            {
                SelectedPageIndex = 0;
            }

            m_lastSelectedPageIndex = oldValue;

            if (m_comboBox != null && SelectedPageIndex < m_comboBoxEntries.Count)
            {
                m_comboBox.SelectedIndex = SelectedPageIndex;
            }

            if (m_numberBox != null)
            {
                m_numberBox.Value = SelectedPageIndex + 1;
            }

            UpdateOnEdgeButtonVisualStates();
            UpdateTemplateSettingElementLists();

            if (DisplayMode == PagerControlDisplayMode.ButtonPanel)
            {
                UpdateNumberPanel(NumberOfPages);
            }

            if (FrameworkElementAutomationPeer.FromElement(this) is PagerControlAutomationPeer peer)
            {
                peer.RaiseSelectionChanged();
            }

            RaiseSelectedIndexChanged();
        }

        private void RaiseSelectedIndexChanged()
        {
            SelectedIndexChanged?.Invoke(this, new PagerControlSelectedIndexChangedEventArgs(m_lastSelectedPageIndex, SelectedPageIndex));
        }

        private void OnButtonVisibilityChanged(
            PagerControlButtonVisibility visibility,
            string visibleStateName,
            string collapsedStateName,
            string hiddenStateName,
            int hiddenOnEdgePageCriteria)
        {
            if (visibility == PagerControlButtonVisibility.Visible)
            {
                VisualStateManager.GoToState(this, visibleStateName, false);
            }
            else if (visibility == PagerControlButtonVisibility.Hidden)
            {
                VisualStateManager.GoToState(this, collapsedStateName, false);
            }
            else
            {
                VisualStateManager.GoToState(
                    this,
                    SelectedPageIndex != hiddenOnEdgePageCriteria ? visibleStateName : hiddenStateName,
                    false);
            }
        }

        private void UpdateTemplateSettingElementLists()
        {
            var displayMode = DisplayMode;
            var numberOfPages = NumberOfPages;

            if (displayMode == PagerControlDisplayMode.ComboBox ||
                displayMode == PagerControlDisplayMode.Auto)
            {
                if (numberOfPages > -1)
                {
                    FillComboBoxCollectionToSize(numberOfPages);
                }
                else if (m_comboBoxEntries.Count < InfiniteModeComboBoxItemsIncrement)
                {
                    FillComboBoxCollectionToSize(InfiniteModeComboBoxItemsIncrement);
                }
            }
            else if (displayMode == PagerControlDisplayMode.ButtonPanel)
            {
                UpdateNumberPanel(numberOfPages);
            }
        }

        private void FillComboBoxCollectionToSize(int numberOfPages)
        {
            var currentComboBoxItemsCount = m_comboBoxEntries.Count;
            if (currentComboBoxItemsCount <= numberOfPages)
            {
                for (var i = currentComboBoxItemsCount; i < numberOfPages; i++)
                {
                    m_comboBoxEntries.Add(i + 1);
                }
            }
            else
            {
                for (var i = currentComboBoxItemsCount; i > numberOfPages; i--)
                {
                    m_comboBoxEntries.RemoveAt(m_comboBoxEntries.Count - 1);
                }
            }
        }

        private void UpdateOnEdgeButtonVisualStates()
        {
            var selectedPageIndex = SelectedPageIndex;
            var numberOfPages = NumberOfPages;

            if (selectedPageIndex == 0)
            {
                VisualStateManager.GoToState(this, FirstPageButtonDisabledVisualState, false);
                VisualStateManager.GoToState(this, PreviousPageButtonDisabledVisualState, false);
                VisualStateManager.GoToState(this, NextPageButtonEnabledVisualState, false);
                VisualStateManager.GoToState(this, LastPageButtonEnabledVisualState, false);
            }
            else if (selectedPageIndex >= numberOfPages - 1)
            {
                VisualStateManager.GoToState(this, FirstPageButtonEnabledVisualState, false);
                VisualStateManager.GoToState(this, PreviousPageButtonEnabledVisualState, false);
                VisualStateManager.GoToState(
                    this,
                    numberOfPages > -1 ? NextPageButtonDisabledVisualState : NextPageButtonEnabledVisualState,
                    false);
                VisualStateManager.GoToState(this, LastPageButtonDisabledVisualState, false);
            }
            else
            {
                VisualStateManager.GoToState(this, FirstPageButtonEnabledVisualState, false);
                VisualStateManager.GoToState(this, PreviousPageButtonEnabledVisualState, false);
                VisualStateManager.GoToState(this, NextPageButtonEnabledVisualState, false);
                VisualStateManager.GoToState(this, LastPageButtonEnabledVisualState, false);
            }

            if (FirstButtonVisibility == PagerControlButtonVisibility.HiddenOnEdge)
            {
                VisualStateManager.GoToState(
                    this,
                    selectedPageIndex != 0 ? FirstPageButtonVisibleVisualState : FirstPageButtonHiddenVisualState,
                    false);
            }

            if (PreviousButtonVisibility == PagerControlButtonVisibility.HiddenOnEdge)
            {
                VisualStateManager.GoToState(
                    this,
                    selectedPageIndex != 0 ? PreviousPageButtonVisibleVisualState : PreviousPageButtonHiddenVisualState,
                    false);
            }

            if (NextButtonVisibility == PagerControlButtonVisibility.HiddenOnEdge)
            {
                VisualStateManager.GoToState(
                    this,
                    selectedPageIndex != numberOfPages - 1 ? NextPageButtonVisibleVisualState : NextPageButtonHiddenVisualState,
                    false);
            }

            if (LastButtonVisibility == PagerControlButtonVisibility.HiddenOnEdge)
            {
                VisualStateManager.GoToState(
                    this,
                    selectedPageIndex != numberOfPages - 1 ? LastPageButtonVisibleVisualState : LastPageButtonHiddenVisualState,
                    false);
            }
        }

        private void UpdateNumberPanel(int numberOfPages)
        {
            if (numberOfPages < 0)
            {
                UpdateNumberOfPanelCollectionInfiniteItems();
            }
            else if (numberOfPages < 8)
            {
                UpdateNumberPanelCollectionAllItems(numberOfPages);
            }
            else
            {
                var selectedIndex = SelectedPageIndex;
                if (selectedIndex < 4)
                {
                    UpdateNumberPanelCollectionStartWithEllipsis(numberOfPages, selectedIndex);
                }
                else if (selectedIndex >= numberOfPages - 4)
                {
                    UpdateNumberPanelCollectionEndWithEllipsis(numberOfPages, selectedIndex);
                }
                else
                {
                    UpdateNumberPanelCollectionCenterWithEllipsis(numberOfPages, selectedIndex);
                }
            }
        }

        private void UpdateNumberOfPanelCollectionInfiniteItems()
        {
            var selectedIndex = SelectedPageIndex;

            m_numberPanelElements.Clear();
            if (selectedIndex < 3)
            {
                AppendButtonToNumberPanelList(1, 0);
                AppendButtonToNumberPanelList(2, 0);
                AppendButtonToNumberPanelList(3, 0);
                AppendButtonToNumberPanelList(4, 0);
                AppendButtonToNumberPanelList(5, 0);
                MoveIdentifierToElement(selectedIndex);
            }
            else
            {
                AppendButtonToNumberPanelList(1, 0);
                AppendEllipsisIconToNumberPanelList();
                AppendButtonToNumberPanelList(selectedIndex, 0);
                AppendButtonToNumberPanelList(selectedIndex + 1, 0);
                AppendButtonToNumberPanelList(selectedIndex + 2, 0);
                MoveIdentifierToElement(3);
            }
        }

        private void UpdateNumberPanelCollectionAllItems(int numberOfPages)
        {
            if (m_lastNumberOfPagesCount != numberOfPages)
            {
                m_numberPanelElements.Clear();
                for (var i = 0; i < numberOfPages && i < 7; i++)
                {
                    AppendButtonToNumberPanelList(i + 1, numberOfPages);
                }
            }

            MoveIdentifierToElement(SelectedPageIndex);
        }

        private void UpdateNumberPanelCollectionStartWithEllipsis(int numberOfPages, int selectedIndex)
        {
            if (m_lastNumberOfPagesCount != numberOfPages)
            {
                m_numberPanelElements.Clear();
                AppendButtonToNumberPanelList(1, numberOfPages);
                AppendButtonToNumberPanelList(2, numberOfPages);
                AppendButtonToNumberPanelList(3, numberOfPages);
                AppendButtonToNumberPanelList(4, numberOfPages);
                AppendButtonToNumberPanelList(5, numberOfPages);
                if (ButtonPanelAlwaysShowFirstLastPageIndex)
                {
                    AppendEllipsisIconToNumberPanelList();
                    AppendButtonToNumberPanelList(numberOfPages, numberOfPages);
                }
            }

            MoveIdentifierToElement(selectedIndex);
        }

        private void UpdateNumberPanelCollectionEndWithEllipsis(int numberOfPages, int selectedIndex)
        {
            if (m_lastNumberOfPagesCount != numberOfPages)
            {
                m_numberPanelElements.Clear();
                if (ButtonPanelAlwaysShowFirstLastPageIndex)
                {
                    AppendButtonToNumberPanelList(1, numberOfPages);
                    AppendEllipsisIconToNumberPanelList();
                }

                AppendButtonToNumberPanelList(numberOfPages - 4, numberOfPages);
                AppendButtonToNumberPanelList(numberOfPages - 3, numberOfPages);
                AppendButtonToNumberPanelList(numberOfPages - 2, numberOfPages);
                AppendButtonToNumberPanelList(numberOfPages - 1, numberOfPages);
                AppendButtonToNumberPanelList(numberOfPages, numberOfPages);
            }

            if (ButtonPanelAlwaysShowFirstLastPageIndex)
            {
                MoveIdentifierToElement(selectedIndex - numberOfPages + 7);
            }
            else
            {
                MoveIdentifierToElement(selectedIndex - numberOfPages + 5);
            }
        }

        private void UpdateNumberPanelCollectionCenterWithEllipsis(int numberOfPages, int selectedIndex)
        {
            var showFirstLastPageIndex = ButtonPanelAlwaysShowFirstLastPageIndex;
            if (m_lastNumberOfPagesCount != numberOfPages)
            {
                m_numberPanelElements.Clear();
                if (showFirstLastPageIndex)
                {
                    AppendButtonToNumberPanelList(1, numberOfPages);
                    AppendEllipsisIconToNumberPanelList();
                }

                AppendButtonToNumberPanelList(selectedIndex, numberOfPages);
                AppendButtonToNumberPanelList(selectedIndex + 1, numberOfPages);
                AppendButtonToNumberPanelList(selectedIndex + 2, numberOfPages);
                if (showFirstLastPageIndex)
                {
                    AppendEllipsisIconToNumberPanelList();
                    AppendButtonToNumberPanelList(numberOfPages, numberOfPages);
                }
            }

            MoveIdentifierToElement(showFirstLastPageIndex ? 3 : 1);
        }

        private void MoveIdentifierToElement(int index)
        {
            if (m_selectedPageIndicator == null || m_numberPanelRepeater == null)
            {
                return;
            }

            m_numberPanelRepeater.UpdateLayout();
            if (m_numberPanelRepeater.TryGetElement(index) is FrameworkElement element)
            {
                var bounds = element.TransformToVisual(m_numberPanelRepeater)
                    .TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
                m_selectedPageIndicator.Margin = new Thickness(bounds.X, 0, 0, 0);
                m_selectedPageIndicator.Width = element.ActualWidth;
            }
        }

        private void AppendButtonToNumberPanelList(int pageNumber, int numberOfPages)
        {
            var button = new Button
            {
                Content = pageNumber,
                Style = FindNumberPanelButtonStyle()
            };

            button.Click += NumberPanelButtonClicked;
            AutomationProperties.SetName(button, ResourceAccessor.GetLocalizedStringResource(SR_PagerControlPageTextName) + " " + pageNumber);
#if NET48_OR_NEWER
            AutomationProperties.SetPositionInSet(button, pageNumber);
            AutomationProperties.SetSizeOfSet(button, numberOfPages);
#endif
            m_numberPanelElements.Add(button);
        }

        private Style FindNumberPanelButtonStyle()
        {
            return m_rootGrid?.TryFindResource(TemplateNumberPanelButtonStyleName) as Style ??
                m_numberPanelRepeater?.TryFindResource(TemplateNumberPanelButtonStyleName) as Style ??
                TryFindResource(NumberPanelButtonStyleName) as Style ??
                m_rootGrid?.TryFindResource(NumberPanelButtonStyleName) as Style ??
                m_numberPanelRepeater?.TryFindResource(NumberPanelButtonStyleName) as Style ??
                Application.Current?.TryFindResource(NumberPanelButtonStyleName) as Style;
        }

        private void AppendEllipsisIconToNumberPanelList()
        {
            m_numberPanelElements.Add(new SymbolIcon(Symbol.More));
        }

        private void OnRootGridKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Left)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                args.Handled = true;
            }
            else if (args.Key == Key.Right)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                args.Handled = true;
            }
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (m_comboBox != null)
            {
                SelectedPageIndex = m_comboBox.SelectedIndex;
            }
        }

        private void NumberBoxValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            SelectedPageIndex = (int)args.NewValue - 1;
        }

        private void FirstButtonClicked(object sender, RoutedEventArgs args)
        {
            SelectedPageIndex = 0;
            ExecuteCommand(FirstButtonCommand);
        }

        private void PreviousButtonClicked(object sender, RoutedEventArgs args)
        {
            SelectedPageIndex--;
            ExecuteCommand(PreviousButtonCommand);
        }

        private void NextButtonClicked(object sender, RoutedEventArgs args)
        {
            SelectedPageIndex++;
            ExecuteCommand(NextButtonCommand);
        }

        private void LastButtonClicked(object sender, RoutedEventArgs args)
        {
            SelectedPageIndex = NumberOfPages - 1;
            ExecuteCommand(LastButtonCommand);
        }

        private void NumberPanelButtonClicked(object sender, RoutedEventArgs args)
        {
            if (sender is Button button && button.Content is int pageNumber)
            {
                SelectedPageIndex = pageNumber - 1;
            }
        }

        private static void ExecuteCommand(ICommand command)
        {
            if (command?.CanExecute(null) == true)
            {
                command.Execute(null);
            }
        }

        private void UnhookTemplateEvents()
        {
            if (m_rootGrid != null)
            {
                m_rootGrid.KeyDown -= OnRootGridKeyDown;
            }

            if (m_comboBox != null)
            {
                m_comboBox.SelectionChanged -= ComboBoxSelectionChanged;
            }

            if (m_numberBox != null)
            {
                m_numberBox.ValueChanged -= NumberBoxValueChanged;
            }

            if (m_firstPageButton != null)
            {
                m_firstPageButton.Click -= FirstButtonClicked;
            }

            if (m_previousPageButton != null)
            {
                m_previousPageButton.Click -= PreviousButtonClicked;
            }

            if (m_nextPageButton != null)
            {
                m_nextPageButton.Click -= NextButtonClicked;
            }

            if (m_lastPageButton != null)
            {
                m_lastPageButton.Click -= LastButtonClicked;
            }
        }

        private int m_lastSelectedPageIndex = -1;
        private int m_lastNumberOfPagesCount;
        private bool m_templateApplied;

        private FrameworkElement m_rootGrid;
        private ComboBox m_comboBox;
        private NumberBox m_numberBox;
        private ItemsRepeater m_numberPanelRepeater;
        private FrameworkElement m_selectedPageIndicator;
        private Button m_firstPageButton;
        private Button m_previousPageButton;
        private Button m_nextPageButton;
        private Button m_lastPageButton;

        private readonly ObservableCollection<object> m_comboBoxEntries;
        private readonly ObservableCollection<object> m_numberPanelElements;
    }
}
