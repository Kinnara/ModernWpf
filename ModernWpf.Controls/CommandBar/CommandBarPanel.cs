using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ModernWpf.Controls.Primitives
{
    public class CommandBarPanel : ToolBarPanel
    {
        internal bool HasChildren
        {
            get => m_hasChildren;
            set
            {
                if (m_hasChildren != value)
                {
                    m_hasChildren = value;
                    HasChildrenChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        internal event EventHandler HasChildrenChanged;

        internal bool HasVisibleBottomLabel
        {
            get
            {
                foreach (UIElement child in GetChildren(includeCollapsed: false))
                {
                    if (child is IAppBarButtonElement appBarButtonElement &&
                        appBarButtonElement.GetHasBottomLabel())
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        internal IEnumerable<UIElement> GetChildren(bool includeCollapsed)
        {
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                if (InternalChildren[i] is UIElement child &&
                    (includeCollapsed || child.Visibility != Visibility.Collapsed))
                {
                    yield return child;
                }
            }
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            UpdateHasChildren();
        }

        private void UpdateHasChildren()
        {
            bool hasChildren = false;

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                if (InternalChildren[i] != null)
                {
                    hasChildren = true;
                    break;
                }
            }

            HasChildren = hasChildren;
        }

        private bool m_hasChildren;
    }
}
