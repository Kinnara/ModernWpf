using System;

namespace ModernWpf.Controls
{
    public sealed class AutoSuggestBoxSuggestionChosenEventArgs : EventArgs
    {
        public AutoSuggestBoxSuggestionChosenEventArgs()
        {
        }

        public object SelectedItem { get; internal set; }
    }
}
