using System;
using System.Windows;

namespace ModernWpf.Controls
{
    internal class RoutedEventHandlerRevoker
    {
        public RoutedEventHandlerRevoker(UIElement source, RoutedEvent routedEvent, Delegate handler)
        {
            m_source = new WeakReference<UIElement>(source);
            m_event = routedEvent;
            m_handler = new WeakReference<Delegate>(handler);

            source.AddHandler(routedEvent, handler);
        }

        private readonly WeakReference<UIElement> m_source;
        private readonly RoutedEvent m_event;
        private readonly WeakReference<Delegate> m_handler;

        public void Revoke()
        {
            if (m_source.TryGetTarget(out var source) &&
                m_handler.TryGetTarget(out var handler))
            {
                source.RemoveHandler(m_event, handler);
            }
        }
    }
}
