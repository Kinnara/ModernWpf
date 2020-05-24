using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public sealed partial class AutoSuggestBox : ItemsControl
    {
        const string c_popupName = "SuggestionsPopup";
        const string c_popupBorderName = "SuggestionsContainer";
        const string c_textBoxName = "TextBox";
        const string c_textBoxBorderName = "BorderElement";
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
            }

            base.OnApplyTemplate();

            m_textBox = GetTemplateChild(c_textBoxName) as TextBox;
            m_suggestionsPopup = GetTemplateChild(c_popupName) as Popup;
            m_suggestionsList = GetTemplateChild("SuggestionsList") as AutoSuggestBoxListView;

            if (m_textBox != null)
            {
                m_textBox.ApplyTemplate();
                m_queryButton = m_textBox.GetTemplateChild<Button>("QueryButton");

                m_textBox.TextChanged += OnTextBoxTextChanged;
                m_textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;

                UpdateTextBox();
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
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            OpenOrCloseSuggestionListIfFocused();
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
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            if (!(bool)e.NewValue)
            {
                CloseSuggestionList();
            }
        }

        private void OnTextChanged(DependencyPropertyChangedEventArgs args)
        {
            m_delayTimer.Stop();
            m_delayTimer.Tag = null;

            OpenOrCloseSuggestionListIfFocused();

            if (m_textChangeReason != AutoSuggestionBoxTextChangeReason.SuggestionChosen)
            {
                UpdateSearchText((string)args.NewValue);
            }

            UpdateTextBox();

            m_delayTimer.Tag = m_textChangeReason ?? AutoSuggestionBoxTextChangeReason.ProgrammaticChange;
            m_delayTimer.Start();
        }

        private void OnIsSuggestionListOpenChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!(bool)args.NewValue)
            {
                UpdateSearchText(Text);
                ClearSelection();
            }
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
            if (m_ignoreTextBoxTextChange)
            {
                return;
            }

            UpdateTextValue(m_textBox.Text, AutoSuggestionBoxTextChangeReason.UserInput);
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
                        SelectedIndexDecrement();
                        e.Handled = true;
                        break;

                    case Key.Down:
                        if ((ModifierKeys.Alt & Keyboard.Modifiers) == ModifierKeys.None)
                        {
                            if (!TryMoveCaretToEnd())
                            {
                                SelectedIndexIncrement();
                            }
                            e.Handled = true;
                        }
                        break;

                    case Key.Escape:
                        if (IsSuggestionListOpen)
                        {
                            UpdateTextValue(m_searchText);
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
            TryCommitTextBoxText();
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
            if (m_ignoreSelectionChange)
            {
                return;
            }

            if (IsSuggestionListOpen)
            {
                var selectedItem = m_suggestionsList.SelectedItem;
                if (selectedItem != null)
                {
                    m_suggestionsList.ScrollIntoView(selectedItem);

                    SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs { SelectedItem = selectedItem });

                    if (UpdateTextOnSelect)
                    {
                        var selectedValue = m_suggestionsList.SelectedValue;
                        if (selectedValue != null)
                        {
                            UpdateTextValue(selectedValue.ToString(), AutoSuggestionBoxTextChangeReason.SuggestionChosen);
                        }
                    }
                }
                else
                {
                    m_suggestionsList.ScrollToTop();
                    UpdateTextValue(m_searchText);
                }

                if (m_textBox != null)
                {
                    m_textBox.CaretIndex = m_textBox.Text.Length;
                }
            }
        }

        private void OnSuggestionsListItemClick(object sender, ItemClickEventArgs e)
        {
            m_suggestionsList.SelectedItem = e.ClickedItem;
            TryCommitChosenSuggestion();
        }

        private void OnDelayTimerTick(object sender, EventArgs e)
        {
            m_delayTimer.Stop();

            if (m_delayTimer.Tag is AutoSuggestionBoxTextChangeReason reason)
            {
                m_delayTimer.Tag = null;
                TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(this, Text) { Reason = reason });
            }
        }

        private void UpdateTextValue(string value, AutoSuggestionBoxTextChangeReason reason = AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
        {
            if (Text != value)
            {
                m_textChangeReason = reason;
                SetCurrentValue(TextProperty, value);
                m_textChangeReason = null;
            }
        }

        private void UpdateSearchText(string value)
        {
            if (m_searchText != value)
            {
                m_searchText = value;
            }
        }

        private void UpdateTextBox()
        {
            if (m_textBox != null)
            {
                string text = Text;
                if (m_textBox.Text != text)
                {
                    m_ignoreTextBoxTextChange = true;
                    m_textBox.Text = text;
                    m_ignoreTextBoxTextChange = false;
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

        private void OpenOrCloseSuggestionListIfFocused()
        {
            if (IsKeyboardFocusWithin)
            {
                if (HasItems)
                {
                    OpenSuggestionList();
                }
                else
                {
                    CloseSuggestionList();
                }
            }
        }

        private void SelectedIndexIncrement()
        {
            if (m_suggestionsList != null)
            {
                int index = m_suggestionsList.SelectedIndex;
                m_suggestionsList.SelectedIndex = index + 1 >= m_suggestionsList.Items.Count ? -1 : index + 1;
            }
        }

        private void SelectedIndexDecrement()
        {
            if (m_suggestionsList != null)
            {
                int index = m_suggestionsList.SelectedIndex;
                if (index >= 0)
                {
                    m_suggestionsList.SelectedIndex--;
                }
                else if (index == -1)
                {
                    m_suggestionsList.SelectedIndex = m_suggestionsList.Items.Count - 1;
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

        private bool TryCommitChosenSuggestion()
        {
            if (IsSuggestionListOpen && m_textBox != null && m_suggestionsList != null)
            {
                var selectedItem = m_suggestionsList.SelectedItem;
                if (selectedItem != null)
                {
                    CloseSuggestionList();
                    SubmitQuery(m_textBox.Text, selectedItem);
                    return true;
                }
            }
            return false;
        }

        private bool TryCommitTextBoxText()
        {
            if (m_textBox != null)
            {
                bool shouldCloseSuggestionList = IsSuggestionListOpen;
                SubmitQuery(m_textBox.Text, null);
                if (shouldCloseSuggestionList)
                {
                    CloseSuggestionList();
                }
                return true;
            }
            return false;
        }

        private void SubmitQuery(string queryText, object chosenSuggestion)
        {
            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs
            {
                QueryText = queryText,
                ChosenSuggestion = chosenSuggestion
            });
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
                ControlHelper.SetCornerRadius(textBox, textBoxRadius);
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

        private TextBox m_textBox;
        private Button m_queryButton;
        private Popup m_suggestionsPopup;
        private AutoSuggestBoxListView m_suggestionsList;
        private PopupRepositionHelper m_popupRepositionHelper;
        private string m_searchText = string.Empty;
        private readonly DispatcherTimer m_delayTimer;
        private AutoSuggestionBoxTextChangeReason? m_textChangeReason;
        private bool m_ignoreTextBoxTextChange;
        private bool m_ignoreSelectionChange;
    }
}
