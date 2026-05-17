using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
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
            // WinUI returns an empty selection because the number panel mixes page
            // buttons and ellipsis icons, so page indices do not map directly to
            // repeater indices.
            return new IRawElementProviderSimple[0];
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
            return AutomationControlType.Menu;
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
