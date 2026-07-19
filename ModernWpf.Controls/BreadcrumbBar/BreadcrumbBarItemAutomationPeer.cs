using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Automation.Peers
{
    public class BreadcrumbBarItemAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
    {
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(BreadcrumbBar));

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

        protected override string GetLocalizedControlTypeCore()
        {
            return ResourceAccessor.GetLocalizedStringResource(SR_BreadcrumbBarItemLocalizedControlType);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Button;
        }

        protected override bool IsControlElementCore()
        {
            return OwnerItem.IsVisibleForAutomation && base.IsControlElementCore();
        }

        protected override bool IsContentElementCore()
        {
            return OwnerItem.IsVisibleForAutomation && base.IsContentElementCore();
        }

        private BreadcrumbBarItem OwnerItem => (BreadcrumbBarItem)Owner;
    }
}
