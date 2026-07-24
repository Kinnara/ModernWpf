using System.Windows.Automation.Peers;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class InfoBarAutomationPeer : FrameworkElementAutomationPeer
    {
        public InfoBarAutomationPeer(InfoBar owner) : base(owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.StatusBar;
        }

        protected override string GetClassNameCore()
        {
            return nameof(InfoBar);
        }

        internal void RaiseOpenedEvent(InfoBarSeverity severity, string displayString)
        {
            InvalidatePeer();
        }

        internal void RaiseClosedEvent(InfoBarSeverity severity, string displayString)
        {
            InvalidatePeer();
        }

        protected override bool IsControlElementCore()
        {
            return Owner is InfoBar infoBar && infoBar.IsOpen;
        }
    }
}
