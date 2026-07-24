using System.Windows.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Automation.Peers
{
    internal sealed class CommandBarFlyoutCommandBarAutomationPeer : FrameworkElementAutomationPeer
    {
        internal CommandBarFlyoutCommandBarAutomationPeer(CommandBarFlyoutCommandBar owner)
            : base(owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Menu;
        }

        protected override string GetClassNameCore()
        {
            return nameof(CommandBarFlyoutCommandBar);
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return "menu";
        }
    }
}
