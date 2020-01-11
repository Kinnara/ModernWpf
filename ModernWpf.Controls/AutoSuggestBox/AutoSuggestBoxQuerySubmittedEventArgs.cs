using System.Windows;

namespace ModernWpf.Controls
{
    public sealed class AutoSuggestBoxQuerySubmittedEventArgs : DependencyObject
    {
        public AutoSuggestBoxQuerySubmittedEventArgs()
        {
        }

        public object ChosenSuggestion { get; internal set; }
        public string QueryText { get; internal set; } = string.Empty;
    }
}
