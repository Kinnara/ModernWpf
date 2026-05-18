using System.Windows;

namespace ModernWpf.Controls
{
    partial class AutoSuggestBox
    {
        public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;

        public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxTextChangedEventArgs> TextChanged;

        public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;

        private static object CoerceTextMemberPath(DependencyObject d, object baseValue)
        {
            return baseValue ?? string.Empty;
        }

        private static object CoerceText(DependencyObject d, object baseValue)
        {
            return baseValue ?? string.Empty;
        }
    }
}
