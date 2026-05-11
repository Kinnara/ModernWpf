using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class SelectorBarItemAutomationPeer : FrameworkElementAutomationPeer, ISelectionItemProvider
    {
        public SelectorBarItemAutomationPeer(SelectorBarItem owner)
            : base(owner)
        {
        }

        public bool IsSelected => OwnerItem.IsSelected;

        public IRawElementProviderSimple SelectionContainer
        {
            get
            {
                if (OwnerItem.Owner == null)
                {
                    return null;
                }

                var peer = CreatePeerForElement(OwnerItem.Owner) ?? new SelectorBarAutomationPeer(OwnerItem.Owner);
                return ProviderFromPeer(peer);
            }
        }

        public void AddToSelection()
        {
            Select();
        }

        public void RemoveFromSelection()
        {
            if (OwnerItem.Owner?.SelectedItem == OwnerItem)
            {
                OwnerItem.Owner.SelectedItem = null;
            }
        }

        public void Select()
        {
            OwnerItem.Select();
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.SelectionItem ? this : base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(SelectorBarItem);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.TabItem;
        }

        private SelectorBarItem OwnerItem => (SelectorBarItem)Owner;
    }
}
