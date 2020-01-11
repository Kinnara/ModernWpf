using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public class AutoSuggestBoxListViewItem : ListViewItem
    {
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
            }
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                HandleMouseButtonUp(MouseButton.Left);
            }
            base.OnMouseLeftButtonUp(e);
        }

        private void HandleMouseButtonUp(MouseButton mouseButton)
        {
            if (SelectorHelper.UiGetIsSelectable(this) && Focus())
            {
                ParentListView?.NotifyListItemClicked(this, mouseButton);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.Enter:
                    if (SelectorHelper.UiGetIsSelectable(this) && Focus())
                    {
                        ParentListView?.NotifyListItemClicked(this);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private AutoSuggestBoxListView ParentListView => ParentSelector as AutoSuggestBoxListView;

        internal Selector ParentSelector => ItemsControl.ItemsControlFromItemContainer(this) as Selector;
    }
}