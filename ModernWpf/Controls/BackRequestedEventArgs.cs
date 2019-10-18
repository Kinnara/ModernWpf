using ModernWpf.Controls.Primitives;
using System.Windows;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Provides event data for the BackRequested event.
    /// </summary>
    public sealed class BackRequestedEventArgs : RoutedEventArgs
    {
        internal BackRequestedEventArgs() : base(WindowHelper.BackRequestedEvent)
        {
        }

        internal BackRequestedEventArgs(object source) : base(WindowHelper.BackRequestedEvent, source)
        {
        }
    }
}
