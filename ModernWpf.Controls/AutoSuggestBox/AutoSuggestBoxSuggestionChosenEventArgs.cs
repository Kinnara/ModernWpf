using System.Windows;

namespace ModernWpf.Controls
{
    public sealed class AutoSuggestBoxSuggestionChosenEventArgs : DependencyObject
    {
        public AutoSuggestBoxSuggestionChosenEventArgs()
        {
        }

        public object SelectedItem { get; internal set; }
    }
}
