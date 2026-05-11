using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class PagerControlAutomationPeer : FrameworkElementAutomationPeer, ISelectionProvider
    {
        public PagerControlAutomationPeer(PagerControl owner)
            : base(owner)
        {
        }

        public bool CanSelectMultiple => false;

        public bool IsSelectionRequired => true;

        public IRawElementProviderSimple[] GetSelection()
        {
            var selectedButton = OwnerPagerControl.GetSelectedButton();
            if (selectedButton == null)
            {
                return new IRawElementProviderSimple[0];
            }

            var peer = CreatePeerForElement(selectedButton) ?? new ButtonAutomationPeer(selectedButton);
            return new[] { ProviderFromPeer(peer) };
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Selection ? this : base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(PagerControl);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Group;
        }

        private PagerControl OwnerPagerControl => (PagerControl)Owner;
    }
}
