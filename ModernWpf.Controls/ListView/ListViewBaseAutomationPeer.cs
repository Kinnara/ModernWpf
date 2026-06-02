using System.Windows.Automation.Peers;

namespace ModernWpf.Controls
{
    public class ListViewBaseAutomationPeer : ListBoxAutomationPeer
    {
        public ListViewBaseAutomationPeer(ListViewBase owner)
            : base(owner)
        {
            OwnerListView = owner;
        }

        protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
        {
            return new ListViewBaseItemAutomationPeer(item, this);
        }

        internal ListViewBase OwnerListView { get; }
    }
}
