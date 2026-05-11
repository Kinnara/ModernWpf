using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class BreadcrumbBarItemAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
    {
        public BreadcrumbBarItemAutomationPeer(BreadcrumbBarItem owner)
            : base(owner)
        {
        }

        public void Invoke()
        {
            OwnerItem.Invoke();
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Invoke ? this : base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(BreadcrumbBarItem);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Button;
        }

        private BreadcrumbBarItem OwnerItem => (BreadcrumbBarItem)Owner;
    }
}
