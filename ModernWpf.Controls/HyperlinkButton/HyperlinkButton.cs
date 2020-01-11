using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace ModernWpf.Controls
{
    public class HyperlinkButton : ButtonBase
    {
        static HyperlinkButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyperlinkButton),
                new FrameworkPropertyMetadata(typeof(HyperlinkButton)));
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

        protected override void OnClick()
        {
            m_hyperlink.DoClick();
            base.OnClick();
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
