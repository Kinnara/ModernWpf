using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class TeachingTipAutomationPeer : FrameworkElementAutomationPeer, IWindowProvider
    {
        public TeachingTipAutomationPeer(TeachingTip owner)
            : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Window ? this : base.GetPattern(patternInterface);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return GetImpl().IsLightDismissEnabled ? AutomationControlType.Window : AutomationControlType.Pane;
        }

        protected override string GetClassNameCore()
        {
            return nameof(TeachingTip);
        }

        public bool Maximizable => false;

        public bool Minimizable => false;

        public bool IsModal => GetImpl().IsLightDismissEnabled;

        public bool IsTopmost => GetImpl().IsOpen;

        public WindowInteractionState InteractionState
        {
            get
            {
                var teachingTip = GetImpl();
                if (teachingTip.IsIdleForAutomation && teachingTip.IsOpen)
                {
                    return WindowInteractionState.ReadyForUserInteraction;
                }

                if (teachingTip.IsIdleForAutomation)
                {
                    return WindowInteractionState.BlockedByModalWindow;
                }

                return teachingTip.IsOpen
                    ? WindowInteractionState.Running
                    : WindowInteractionState.Closing;
            }
        }

        public WindowVisualState VisualState => WindowVisualState.Normal;

        public void Close()
        {
            GetImpl().SetCurrentValue(TeachingTip.IsOpenProperty, false);
        }

        public void SetVisualState(WindowVisualState state)
        {
        }

        public bool WaitForInputIdle(int milliseconds)
        {
            return true;
        }

        internal void RaiseWindowOpenedEvent(string displayString)
        {
            // WPF AutomationPeer does not expose WinUI's notification or window-opened events.
        }

        internal void RaiseWindowClosedEvent()
        {
            // WPF AutomationPeer does not expose WinUI's window-closed event.
        }

        private TeachingTip GetImpl()
        {
            return (TeachingTip)Owner;
        }
    }
}
