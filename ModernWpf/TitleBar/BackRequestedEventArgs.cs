using System.Windows;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Provides event data for the BackRequested event.
    /// </summary>
    public sealed class BackRequestedEventArgs : RoutedEventArgs
    {
        internal BackRequestedEventArgs() : this(TitleBar.BackRequestedEvent)
        {
        }

        internal BackRequestedEventArgs(object source) : this(TitleBar.BackRequestedEvent, source)
        {
        }

        internal BackRequestedEventArgs(RoutedEvent routedEvent) : base(routedEvent)
        {
        }

        internal BackRequestedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {
        }
    }
}
