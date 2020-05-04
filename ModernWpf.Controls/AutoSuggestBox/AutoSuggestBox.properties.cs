using System.Windows;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    partial class AutoSuggestBox
    {
        #region UpdateTextOnSelect

        public static readonly DependencyProperty UpdateTextOnSelectProperty =
            DependencyProperty.Register(
                nameof(UpdateTextOnSelect),
                typeof(bool),
                typeof(AutoSuggestBox),
                new PropertyMetadata(true));

        public bool UpdateTextOnSelect
        {
            get => (bool)GetValue(UpdateTextOnSelectProperty);
            set => SetValue(UpdateTextOnSelectProperty, value);
        }

        #endregion

        #region TextMemberPath

        public static readonly DependencyProperty TextMemberPathProperty =
            DependencyProperty.Register(
                nameof(TextMemberPath),
                typeof(string),
                typeof(AutoSuggestBox),
                new PropertyMetadata(string.Empty));

        public string TextMemberPath
        {
            get => (string)GetValue(TextMemberPathProperty);
            set => SetValue(TextMemberPathProperty, value);
        }

        #endregion

        #region TextBoxStyle

        public static readonly DependencyProperty TextBoxStyleProperty =
            DependencyProperty.Register(
                nameof(TextBoxStyle),
                typeof(Style),
                typeof(AutoSuggestBox),
                null);

        public Style TextBoxStyle
        {
            get => (Style)GetValue(TextBoxStyleProperty);
            set => SetValue(TextBoxStyleProperty, value);
        }

        #endregion

        #region Text

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(AutoSuggestBox),
                new PropertyMetadata(string.Empty, OnTextPropertyChanged, CoerceText));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((AutoSuggestBox)sender).OnTextChanged(args);
        }

        private static object CoerceText(DependencyObject d, object baseValue)
        {
            return baseValue ?? string.Empty;
        }

        #endregion

        #region PlaceholderText

        public static readonly DependencyProperty PlaceholderTextProperty =
            ControlHelper.PlaceholderTextProperty.AddOwner(typeof(AutoSuggestBox));

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        #endregion

        #region MaxSuggestionListHeight

        public static readonly DependencyProperty MaxSuggestionListHeightProperty =
            DependencyProperty.Register(
                nameof(MaxSuggestionListHeight),
                typeof(double),
                typeof(AutoSuggestBox),
                new PropertyMetadata(double.PositiveInfinity));

        public double MaxSuggestionListHeight
        {
            get => (double)GetValue(MaxSuggestionListHeightProperty);
            set => SetValue(MaxSuggestionListHeightProperty, value);
        }

        #endregion

        #region IsSuggestionListOpen

        public static readonly DependencyProperty IsSuggestionListOpenProperty =
            DependencyProperty.Register(
                nameof(IsSuggestionListOpen),
                typeof(bool),
                typeof(AutoSuggestBox),
                new PropertyMetadata(false, OnIsSuggestionListOpenPropertyChanged));

        public bool IsSuggestionListOpen
        {
            get => (bool)GetValue(IsSuggestionListOpenProperty);
            set => SetValue(IsSuggestionListOpenProperty, value);
        }

        private static void OnIsSuggestionListOpenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((AutoSuggestBox)sender).OnIsSuggestionListOpenChanged(args);
        }

        #endregion

        #region Header

        public static readonly DependencyProperty HeaderProperty =
            ControlHelper.HeaderProperty.AddOwner(typeof(AutoSuggestBox));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region QueryIcon

        public static readonly DependencyProperty QueryIconProperty =
            DependencyProperty.Register(
                nameof(QueryIcon),
                typeof(IconElement),
                typeof(AutoSuggestBox),
                new PropertyMetadata(null, OnQueryIconPropertyChanged));

        public IconElement QueryIcon
        {
            get => (IconElement)GetValue(QueryIconProperty);
            set => SetValue(QueryIconProperty, value);
        }

        private static void OnQueryIconPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((AutoSuggestBox)sender).OnQueryIconChanged(args);
        }

        #endregion

        #region Description

        public static readonly DependencyProperty DescriptionProperty =
            ControlHelper.DescriptionProperty.AddOwner(typeof(AutoSuggestBox));

        public object Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        #endregion

        #region UseSystemFocusVisuals

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(AutoSuggestBox));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(AutoSuggestBox));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;

        public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxTextChangedEventArgs> TextChanged;

        public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;
    }
}
