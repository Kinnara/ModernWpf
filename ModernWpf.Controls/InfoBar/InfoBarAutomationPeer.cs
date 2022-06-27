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

        public InfoBar GetInfoBar()
        {
            UIElement owner = Owner;
            return owner as InfoBar;
        }
    }
}
