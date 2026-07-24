using System.Windows.Automation.Peers;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    internal class BreadcrumbBarAutomationPeer : FrameworkElementAutomationPeer
    {
        public BreadcrumbBarAutomationPeer(BreadcrumbBar owner)
            : base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return nameof(BreadcrumbBar);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Group;
        }
    }
}
