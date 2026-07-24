using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class MenuBarItemAutomationPeer :
        FrameworkElementAutomationPeer,
        IExpandCollapseProvider,
        IInvokeProvider
    {
        public MenuBarItemAutomationPeer(MenuBarItem owner)
            : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ExpandCollapse ||
                patternInterface == PatternInterface.Invoke)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.MenuItem;
        }

        protected override string GetClassNameCore()
        {
            return nameof(MenuBarItem);
        }

        protected override string GetNameCore()
        {
            var name = base.GetNameCore();

            if (string.IsNullOrEmpty(name) && Owner is MenuBarItem owner)
            {
                name = owner.Title;
            }

            return name;
        }

        public ExpandCollapseState ExpandCollapseState
        {
            get
            {
                return Owner is MenuBarItem { IsFlyoutOpen: true } ?
                    ExpandCollapseState.Expanded :
                    ExpandCollapseState.Collapsed;
            }
        }

        public void Expand()
        {
            if (!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            ((MenuBarItem)Owner).ShowMenuFlyout();
        }

        public void Collapse()
        {
            if (!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            ((MenuBarItem)Owner).CloseMenuFlyout();
        }

        public void Invoke()
        {
            if (!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            ((MenuBarItem)Owner).Invoke();
        }
    }
}
