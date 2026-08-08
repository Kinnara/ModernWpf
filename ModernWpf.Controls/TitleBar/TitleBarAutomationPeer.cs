using System.Windows.Automation.Peers;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class TitleBarAutomationPeer : FrameworkElementAutomationPeer
    {
        public TitleBarAutomationPeer(TitleBar owner)
            : base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return nameof(TitleBar);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.TitleBar;
        }

        protected override string GetNameCore()
        {
            var name = base.GetNameCore();
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }

            return ((TitleBar)Owner).Title ?? string.Empty;
        }
    }
}
