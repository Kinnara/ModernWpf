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
    [TemplatePart(Name = RootPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = NumberPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = FirstButtonName, Type = typeof(Button))]
    [TemplatePart(Name = PreviousButtonName, Type = typeof(Button))]
    [TemplatePart(Name = NextButtonName, Type = typeof(Button))]
    [TemplatePart(Name = LastButtonName, Type = typeof(Button))]
    public class PagerControl : Control
    {
        private const string RootPanelName = "PART_RootPanel";
        private const string NumberPanelName = "PART_NumberPanel";
        private const string FirstButtonName = "PART_FirstButton";
        private const string PreviousButtonName = "PART_PreviousButton";
        private const string NextButtonName = "PART_NextButton";
        private const string LastButtonName = "PART_LastButton";
        private const int InfiniteModeComboBoxItemsIncrement = 100;

        static PagerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PagerControl), new FrameworkPropertyMetadata(typeof(PagerControl)));
        }

        public PagerControl()
        {
            SetValue(TemplateSettingsPropertyKey, new PagerControlTemplateSettings());
            Loaded += OnLoaded;
            UpdateTemplateSettingElementLists();
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
                new FrameworkPropertyMetadata(null, OnPagerPropertyChanged));

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
                new FrameworkPropertyMetadata(null, OnPagerPropertyChanged));

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
                new FrameworkPropertyMetadata(null, OnPagerPropertyChanged));

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
                new FrameworkPropertyMetadata(null, OnPagerPropertyChanged));

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
                new FrameworkPropertyMetadata(null, OnPagerPropertyChanged));

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
                new FrameworkPropertyMetadata(null, OnPagerPropertyChanged));

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
                typeof(PagerControl),
                new FrameworkPropertyMetadata(null, OnPagerPropertyChanged));

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
                new FrameworkPropertyMetadata(null, OnPagerPropertyChanged));

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
                new FrameworkPropertyMetadata(string.Empty, OnPagerPropertyChanged));

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
                new FrameworkPropertyMetadata(string.Empty, OnPagerPropertyChanged));

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
            UnhookButtonHandlers();

            base.OnApplyTemplate();

            _rootPanel = GetTemplateChild(RootPanelName) as Panel;
            _numberPanel = GetTemplateChild(NumberPanelName) as Panel;
            _firstButton = GetTemplateChild(FirstButtonName) as Button;
            _previousButton = GetTemplateChild(PreviousButtonName) as Button;
            _nextButton = GetTemplateChild(NextButtonName) as Button;
            _lastButton = GetTemplateChild(LastButtonName) as Button;

            HookButtonHandlers();
            UpdateVisuals();
        }

        internal Button ContainerFromPageIndex(int pageIndex)
        {
            _pageButtonsByPageIndex.TryGetValue(pageIndex, out var button);
            return button;
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
            ((PagerControl)d).UpdateVisuals();
        }

        private static void OnNumberOfPagesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pager = (PagerControl)d;
            pager.CoerceValue(SelectedPageIndexProperty);
            pager.UpdateVisuals();
        }

        private static void OnSelectedPageIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pager = (PagerControl)d;
            var previousIndex = (int)e.OldValue;
            pager.UpdateVisuals();
            pager.RaiseSelectedIndexChanged(previousIndex, pager.SelectedPageIndex);
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_hasRaisedInitialSelectedIndexChanged)
            {
                _hasRaisedInitialSelectedIndexChanged = true;
                RaiseSelectedIndexChanged(-1, SelectedPageIndex);
            }
        }

        private void HookButtonHandlers()
        {
            if (_firstButton != null)
            {
                _firstButton.Click += OnFirstButtonClick;
            }

            if (_previousButton != null)
            {
                _previousButton.Click += OnPreviousButtonClick;
            }

            if (_nextButton != null)
            {
                _nextButton.Click += OnNextButtonClick;
            }

            if (_lastButton != null)
            {
                _lastButton.Click += OnLastButtonClick;
            }
        }

        private void UnhookButtonHandlers()
        {
            if (_firstButton != null)
            {
                _firstButton.Click -= OnFirstButtonClick;
            }

            if (_previousButton != null)
            {
                _previousButton.Click -= OnPreviousButtonClick;
            }

            if (_nextButton != null)
            {
                _nextButton.Click -= OnNextButtonClick;
            }

            if (_lastButton != null)
            {
                _lastButton.Click -= OnLastButtonClick;
            }
        }

        private void OnFirstButtonClick(object sender, RoutedEventArgs e)
        {
            SelectedPageIndex = 0;
            ExecuteCommand(FirstButtonCommand);
        }

        private void OnPreviousButtonClick(object sender, RoutedEventArgs e)
        {
            if (SelectedPageIndex > 0)
            {
                SelectedPageIndex--;
            }

            ExecuteCommand(PreviousButtonCommand);
        }

        private void OnNextButtonClick(object sender, RoutedEventArgs e)
        {
            if (NumberOfPages < 0 || SelectedPageIndex < NumberOfPages - 1)
            {
                SelectedPageIndex++;
            }

            ExecuteCommand(NextButtonCommand);
        }

        private void OnLastButtonClick(object sender, RoutedEventArgs e)
        {
            if (NumberOfPages > 0)
            {
                SelectedPageIndex = NumberOfPages - 1;
            }

            ExecuteCommand(LastButtonCommand);
        }

        private void OnPageButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int pageIndex)
            {
                SelectedPageIndex = pageIndex;
            }
        }

        private static void ExecuteCommand(ICommand command)
        {
            if (command?.CanExecute(null) == true)
            {
                command.Execute(null);
            }
        }

        private void RaiseSelectedIndexChanged(int previousPageIndex, int newPageIndex)
        {
            SelectedIndexChanged?.Invoke(this, new PagerControlSelectedIndexChangedEventArgs(previousPageIndex, newPageIndex));
        }

        private void UpdateVisuals()
        {
            UpdateTemplateSettingElementLists();
            UpdateNavigationButton(_firstButton, FirstButtonStyle, FirstButtonVisibility, SelectedPageIndex != 0);
            UpdateNavigationButton(_previousButton, PreviousButtonStyle, PreviousButtonVisibility, SelectedPageIndex != 0);
            UpdateNavigationButton(_nextButton, NextButtonStyle, NextButtonVisibility, NumberOfPages < 0 || SelectedPageIndex < NumberOfPages - 1);
            UpdateNavigationButton(_lastButton, LastButtonStyle, LastButtonVisibility, NumberOfPages > 0 && SelectedPageIndex < NumberOfPages - 1);
            UpdateNumberPanelButtons();
        }

        private void UpdateTemplateSettingElementLists()
        {
            var pages = TemplateSettings.Pages;
            if (NumberOfPages >= 0)
            {
                FillCollectionToSize(pages, NumberOfPages);
            }
            else if (pages.Count < InfiniteModeComboBoxItemsIncrement)
            {
                FillCollectionToSize(pages, InfiniteModeComboBoxItemsIncrement);
            }

            var numberPanelItems = TemplateSettings.NumberPanelItems;
            numberPanelItems.Clear();
            foreach (var pageNumber in GetNumberPanelPageNumbers())
            {
                numberPanelItems.Add(pageNumber);
            }
        }

        private static void FillCollectionToSize(IList<object> collection, int numberOfPages)
        {
            while (collection.Count < numberOfPages)
            {
                collection.Add(collection.Count + 1);
            }

            while (collection.Count > numberOfPages)
            {
                collection.RemoveAt(collection.Count - 1);
            }
        }

        private IEnumerable<int> GetNumberPanelPageNumbers()
        {
            if (NumberOfPages > 0)
            {
                for (var i = 1; i <= NumberOfPages; i++)
                {
                    yield return i;
                }
            }
            else if (NumberOfPages < 0)
            {
                var start = Math.Max(1, SelectedPageIndex - 1);
                for (var i = start; i < start + 5; i++)
                {
                    yield return i;
                }
            }
        }

        private void UpdateNavigationButton(Button button, Style style, PagerControlButtonVisibility buttonVisibility, bool isEnabled)
        {
            if (button == null)
            {
                return;
            }

            if (style != null)
            {
                button.Style = style;
            }

            button.Visibility = GetButtonVisibility(buttonVisibility, isEnabled);
            button.IsEnabled = isEnabled;
        }

        private static Visibility GetButtonVisibility(PagerControlButtonVisibility buttonVisibility, bool isEnabled)
        {
            if (buttonVisibility == PagerControlButtonVisibility.Hidden)
            {
                return Visibility.Collapsed;
            }

            if (buttonVisibility == PagerControlButtonVisibility.HiddenOnEdge && !isEnabled)
            {
                return Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        private void UpdateNumberPanelButtons()
        {
            if (_numberPanel == null)
            {
                return;
            }

            foreach (var button in _pageButtonsByPageIndex.Values)
            {
                button.Click -= OnPageButtonClick;
            }

            _pageButtonsByPageIndex.Clear();
            _numberPanel.Children.Clear();

            var sizeOfSet = NumberOfPages > 0 ? NumberOfPages : 0;
            foreach (var item in TemplateSettings.NumberPanelItems)
            {
                var pageNumber = (int)item;
                var pageIndex = pageNumber - 1;
                var button = new Button
                {
                    Content = pageNumber.ToString(),
                    Tag = pageIndex,
                    MinWidth = 28,
                    Margin = new Thickness(2),
                    Padding = new Thickness(4, 2, 4, 2),
                    FontWeight = pageIndex == SelectedPageIndex ? FontWeights.SemiBold : FontWeights.Normal
                };

                AutomationProperties.SetName(button, $"Page {pageNumber}");
#if NET48_OR_NEWER
                AutomationProperties.SetPositionInSet(button, pageNumber);
                AutomationProperties.SetSizeOfSet(button, sizeOfSet);
#endif
                button.Click += OnPageButtonClick;
                _pageButtonsByPageIndex[pageIndex] = button;
                _numberPanel.Children.Add(button);
            }
        }

        private Panel _rootPanel;
        private Panel _numberPanel;
        private Button _firstButton;
        private Button _previousButton;
        private Button _nextButton;
        private Button _lastButton;
        private bool _hasRaisedInitialSelectedIndexChanged;
        private readonly Dictionary<int, Button> _pageButtonsByPageIndex = new Dictionary<int, Button>();
    }
}
