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
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        public void Invoke()
        {
            var owner = _selectorAutomationPeer.OwnerListView;
            if (!owner.IsEnabled)
            {
                throw new ElementNotEnabledException();
            }

            var container = owner.ItemContainerGenerator.ContainerFromItem(_item) as ListViewBaseItem;
            if (container == null && _item is ListViewBaseItem itemContainer)
            {
                container = itemContainer;
            }

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

        private readonly object _item;
        private readonly ListViewBaseAutomationPeer _selectorAutomationPeer;
    }
}
