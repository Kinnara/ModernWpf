using System.Windows.Automation;
using System.Windows.Automation.Peers;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class MenuBarAutomationPeer : FrameworkElementAutomationPeer
    {
        public MenuBarAutomationPeer(MenuBar owner)
            : base(owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.MenuBar;
        }

        protected override string GetClassNameCore()
        {
            return nameof(MenuBar);
        }
    }
}
