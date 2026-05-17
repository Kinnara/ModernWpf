using System.Windows.Automation;
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

        protected override string GetLocalizedControlTypeCore()
        {
            return DefaultControlName;
        }

        protected override string GetNameCore()
        {
            var name = AutomationProperties.GetName(OwnerItem);
            if (string.IsNullOrEmpty(name))
            {
                name = OwnerItem.Text;
            }

            if (string.IsNullOrEmpty(name) && OwnerItem.Child != null)
            {
                name = OwnerItem.Child.ToString();
            }

            return string.IsNullOrEmpty(name) ? DefaultControlName : name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.TabItem;
        }

        private const string DefaultControlName = nameof(SelectorBarItem);
        private SelectorBarItem OwnerItem => (SelectorBarItem)Owner;
    }
}
