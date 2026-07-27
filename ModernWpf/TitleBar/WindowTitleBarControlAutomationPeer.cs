using System.Windows.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Automation.Peers
{
    public class WindowTitleBarControlAutomationPeer : FrameworkElementAutomationPeer
    {
        public WindowTitleBarControlAutomationPeer(WindowTitleBarControl owner)
            : base(owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.TitleBar;
        }

        protected override string GetClassNameCore()
        {
            return "TitleBar";
        }

        protected override string GetNameCore()
        {
            var name = base.GetNameCore();
            if (string.IsNullOrEmpty(name))
            {
                name = ((WindowTitleBarControl)Owner).Title;
            }

            return name;
        }
    }
}
