using System;

namespace ModernWpf.Controls
{
    public enum AutoSuggestionBoxTextChangeReason
    {
        UserInput = 0,
        ProgrammaticChange = 1,
        SuggestionChosen = 2
    }

    public sealed class AutoSuggestBoxTextChangedEventArgs : EventArgs
    {
        public AutoSuggestBoxTextChangedEventArgs()
        {
        }

        internal AutoSuggestBoxTextChangedEventArgs(AutoSuggestBox source, string value, AutoSuggestionBoxTextChangeReason reason)
        {
            m_source = new WeakReference<AutoSuggestBox>(source);
            m_value = value;
            Reason = reason;
        }

        public AutoSuggestionBoxTextChangeReason Reason { get; private set; }

        public bool CheckCurrent()
        {
            return m_source != null &&
                   m_source.TryGetTarget(out var source) &&
                   source.Text == m_value;
        }

        private readonly WeakReference<AutoSuggestBox> m_source;
        private readonly string m_value;
    }
}
