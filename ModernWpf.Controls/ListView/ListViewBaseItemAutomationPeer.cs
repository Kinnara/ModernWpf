using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace ModernWpf.Controls
{
    public class ListViewBaseItemAutomationPeer : ListBoxItemAutomationPeer, IInvokeProvider
    {
        public ListViewBaseItemAutomationPeer(object item, ListViewBaseAutomationPeer selectorAutomationPeer)
            : base(item, selectorAutomationPeer)
        {
            _item = item;
            _selectorAutomationPeer = selectorAutomationPeer;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Invoke)
            {
                return _selectorAutomationPeer.OwnerListView.IsItemClickEnabled ? this : null;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            var container = GetContainer();
            if (container is GridViewItem)
            {
                return nameof(GridViewItem);
            }

            if (container is ListViewItem)
            {
                return nameof(ListViewItem);
            }

            return base.GetClassNameCore();
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ListItem;
        }

        public void Invoke()
        {
            var owner = _selectorAutomationPeer.OwnerListView;
            if (!owner.IsEnabled)
            {
                throw new ElementNotEnabledException();
            }

            var container = GetContainer();

            if (container == null)
            {
                throw new InvalidOperationException("ListViewBase item container was not realized.");
            }

            if (!container.IsEnabled)
            {
                throw new ElementNotEnabledException();
            }

            owner.NotifyListItemClicked(container);
        }

        private ListViewBaseItem GetContainer()
        {
            var container = _selectorAutomationPeer.OwnerListView.ItemContainerGenerator.ContainerFromItem(_item) as ListViewBaseItem;
            if (container == null && _item is ListViewBaseItem itemContainer)
            {
                container = itemContainer;
            }

            return container;
        }

        private readonly object _item;
        private readonly ListViewBaseAutomationPeer _selectorAutomationPeer;
    }
}
