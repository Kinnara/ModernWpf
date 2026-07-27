using System.Windows;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Provides event data for the BackRequested event.
    /// </summary>
    public sealed class BackRequestedEventArgs : RoutedEventArgs
    {
        internal BackRequestedEventArgs() : base(WindowTitleBar.BackRequestedEvent)
        {
        }

        internal BackRequestedEventArgs(object source) : base(WindowTitleBar.BackRequestedEvent, source)
        {
        }
    }
}
