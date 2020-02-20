using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    partial class ItemsRepeater
    {
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            bool ignore = true;

            var last = m_layoutEvents.Last;
            if (last != null)
            {
                if (last.Value == LayoutEvent.Measure)
                {
                    if (last.Previous?.Value != LayoutEvent.Measure)
                    {
                        ignore = false;
                    }
                }
                else if (last.Value == LayoutEvent.Arrange)
                {
                    var previous = last.Previous;
                    if (previous == null || previous.Value == LayoutEvent.Measure)
                    {
                        var virtInfo = GetVirtualizationInfo(child);
                        if (virtInfo.IsPinned)
                        {
                            ignore = false;
                        }
                    }
                }
            }
            else
            {
                ignore = false;
            }

            if (!ignore)
            {
                OnLayoutEvent(LayoutEvent.ChildDesiredSizeChanged);
                base.OnChildDesiredSizeChanged(child);
            }
        }

        protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            return new RepeaterUIElementCollection(this, logicalParent);
        }

        private void OnLayoutEvent(LayoutEvent e)
        {
            if (m_layoutEvents.Count >= 2)
            {
                m_layoutEvents.RemoveFirst();
            }

            m_layoutEvents.AddLast(e);
        }

        private enum LayoutEvent
        {
            Measure,
            Arrange,
            ChildDesiredSizeChanged
        }

        private readonly LinkedList<LayoutEvent> m_layoutEvents = new LinkedList<LayoutEvent>();
    }
}
