using ModernWpf.Controls.Primitives;
using System.Windows;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Provides event data for the BackRequested event.
    /// </summary>
    public sealed class BackRequestedEventArgs : RoutedEventArgs
    {
        internal BackRequestedEventArgs() : this(WindowHelper.BackRequestedEvent)
        {
        }

        internal BackRequestedEventArgs(object source) : this(WindowHelper.BackRequestedEvent, source)
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
