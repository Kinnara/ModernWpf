using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows;
using System.Windows.Automation;
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
            if (m_isPressed && SelectorHelper.UiGetIsSelectable(this))
            {
                NotifyClicked(mouseButton);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Enter || (e.Key == Key.Space && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                if (SelectorHelper.UiGetIsSelectable(this))
                {
                    NotifyClicked(isSecondaryGesture: e.Key == Key.Space);
                    e.Handled = true;
                }
            }
        }

        internal void NotifyClicked(MouseButton? mouseButton = null, bool isSecondaryGesture = false)
        {
            if (SelectorHelper.UiGetIsSelectable(this))
            {
                Focus();
                ParentListView?.NotifyListItemClicked(this, mouseButton, isSecondaryGesture);
            }
        }

        private AutoSuggestBoxListView ParentListView => ParentSelector as AutoSuggestBoxListView;

        internal Selector ParentSelector => ItemsControl.ItemsControlFromItemContainer(this) as Selector;

        private bool m_isPressed;
    }

    internal class AutoSuggestBoxListViewAutomationPeer : ListBoxAutomationPeer
    {
        public AutoSuggestBoxListViewAutomationPeer(AutoSuggestBoxListView owner)
            : base(owner)
        {
            OwnerListView = owner;
        }

        protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
        {
            return new AutoSuggestBoxListViewItemAutomationPeer(item, this);
        }

        internal AutoSuggestBoxListView OwnerListView { get; }
    }

    internal class AutoSuggestBoxListViewItemAutomationPeer : ListBoxItemAutomationPeer, IInvokeProvider
    {
        public AutoSuggestBoxListViewItemAutomationPeer(object item, AutoSuggestBoxListViewAutomationPeer selectorAutomationPeer)
            : base(item, selectorAutomationPeer)
        {
            _item = item;
            _selectorAutomationPeer = selectorAutomationPeer;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Invoke)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(AutoSuggestBoxListViewItem);
        }

        public void Invoke()
        {
            var owner = _selectorAutomationPeer.OwnerListView;
            if (!owner.IsEnabled)
            {
                throw new ElementNotEnabledException();
            }

            var container = owner.ItemContainerGenerator.ContainerFromItem(_item) as AutoSuggestBoxListViewItem;
            if (container == null && _item is AutoSuggestBoxListViewItem itemContainer)
            {
                container = itemContainer;
            }

            if (container == null)
            {
                throw new System.InvalidOperationException("AutoSuggestBox suggestion item container was not realized.");
            }

            if (!container.IsEnabled)
            {
                throw new ElementNotEnabledException();
            }

            container.NotifyClicked(MouseButton.Left);
        }

        private readonly object _item;
        private readonly AutoSuggestBoxListViewAutomationPeer _selectorAutomationPeer;
    }
}
