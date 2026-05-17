using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class HyperlinkButton : ButtonBase
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

        public static readonly DependencyProperty NavigateUriProperty =
            DependencyProperty.Register(
                nameof(NavigateUri),
                typeof(Uri),
                typeof(HyperlinkButton),
                new FrameworkPropertyMetadata(null));

        public Uri NavigateUri
        {
            get => (Uri)GetValue(NavigateUriProperty);
            set => SetValue(NavigateUriProperty, value);
        }

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(HyperlinkButton));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        public static readonly DependencyProperty FocusVisualMarginProperty =
            FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(HyperlinkButton));

        public Thickness FocusVisualMargin
        {
            get => (Thickness)GetValue(FocusVisualMarginProperty);
            set => SetValue(FocusVisualMarginProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(HyperlinkButton));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
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
