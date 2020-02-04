using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public class AutoSuggestBoxListViewItem : System.Windows.Controls.ListViewItem
    {
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                m_isPressed = true;
            }
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                HandleMouseUp(MouseButton.Left);
                m_isPressed = false;
            }
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (!e.Handled)
            {
                m_isPressed = false;
            }
            base.OnMouseLeave(e);
        }

        private void HandleMouseUp(MouseButton mouseButton)
        {
            if (m_isPressed && SelectorHelper.UiGetIsSelectable(this) && Focus())
            {
                ParentListView?.NotifyListItemClicked(this, mouseButton);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Enter)
            {
                if (SelectorHelper.UiGetIsSelectable(this) && Focus())
                {
                    ParentListView?.NotifyListItemClicked(this);
                    e.Handled = true;
                }
            }
        }

        private AutoSuggestBoxListView ParentListView => ParentSelector as AutoSuggestBoxListView;

        internal Selector ParentSelector => ItemsControl.ItemsControlFromItemContainer(this) as Selector;

        private bool m_isPressed;
    }
}