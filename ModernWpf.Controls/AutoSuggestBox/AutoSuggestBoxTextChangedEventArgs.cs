using System;
using System.Windows;

namespace ModernWpf.Controls
{
    public enum AutoSuggestionBoxTextChangeReason
    {
        UserInput = 0,
        ProgrammaticChange = 1,
        SuggestionChosen = 2
    }

    public sealed class AutoSuggestBoxTextChangedEventArgs : DependencyObject
    {
        public AutoSuggestBoxTextChangedEventArgs()
        {
        }

        internal AutoSuggestBoxTextChangedEventArgs(AutoSuggestBox source, string value)
        {
            m_source = new WeakReference<AutoSuggestBox>(source);
            m_value = value;
        }

        #region Reason

        public static readonly DependencyProperty ReasonProperty =
            DependencyProperty.Register(
                nameof(Reason),
                typeof(AutoSuggestionBoxTextChangeReason),
                typeof(AutoSuggestBoxTextChangedEventArgs),
                new PropertyMetadata(AutoSuggestionBoxTextChangeReason.ProgrammaticChange));

        public AutoSuggestionBoxTextChangeReason Reason
        {
            get => (AutoSuggestionBoxTextChangeReason)GetValue(ReasonProperty);
            set => SetValue(ReasonProperty, value);
        }

        #endregion

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
