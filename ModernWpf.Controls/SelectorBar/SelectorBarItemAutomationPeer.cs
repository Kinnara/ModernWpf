using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Automation.Peers
{
    public class SelectorBarItemAutomationPeer : FrameworkElementAutomationPeer, ISelectionItemProvider
    {
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(SelectorBar));

        public SelectorBarItemAutomationPeer(SelectorBarItem owner)
            : base(owner)
        {
        }

        public bool IsSelected => OwnerItem.IsSelected;

        public IRawElementProviderSimple SelectionContainer
        {
            get
            {
                var itemsView = OwnerItem.Owner?.ItemsView;
                if (itemsView == null)
                {
                    return null;
                }

                var peer = CreatePeerForElement(itemsView) ?? new SelectorBarItemsControlAutomationPeer(itemsView);
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
            return GetDefaultControlName();
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

            return string.IsNullOrEmpty(name) ? GetDefaultControlName() : name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ListItem;
        }

        private static string GetDefaultControlName()
        {
            return ResourceAccessor.GetLocalizedStringResource(SR_SelectorBarItemDefaultControlName);
        }

        private SelectorBarItem OwnerItem => (SelectorBarItem)Owner;
    }
}
