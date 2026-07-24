using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public sealed partial class AutoSuggestBox : ItemsControl
    {
        const string c_popupName = "SuggestionsPopup";
        const string c_popupBorderName = "SuggestionsContainer";
        const string c_textBoxName = "TextBox";
        const string c_textBoxBorderName = "BorderElement";
        const string c_suggestionsListName = "SuggestionsList";
        const string c_controlCornerRadiusKey = "ControlCornerRadius";
        const string c_overlayCornerRadiusKey = "OverlayCornerRadius";

        static AutoSuggestBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoSuggestBox), new FrameworkPropertyMetadata(typeof(AutoSuggestBox)));
        }

        public AutoSuggestBox()
        {
            m_delayTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
            m_delayTimer.Tick += OnDelayTimerTick;
        }

        public override void OnApplyTemplate()
        {
            if (m_textBox != null)
            {
                m_textBox.TextChanged -= OnTextBoxTextChanged;
                m_textBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
            }

            if (m_queryButton != null)
            {
                m_queryButton.Click -= OnQueryButtonClick;
                m_queryButton.ClearValue(ContentControl.ContentProperty);
                m_queryButton = null;
            }

            if (m_suggestionsPopup != null)
            {
                m_suggestionsPopup.Opened -= OnSuggestionsPopupOpened;
                m_suggestionsPopup.Closed -= OnSuggestionsPopupClosed;
                m_suggestionsPopup.ClearValue(Popup.PlacementTargetProperty);
            }

            if (m_popupRepositionHelper != null)
            {
                m_popupRepositionHelper.Dispose();
                m_popupRepositionHelper = null;
            }

            if (m_suggestionsList != null)
            {
                m_suggestionsList.Loaded -= OnSuggestionsListLoaded;
                m_suggestionsList.SelectionChanged -= OnSuggestionsListSelectionChanged;
                m_suggestionsList.ItemClick -= OnSuggestionsListItemClick;
                m_suggestionsList.PreviewKeyDown -= OnSuggestionsListPreviewKeyDown;
            }

            base.OnApplyTemplate();

            m_textBox = GetTemplateChild(c_textBoxName) as TextBox;
            m_suggestionsPopup = GetTemplateChild(c_popupName) as Popup;
            m_suggestionsList = GetTemplateChild(c_suggestionsListName) as AutoSuggestBoxListView;

            if (m_textBox != null)
            {
                BindCornerRadius(m_textBox);
                m_textBox.ApplyTemplate();
                var deleteButton = m_textBox.GetTemplateChild<Button>("DeleteButton");
                m_queryButton = m_textBox.GetTemplateChild<Button>("QueryButton");

                BindCornerRadius(deleteButton);
                BindCornerRadius(m_queryButton);

                m_textBox.TextChanged += OnTextBoxTextChanged;
                m_textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;

                UpdateTextBoxText(Text, AutoSuggestionBoxTextChangeReason.ProgrammaticChange);
            }

            if (m_queryButton != null)
            {
                m_queryButton.Click += OnQueryButtonClick;
                OnQueryIconChanged(null, QueryIcon);
            }

            if (m_suggestionsPopup != null)
            {
                m_suggestionsPopup.Opened += OnSuggestionsPopupOpened;
                m_suggestionsPopup.Closed += OnSuggestionsPopupClosed;
                m_popupRepositionHelper = new PopupRepositionHelper(m_suggestionsPopup, this);

                if (m_textBox != null)
                {
                    var textBoxBorder = m_textBox.GetTemplateChild<FrameworkElement>(c_textBoxBorderName);
                    if (textBoxBorder != null)
                    {
                        m_suggestionsPopup.PlacementTarget = textBoxBorder;
                    }
                }
            }

            if (m_suggestionsList != null)
            {
                m_suggestionsList.Loaded += OnSuggestionsListLoaded;
                m_suggestionsList.SelectionChanged += OnSuggestionsListSelectionChanged;
                m_suggestionsList.ItemClick += OnSuggestionsListItemClick;
                m_suggestionsList.PreviewKeyDown += OnSuggestionsListPreviewKeyDown;
            }
        }

        private void BindCornerRadius(Control control)
        {
            control?.SetBinding(System.Windows.Controls.Border.CornerRadiusProperty, new Binding(nameof(CornerRadius)) { Source = this, Mode = BindingMode.OneWay });
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (IsTextBoxFocused())
            {
                BeginDeferredSuggestionListUpdate();
                UpdateSuggestionListVisibility();
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            ClearSelection();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            m_textBox?.Focus();

            if (IsTextBoxFocused() && !string.IsNullOrEmpty(Text) && !m_suppressSuggestionListVisibility)
            {
                UpdateSuggestionListVisibility();
                m_suppressSuggestionListVisibility = false;
            }
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            if (!(bool)e.NewValue)
            {
                m_suppressSuggestionListVisibility = false;
                CloseSuggestionList();
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AutoSuggestBoxAutomationPeer(this);
        }

        private void OnTextChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateTextBoxText((string)args.NewValue, AutoSuggestionBoxTextChangeReason.ProgrammaticChange);
        }

        private void OnIsSuggestionListOpenChanged(DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue)
            {
                if (!m_isUpdatingSuggestionListVisibility)
                {
                    m_textBox?.Focus();
                }
            }
            else
            {
                ClearSelection();
            }
        }

        private void OnTextMemberPathChanged()
        {
            // WinUI releases its cached PropertyPathListener when TextMemberPath changes.
        }

        private void OnQueryIconChanged(DependencyPropertyChangedEventArgs args)
        {
            OnQueryIconChanged(args.OldValue as IconElement, args.NewValue as IconElement);
        }

        private void OnQueryIconChanged(IconElement oldQueryIcon, IconElement newQueryIcon)
        {
            if (oldQueryIcon != null)
            {
                oldQueryIcon.ClearValue(IconElement.ForegroundProperty);

                if (newQueryIcon is SymbolIcon)
                {
                    oldQueryIcon.ClearValue(SymbolIcon.FontSizeProperty);
                }
            }

            if (newQueryIcon != null && m_queryButton != null)
            {
                if (newQueryIcon is SymbolIcon)
                {
                    newQueryIcon.SetBinding(SymbolIcon.FontSizeProperty,
                        new Binding
                        {
                            Path = new PropertyPath(TextElement.FontSizeProperty),
                            RelativeSource = new RelativeSource { AncestorType = typeof(ContentPresenter) }
                        });
                }
            }

            UpdateQueryButton();
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var reason = m_textChangeReason;

            m_textChangedCounter++;
            var textChangedArgs = new AutoSuggestBoxTextChangedEventArgs(this, m_textChangedCounter, reason);
            m_delayTimer.Stop();
            m_delayTimer.Tag = textChangedArgs;
            m_delayTimer.Start();

            UpdateText(m_textBox.Text);

            if (!m_ignoreTextChanges)
            {
                if (reason == AutoSuggestionBoxTextChangeReason.UserInput)
                {
                    m_userTypedText = m_textBox.Text;
                    UpdateSuggestionListVisibility();
                }

                if (m_suggestionsList != null && m_suggestionsList.SelectedIndex != -1)
                {
                    m_ignoreSelectionChange = true;
                    m_suggestionsList.SelectedIndex = -1;
                    m_ignoreSelectionChange = false;
                }
            }

            m_textChangeReason = AutoSuggestionBoxTextChangeReason.UserInput;
        }

        private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsSuggestionListOpen)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        if (TryCommitChosenSuggestion() || TryCommitTextBoxText())
                        {
                            e.Handled = true;
                        }
                        break;

                    case Key.Up:
                        MoveSelection(isForward: false);
                        e.Handled = true;
                        break;

                    case Key.Down:
                        if ((ModifierKeys.Alt & Keyboard.Modifiers) == ModifierKeys.None)
                        {
                            if (!TryMoveCaretToEnd())
                            {
                                MoveSelection(isForward: true);
                            }
                            e.Handled = true;
                        }
                        break;

                    case Key.Tab:
                        UpdateTextBoxText(m_userTypedText, AutoSuggestionBoxTextChangeReason.ProgrammaticChange);
                        CloseSuggestionList();
                        break;

                    case Key.Escape:
                        if (IsSuggestionListOpen)
                        {
                            UpdateTextBoxText(m_userTypedText, AutoSuggestionBoxTextChangeReason.ProgrammaticChange);
                            TryMoveCaretToEnd();
                            CloseSuggestionList();
                            e.Handled = true;
                        }
                        break;
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (TryCommitTextBoxText())
                {
                    e.Handled = true;
                }
            }
        }

        private void OnQueryButtonClick(object sender, RoutedEventArgs e)
        {
            ProgrammaticSubmitQuery();
        }

        private void OnSuggestionsPopupOpened(object sender, EventArgs e)
        {
            UpdateCornerRadius(/*IsDropDownOpen=*/true);
        }

        private void OnSuggestionsPopupClosed(object sender, EventArgs e)
        {
            UpdateCornerRadius(/*IsDropDownOpen=*/false);
        }

        private void OnSuggestionsListLoaded(object sender, RoutedEventArgs e)
        {
            ClearSelection();
            m_suggestionsList.ScrollToTop();
        }

        private void OnSuggestionsListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollSelectedSuggestionIntoView();

            if (m_ignoreSelectionChange)
            {
                return;
            }

            m_ignoreTextChanges = true;

            if (IsSuggestionListOpen && m_suggestionsList.SelectedItem != null)
            {
                var selectedItem = m_suggestionsList.SelectedItem;

                if (UpdateTextOnSelect)
                {
                    var selectedValue = GetSuggestionText(selectedItem);
                    if (selectedValue != null)
                    {
                        UpdateTextBoxText(selectedValue, AutoSuggestionBoxTextChangeReason.SuggestionChosen);
                    }
                }

                SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs { SelectedItem = selectedItem });

                if (m_textBox != null)
                {
                    m_textBox.CaretIndex = m_textBox.Text.Length;
                }
            }

            Dispatcher.BeginInvoke(
                new Action(() => m_ignoreTextChanges = false),
                DispatcherPriority.Background);
        }

        private void OnSuggestionsListItemClick(object sender, ItemClickEventArgs e)
        {
            m_suggestionsList.SelectedItem = e.ClickedItem;
            Dispatcher.BeginInvoke(
                new Action(() => SubmitQuery(e.ClickedItem)),
                DispatcherPriority.Background);
        }

        private void OnSuggestionsListPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    if (m_suggestionsList != null)
                    {
                        int selectedIndex = m_suggestionsList.SelectedIndex;
                        int lastIndex = m_suggestionsList.Items.Count - 1;

                        if ((selectedIndex == 0 && e.Key == Key.Up) ||
                            (selectedIndex == lastIndex && e.Key == Key.Down))
                        {
                            UpdateTextBoxText(m_userTypedText, AutoSuggestionBoxTextChangeReason.ProgrammaticChange);
                            m_suggestionsList.SelectedIndex = -1;
                            m_textBox?.Focus();
                            e.Handled = true;
                        }
                    }
                    break;

                case Key.Enter:
                case Key.Space:
                    SubmitQuery(m_suggestionsList?.SelectedItem);
                    e.Handled = true;
                    break;

                case Key.Escape:
                    UpdateTextBoxText(m_userTypedText, AutoSuggestionBoxTextChangeReason.ProgrammaticChange);
                    CloseSuggestionList();
                    m_textBox?.Focus();
                    e.Handled = true;
                    break;
            }
        }

        private void OnDelayTimerTick(object sender, EventArgs e)
        {
            m_delayTimer.Stop();

            if (m_delayTimer.Tag is AutoSuggestBoxTextChangedEventArgs args)
            {
                m_delayTimer.Tag = null;
                TextChanged?.Invoke(this, args);
            }
        }

        private void UpdateText(string value)
        {
            if (Text != value)
            {
                SetCurrentValue(TextProperty, value);
            }
        }

        private void UpdateTextBoxText(string value, AutoSuggestionBoxTextChangeReason reason)
        {
            value ??= string.Empty;

            if (m_textBox != null && m_textBox.Text != value)
            {
                var previousReason = m_textChangeReason;
                m_textChangeReason = reason;
                try
                {
                    m_textBox.Text = value;
                    m_textBox.CaretIndex = m_textBox.Text.Length;
                }
                finally
                {
                    if (m_textChangeReason == reason)
                    {
                        m_textChangeReason = previousReason;
                    }
                }
            }
        }

        private void UpdateQueryButton()
        {
            if (m_queryButton != null)
            {
                var icon = QueryIcon;
                m_queryButton.Content = icon;
                m_queryButton.Visibility = icon != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void OpenSuggestionList()
        {
            if (!IsSuggestionListOpen)
            {
                SetCurrentValue(IsSuggestionListOpenProperty, true);
            }
        }

        private void CloseSuggestionList()
        {
            if (IsSuggestionListOpen)
            {
                SetCurrentValue(IsSuggestionListOpenProperty, false);
            }
        }

        private void UpdateSuggestionListVisibility()
        {
            bool shouldOpen = m_suggestionsList != null &&
                m_suggestionsList.Items.Count > 0 &&
                GetEffectiveMaxSuggestionListHeight() > 0;

            m_isUpdatingSuggestionListVisibility = true;
            try
            {
                if (shouldOpen)
                {
                    OpenSuggestionList();
                }
                else
                {
                    CloseSuggestionList();
                }
            }
            finally
            {
                m_isUpdatingSuggestionListVisibility = false;
            }
        }

        private void BeginDeferredSuggestionListUpdate()
        {
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    UpdateSuggestionListSize();
                    UpdateSuggestionListVisibility();
                }),
                DispatcherPriority.Background);
        }

        private void MoveSelection(bool isForward)
        {
            if (m_suggestionsList != null && m_suggestionsList.Items.Count > 0)
            {
                int selectedIndex = m_suggestionsList.SelectedIndex;
                int lastIndex = m_suggestionsList.Items.Count - 1;

                if (selectedIndex == -1)
                {
                    m_suggestionsList.SelectedIndex = isForward ? 0 : lastIndex;
                }
                else if (selectedIndex == 0)
                {
                    if (isForward)
                    {
                        m_suggestionsList.SelectedIndex = selectedIndex + 1;
                    }
                    else
                    {
                        UpdateTextBoxText(m_userTypedText, AutoSuggestionBoxTextChangeReason.ProgrammaticChange);
                        m_suggestionsList.SelectedIndex = -1;
                    }
                }
                else if (selectedIndex == lastIndex)
                {
                    if (isForward)
                    {
                        UpdateTextBoxText(m_userTypedText, AutoSuggestionBoxTextChangeReason.ProgrammaticChange);
                        m_suggestionsList.SelectedIndex = -1;
                    }
                    else
                    {
                        m_suggestionsList.SelectedIndex = lastIndex - 1;
                    }
                }
                else
                {
                    m_suggestionsList.SelectedIndex = isForward ? selectedIndex + 1 : selectedIndex - 1;
                }
            }
        }

        private void ClearSelection()
        {
            if (m_suggestionsList != null)
            {
                m_ignoreSelectionChange = true;
                m_suggestionsList.ClearValue(Selector.SelectedItemProperty);
                m_suggestionsList.ClearValue(Selector.SelectedIndexProperty);
                m_ignoreSelectionChange = false;
            }
        }

        internal void ProgrammaticSubmitQuery()
        {
            m_ignoreSelectionChange = true;
            try
            {
                if (m_suggestionsList != null)
                {
                    m_suggestionsList.SelectedIndex = -1;
                }
            }
            finally
            {
                m_ignoreSelectionChange = false;
            }

            SubmitQuery(null);
        }

        private bool TryCommitChosenSuggestion()
        {
            if (IsSuggestionListOpen && m_textBox != null && m_suggestionsList != null)
            {
                var selectedItem = m_suggestionsList.SelectedItem;
                if (selectedItem != null)
                {
                    SubmitQuery(selectedItem);
                    return true;
                }
            }
            return false;
        }

        private bool TryCommitTextBoxText()
        {
            if (m_textBox != null)
            {
                SubmitQuery(null);
                return true;
            }
            return false;
        }

        private void SubmitQuery(object chosenSuggestion)
        {
            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs
            {
                QueryText = m_textBox?.Text ?? Text,
                ChosenSuggestion = chosenSuggestion
            });

            CloseSuggestionList();
        }

        private bool TryMoveCaretToEnd()
        {
            if (m_textBox != null)
            {
                int textLength = m_textBox.Text.Length;
                if (m_textBox.CaretIndex < textLength)
                {
                    m_textBox.CaretIndex = textLength;
                    return true;
                }
            }
            return false;
        }

        private void UpdateCornerRadius(bool isPopupOpen)
        {
            var textBoxRadius = CornerRadius;
            var popupRadius = (CornerRadius)ResourceLookup(c_overlayCornerRadiusKey);

            if (isPopupOpen)
            {
                bool isOpenDown = IsPopupOpenDown();
                var cornerRadiusConverter = new CornerRadiusFilterConverter();

                var popupRadiusFilter = isOpenDown ? CornerRadiusFilterKind.Bottom : CornerRadiusFilterKind.Top;
                popupRadius = cornerRadiusConverter.Convert(popupRadius, popupRadiusFilter);

                var textBoxRadiusFilter = isOpenDown ? CornerRadiusFilterKind.Top : CornerRadiusFilterKind.Bottom;
                textBoxRadius = cornerRadiusConverter.Convert(textBoxRadius, textBoxRadiusFilter);
            }

            if (GetTemplateChild(c_popupBorderName) is Border popupBorder)
            {
                popupBorder.CornerRadius = popupRadius;
            }

            if (GetTemplateChild(c_textBoxName) is TextBox textBox)
            {
                textBox.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, textBoxRadius);
            }
        }

        private bool IsPopupOpenDown()
        {
            double verticalOffset = 0;
            if (GetTemplateChild(c_popupBorderName) is Border popupBorder)
            {
                if (GetTemplateChild(c_textBoxName) is TextBox textBox)
                {
                    var popupTop = popupBorder.TranslatePoint(new Point(0, 0), textBox);
                    verticalOffset = popupTop.Y;
                }
            }
            return verticalOffset >= 0;
        }

        private object ResourceLookup(object key)
        {
            return TryFindResource(key);
        }

        private bool IsTextBoxFocused()
        {
            return m_textBox != null && m_textBox.IsKeyboardFocusWithin;
        }

        private double GetEffectiveMaxSuggestionListHeight()
        {
            double maxHeight = MaxSuggestionListHeight;

            if (m_suggestionsPopup != null &&
                m_suggestionsPopup.Child is FrameworkElement popupChild &&
                popupChild.MaxHeight > 0 &&
                popupChild.MaxHeight < maxHeight)
            {
                maxHeight = popupChild.MaxHeight;
            }

            return maxHeight;
        }

        private void UpdateSuggestionListSize()
        {
            if (GetTemplateChild(c_popupBorderName) is FrameworkElement suggestionsContainer)
            {
                suggestionsContainer.Width = ActualWidth;
                suggestionsContainer.MaxHeight = MaxSuggestionListHeight;
            }
        }

        private void ScrollSelectedSuggestionIntoView()
        {
            if (m_suggestionsList == null)
            {
                return;
            }

            var scrollToItem = m_suggestionsList.SelectedItem;
            if (scrollToItem == null && m_suggestionsList.Items.Count > 0)
            {
                scrollToItem = m_suggestionsList.Items[0];
            }

            if (scrollToItem != null)
            {
                m_suggestionsList.ScrollIntoView(scrollToItem);
            }
        }

        private string GetSuggestionText(object selectedItem)
        {
            if (selectedItem == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(TextMemberPath))
            {
                var descriptor = TypeDescriptor.GetProperties(selectedItem)[TextMemberPath];
                if (descriptor != null)
                {
                    return descriptor.GetValue(selectedItem)?.ToString();
                }
            }

            return selectedItem.ToString();
        }

        internal uint TextChangedEventCounter => m_textChangedCounter;

        private TextBox m_textBox;
        private Button m_queryButton;
        private Popup m_suggestionsPopup;
        private AutoSuggestBoxListView m_suggestionsList;
        private PopupRepositionHelper m_popupRepositionHelper;
        private string m_userTypedText = string.Empty;
        private readonly DispatcherTimer m_delayTimer;
        private AutoSuggestionBoxTextChangeReason m_textChangeReason = AutoSuggestionBoxTextChangeReason.UserInput;
        private uint m_textChangedCounter;
        private bool m_ignoreTextChanges;
        private bool m_ignoreSelectionChange;
        private bool m_isUpdatingSuggestionListVisibility;
        private bool m_suppressSuggestionListVisibility;
    }
}
