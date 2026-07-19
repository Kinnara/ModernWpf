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

        protected override string GetClassNameCore()
        {
            return OwnerListView is GridView ? nameof(GridView) : nameof(ListView);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.List;
        }

        internal ListViewBase OwnerListView { get; }
    }
}
