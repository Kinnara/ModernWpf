using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class AppBarButtonAutomationPeer : ButtonAutomationPeer, IExpandCollapseProvider
    {
        public AppBarButtonAutomationPeer(AppBarButton owner)
            : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ExpandCollapse && GetImpl().Flyout != null)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(AppBarButton);
        }

        protected override string GetNameCore()
        {
            return AppBarButtonAutomationPeerHelper.GetName(GetImpl(), GetImpl().Label);
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return "app bar button";
        }

        protected override string GetAcceleratorKeyCore()
        {
            return AppBarButtonAutomationPeerHelper.GetAcceleratorKey(
                GetImpl(),
                GetImpl().KeyboardAcceleratorTextOverride);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Button;
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            return null;
        }

        public ExpandCollapseState ExpandCollapseState
        {
            get
            {
                return GetImpl().Flyout?.IsOpen == true
                    ? ExpandCollapseState.Expanded
                    : ExpandCollapseState.Collapsed;
            }
        }

        public void Expand()
        {
            if (!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            var owner = GetImpl();
            owner.OpenAssociatedFlyout();
        }

        public void Collapse()
        {
            GetImpl().Flyout?.Hide();
        }

        private AppBarButton GetImpl()
        {
            return (AppBarButton)Owner;
        }
    }

    internal static class AppBarButtonAutomationPeerHelper
    {
        public static string GetName(DependencyObject owner, string label)
        {
            if (HasCustomAutomationProperty(owner, AutomationProperties.NameProperty))
            {
                return AutomationProperties.GetName(owner) ?? string.Empty;
            }

            return label ?? string.Empty;
        }

        public static string GetAcceleratorKey(DependencyObject owner, string keyboardAcceleratorTextOverride)
        {
            if (HasCustomAutomationProperty(owner, AutomationProperties.AcceleratorKeyProperty))
            {
                return AutomationProperties.GetAcceleratorKey(owner) ?? string.Empty;
            }

            return TrimKeyboardAcceleratorTextOverride(keyboardAcceleratorTextOverride);
        }

        private static bool HasCustomAutomationProperty(DependencyObject owner, DependencyProperty property)
        {
            return DependencyPropertyHelper.GetValueSource(owner, property).BaseValueSource != BaseValueSource.Default;
        }

        private static string TrimKeyboardAcceleratorTextOverride(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Trim(' ');
        }
    }
}
