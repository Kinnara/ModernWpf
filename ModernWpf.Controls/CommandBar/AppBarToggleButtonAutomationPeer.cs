using System.Collections.Generic;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class AppBarToggleButtonAutomationPeer : ToggleButtonAutomationPeer, IToggleProvider
    {
        public AppBarToggleButtonAutomationPeer(AppBarToggleButton owner)
            : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Toggle)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(AppBarToggleButton);
        }

        protected override string GetNameCore()
        {
            return AppBarButtonAutomationPeerHelper.GetName(GetImpl(), GetImpl().Label);
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return AppBarElementProperties.GetIsInCommandBarFlyout(GetImpl())
                ? "menu item"
                : "app bar toggle button";
        }

        protected override string GetAcceleratorKeyCore()
        {
            return AppBarButtonAutomationPeerHelper.GetAcceleratorKey(
                GetImpl(),
                GetImpl().KeyboardAcceleratorTextOverride);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AppBarElementProperties.GetIsInCommandBarFlyout(GetImpl())
                ? AutomationControlType.MenuItem
                : AutomationControlType.Button;
        }

        protected override bool IsKeyboardFocusableCore()
        {
            var owner = GetImpl();
            if (CommandBar.FindParentCommandBarForElement(owner) != null)
            {
                return AppBarButtonAutomationPeerHelper.IsKeyboardFocusable(owner);
            }

            return base.IsKeyboardFocusableCore();
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            return null;
        }

        ToggleState IToggleProvider.ToggleState
        {
            get
            {
                switch (GetImpl().IsChecked)
                {
                    case true:
                        return ToggleState.On;
                    case false:
                        return ToggleState.Off;
                    default:
                        return ToggleState.Indeterminate;
                }
            }
        }

        void IToggleProvider.Toggle()
        {
            if (!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            GetImpl().AutomationToggleButtonOnToggle();
        }

        private AppBarToggleButton GetImpl()
        {
            return (AppBarToggleButton)Owner;
        }
    }
}
