using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    internal class SelectorBarAutomationPeer : FrameworkElementAutomationPeer, ISelectionProvider
    {
        public SelectorBarAutomationPeer(SelectorBar owner)
            : base(owner)
        {
        }

        public bool CanSelectMultiple => false;

        public bool IsSelectionRequired => false;

        public IRawElementProviderSimple[] GetSelection()
        {
            if (OwnerSelectorBar.SelectedItem == null)
            {
                return new IRawElementProviderSimple[0];
            }

            var peer = CreatePeerForElement(OwnerSelectorBar.SelectedItem) ?? new SelectorBarItemAutomationPeer(OwnerSelectorBar.SelectedItem);
            return new[] { ProviderFromPeer(peer) };
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Selection ? this : base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(SelectorBar);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Group;
        }

        protected override bool IsControlElementCore()
        {
            return false;
        }

        protected override bool IsContentElementCore()
        {
            return false;
        }

        private SelectorBar OwnerSelectorBar => (SelectorBar)Owner;
    }
}
