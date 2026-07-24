using System.Windows.Automation.Peers;

namespace ModernWpf.Controls.Primitives
{
    public class TitleBarControlAutomationPeer : FrameworkElementAutomationPeer
    {
        public TitleBarControlAutomationPeer(TitleBarControl owner)
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
                name = ((TitleBarControl)Owner).Title;
            }

            return name;
        }
    }
}
