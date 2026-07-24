using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class HyperlinkButton : ButtonBase
    {
        static HyperlinkButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HyperlinkButton),
                new FrameworkPropertyMetadata(typeof(HyperlinkButton)));

            HorizontalContentAlignmentProperty.OverrideMetadata(
                typeof(HyperlinkButton),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));

            VerticalContentAlignmentProperty.OverrideMetadata(
                typeof(HyperlinkButton),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new HyperlinkButtonAutomationPeer(this);
        }

        protected override void OnClick()
        {
            if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
            {
                AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(this);
                if (peer != null)
                    peer.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
            }

            base.OnClick();

            if (NavigateUri is { } uri)
            {
                Process.Start(new ProcessStartInfo(uri.ToString())
                {
                    UseShellExecute = true
                });
            }
        }

        internal void AutomationButtonBaseClick()
        {
            OnClick();
        }

    }
}
