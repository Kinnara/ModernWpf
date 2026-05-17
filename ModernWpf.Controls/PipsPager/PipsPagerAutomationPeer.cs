using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class PipsPagerAutomationPeer : FrameworkElementAutomationPeer, ISelectionProvider
    {
        public PipsPagerAutomationPeer(PipsPager owner) : base(owner)
        {
        }

        public bool CanSelectMultiple => false;

        public bool IsSelectionRequired => true;

        public IRawElementProviderSimple[] GetSelection()
        {
            var owner = (PipsPager)Owner;
            var selectedButton = owner.GetSelectedButton();
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

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Menu;
        }

        protected override string GetClassNameCore()
        {
            return nameof(PipsPager);
        }

        protected override string GetNameCore()
        {
            var name = base.GetNameCore();
            return string.IsNullOrEmpty(name) ? AutomationProperties.GetName(Owner) : name;
        }

        internal void RaiseSelectionChanged()
        {
            if (AutomationPeer.ListenerExists(AutomationEvents.SelectionPatternOnInvalidated))
            {
                RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
            }
        }
    }
}
