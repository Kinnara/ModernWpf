using System;
using System.Windows;

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
