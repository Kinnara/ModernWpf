using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    internal class CommandBarAutomationPeer : FrameworkElementAutomationPeer,
        IToggleProvider,
        IExpandCollapseProvider,
        IWindowProvider
    {
        public CommandBarAutomationPeer(CommandBar owner)
            : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Toggle ||
                patternInterface == PatternInterface.ExpandCollapse ||
                (patternInterface == PatternInterface.Window && GetImpl().IsOpen))
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return "ApplicationBar";
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return "app bar";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            // WPF's AutomationControlType enum has no WinUI AppBar value.
            return AutomationControlType.Custom;
        }

        public ToggleState ToggleState => GetImpl().IsOpen
            ? ToggleState.On
            : ToggleState.Off;

        public void Toggle()
        {
            EnsureEnabled();
            var commandBar = GetImpl();
            commandBar.SetCurrentValue(CommandBar.IsOpenProperty, !commandBar.IsOpen);
        }

        public ExpandCollapseState ExpandCollapseState => GetImpl().IsOpen
            ? ExpandCollapseState.Expanded
            : ExpandCollapseState.Collapsed;

        public void Expand()
        {
            EnsureEnabled();
            GetImpl().SetCurrentValue(CommandBar.IsOpenProperty, true);
        }

        public void Collapse()
        {
            EnsureEnabled();
            GetImpl().SetCurrentValue(CommandBar.IsOpenProperty, false);
        }

        public bool Maximizable => false;

        public bool Minimizable => false;

        public bool IsModal => true;

        public bool IsTopmost => true;

        public WindowInteractionState InteractionState => WindowInteractionState.Running;

        public WindowVisualState VisualState => WindowVisualState.Normal;

        public void Close()
        {
        }

        public void SetVisualState(WindowVisualState state)
        {
        }

        public bool WaitForInputIdle(int milliseconds)
        {
            return true;
        }

        internal void RaiseIsOpenChanged(bool oldValue, bool newValue)
        {
            RaisePropertyChangedEvent(
                TogglePatternIdentifiers.ToggleStateProperty,
                oldValue ? ToggleState.On : ToggleState.Off,
                newValue ? ToggleState.On : ToggleState.Off);
            RaisePropertyChangedEvent(
                ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
                newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
        }

        private void EnsureEnabled()
        {
            if (!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }
        }

        private CommandBar GetImpl()
        {
            return (CommandBar)Owner;
        }
    }
}
