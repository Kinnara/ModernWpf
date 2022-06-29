using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace ModernWpf.Controls
{
    internal class InfoBarAutomationPeer : FrameworkElementAutomationPeer
    {
        public InfoBarAutomationPeer(InfoBar owner) :
            base(owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.StatusBar;
        }

        protected override string GetClassNameCore()
        {
            return nameof(InfoBar);
        }

        public void RaiseOpenedEvent(InfoBarSeverity severity, string displayString)
        {
            //if (this is IAutomationPeer7 automationPeer7)
            //{
            //    automationPeer7.RaiseNotificationEvent(Automation.Peers.AutomationNotificationKind.Other, GetProcessingForSeverity(severity), displayString, "InfoBarOpenedActivityId");
            //}
        }

        public void RaiseClosedEvent(InfoBarSeverity severity, string displayString)
        {
            //Peers.AutomationNotificationProcessing processing = Peers.AutomationNotificationProcessing.CurrentThenMostRecent;

            //if (this is IAutomationPeer7 automationPeer7)
            //{
            //    automationPeer7.RaiseNotificationEvent(Automation.Peers.AutomationNotificationKind.Other, GetProcessingForSeverity(severity), displayString, "InfoBarClosedActivityId");
            //}
        }

        //public Peers.AutomationNotificationProcessing GetProcessingForSeverity(InfoBarSeverity severity)
        //{
        //    Peers.AutomationNotificationProcessing processing = Peers.AutomationNotificationProcessing.CurrentThenMostRecent;

        //    if (severity == InfoBarSeverity.Error || severity == InfoBarSeverity.Warning)
        //    {
        //        processing = Peers.AutomationNotificationProcessing.ImportantAll;
        //    }

        //    return new Peers.AutomationNotificationProcessing(processing);
        //}

        public InfoBar GetInfoBar()
        {
            UIElement owner = Owner;
            return owner as InfoBar;
        }
    }
}
