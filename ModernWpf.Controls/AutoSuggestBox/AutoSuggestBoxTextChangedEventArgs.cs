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
            Reason = AutoSuggestionBoxTextChangeReason.ProgrammaticChange;
        }

        internal AutoSuggestBoxTextChangedEventArgs(AutoSuggestBox source, uint counter, AutoSuggestionBoxTextChangeReason reason)
        {
            m_source = new WeakReference<AutoSuggestBox>(source);
            m_counter = counter;
            Reason = reason;
        }
        
        public AutoSuggestionBoxTextChangeReason Reason { get; }
        
        public bool CheckCurrent()
        {
            return m_source != null &&
                   m_source.TryGetTarget(out var source) &&
                   source.TextChangedEventCounter == m_counter;
        }

        private readonly WeakReference<AutoSuggestBox> m_source;
        private readonly uint m_counter;
    }
}
