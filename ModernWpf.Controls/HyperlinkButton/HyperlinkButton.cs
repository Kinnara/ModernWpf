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
        private readonly Hyperlink _hyperlink;

        static HyperlinkButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyperlinkButton),
                new FrameworkPropertyMetadata(typeof(HyperlinkButton)));
        }

        public HyperlinkButton()
        {
            _hyperlink = new Hyperlink
            {
                NavigateUri = NavigateUri,
                TargetName = TargetName
            };
            _hyperlink.RequestNavigate += OnRequestNavigate;
            AddLogicalChild(_hyperlink);
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
            ((HyperlinkButton)d)._hyperlink.NavigateUri = (Uri)e.NewValue;
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
            ((HyperlinkButton)d)._hyperlink.TargetName = (string)e.NewValue;
        }

        protected override void OnClick()
        {
            _hyperlink.DoClick();
            base.OnClick();
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (e.Uri.Scheme.IndexOf("http", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Process.Start(e.Uri.ToString());
                e.Handled = true;
            }
        }
    }
}
