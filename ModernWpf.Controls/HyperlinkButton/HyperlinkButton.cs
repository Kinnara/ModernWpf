using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Navigation;
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

        public HyperlinkButton()
        {
            m_hyperlink = new Hyperlink
            {
                NavigateUri = NavigateUri,
                TargetName = TargetName
            };
            m_hyperlink.RequestNavigate += OnRequestNavigate;
            AddLogicalChild(m_hyperlink);
        }

        public static readonly DependencyProperty NavigateUriProperty =
            Hyperlink.NavigateUriProperty.AddOwner(
                typeof(HyperlinkButton),
                new FrameworkPropertyMetadata(OnNavigateUriChanged));

        public Uri NavigateUri
        {
            get => (Uri)GetValue(NavigateUriProperty);
            set => SetValue(NavigateUriProperty, value);
        }

        private static void OnNavigateUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HyperlinkButton)d).m_hyperlink.NavigateUri = (Uri)e.NewValue;
        }

        public static readonly DependencyProperty TargetNameProperty =
            Hyperlink.TargetNameProperty.AddOwner(
                typeof(HyperlinkButton),
                new FrameworkPropertyMetadata(OnTargetNameChanged));

        public string TargetName
        {
            get => (string)GetValue(TargetNameProperty);
            set => SetValue(TargetNameProperty, value);
        }

        private static void OnTargetNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HyperlinkButton)d).m_hyperlink.TargetName = (string)e.NewValue;
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

            m_hyperlink.DoClick();
            base.OnClick();
        }

        internal void AutomationButtonBaseClick()
        {
            OnClick();
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Uri uri = e.Uri;
            if (uri.IsAbsoluteUri && uri.Scheme.IndexOf("http", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Process.Start(new ProcessStartInfo(uri.ToString())
                {
                    UseShellExecute = true
                });
                e.Handled = true;
            }
        }

        private readonly Hyperlink m_hyperlink;
    }
}
