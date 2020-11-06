using System.Windows.Automation;
using System.Windows.Automation.Peers;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class TeachingTipAutomationPeer : FrameworkElementAutomationPeer
    {
        public TeachingTipAutomationPeer(TeachingTip owner) : base(owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            if (GetTeachingTip() != null/*.IsLightDismissEnabled*/)
            {
                return AutomationControlType.Window;
            }
            else
            {
                return AutomationControlType.Pane;
            }
        }

        protected override string GetClassNameCore()
        {
            return nameof(TeachingTip);
        }

        private WindowInteractionState InteractionState()
        {
            //var teachingTip = GetTeachingTip();
            //if (teachingTip.m_isIdle && teachingTip.IsOpen)
            //{
            //    return WindowInteractionState.ReadyForUserInteraction;
            //}
            //else if (teachingTip.m_isIdle && !teachingTip.IsOpen)
            //{
            //    return WindowInteractionState.BlockedByModalWindow;
            //}
            //else if (!teachingTip.m_isIdle && !teachingTip.IsOpen)
            //{
            //    return WindowInteractionState.Closing;
            //}
            //else
            //{
            //    return WindowInteractionState.Running;
            //}

            return WindowInteractionState.Running;
        }

        private bool IsModal()
        {
            return GetTeachingTip() != null/*.IsLightDismissEnabled*/;
        }

        private bool IsTopMost()
        {
            return GetTeachingTip() != null/*.IsOpen*/;
        }

        private bool Maximizable()
        {
            return false;
        }

        private bool Minimizable()
        {
            return false;
        }

        private WindowVisualState VisualState()
        {
            return WindowVisualState.Normal;
        }

        private void Close()
        {
            GetTeachingTip()/*.IsOpen = false*/;
        }

        private void SetVisualState(WindowVisualState state)
        {

        }

        private bool WaitForInputIdle(int milliseconds)
        {
            return true;
        }

        // Didn't implement these as AutomationEvents.WindowOpened & AutomationEvents.WindowClosed are not present in WPF

        //internal void RaiseWindowClosedEvent()
        //{
        //     We only report as a window when light dismiss is enabled.
        //    if (GetTeachingTip().IsLightDismissEnabled &&
        //        ListenerExists(AutomationEvents.WindowClosed))
        //    {
        //        RaiseAutomationEvent(AutomationEvents.WindowClosed);
        //    }
        //}

        //internal void RaiseWindowOpenedEvent(string displayString)
        //{
        //    AutomationPeer automationPeer7 = this;
        //    if (automationPeer7 != null)
        //    {
        //        //automationPeer7.RaiseNotificationEvent(
        //        //    Automation.Peers.AutomationNotificationKind.Other,
        //        //    Peers.AutomationNotificationProcessing.CurrentThenMostRecent,
        //        //    displayString,
        //        //    L"TeachingTipOpenedActivityId");
        //    }

        //    // We only report as a window when light dismiss is enabled.
        //    if (GetTeachingTip().IsLightDismissEnabled &&
        //        AutomationPeer.ListenerExists(AutomationEvents.WindowOpened))
        //    {
        //        RaiseAutomationEvent(AutomationEvents.WindowOpened);
        //    }
        //}

        TeachingTip GetTeachingTip()
        {
            var owner = Owner;
            return (TeachingTip)owner;
        }
    }
}
