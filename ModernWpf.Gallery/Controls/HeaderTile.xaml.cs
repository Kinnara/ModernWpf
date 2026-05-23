using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using ModernWpf;

namespace ModernWpf.Gallery.Controls
{
    public sealed partial class HeaderTile : UserControl
    {
        public HeaderTile()
        {
            InitializeComponent();
            UpdateButtonResources();
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            ThemeManager.Current.ActualApplicationThemeChanged += OnActualApplicationThemeChanged;
            Unloaded += OnUnloaded;
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(HeaderTile), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(HeaderTile), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.Register(nameof(Link), typeof(string), typeof(HeaderTile), new PropertyMetadata(null));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(object), typeof(HeaderTile), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public string Link
        {
            get { return (string)GetValue(LinkProperty); }
            set { SetValue(LinkProperty, value); }
        }

        public object Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new HeaderTileAutomationPeer(this);
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            Dispatcher.Invoke(UpdateButtonResources);
        }

        private void OnActualApplicationThemeChanged(ThemeManager sender, object args)
        {
            Dispatcher.Invoke(UpdateButtonResources);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            ThemeManager.Current.ActualApplicationThemeChanged -= OnActualApplicationThemeChanged;
            Unloaded -= OnUnloaded;
        }

        private void UpdateButtonResources()
        {
            if (SystemParameters.HighContrast)
            {
                RootButton.Resources["ButtonBackground"] = SystemColors.ControlBrush;
                RootButton.Resources["ButtonBackgroundPointerOver"] = SystemColors.ControlBrush;
                RootButton.Resources["ButtonBackgroundPressed"] = SystemColors.ControlBrush;
                return;
            }

            var color = GetWpfGalleryAcrylicBackgroundColor();
            RootButton.Resources["ButtonBackground"] = new SolidColorBrush { Color = color, Opacity = 0.8 };
            RootButton.Resources["ButtonBackgroundPointerOver"] = new SolidColorBrush { Color = color, Opacity = 0.9 };
            RootButton.Resources["ButtonBackgroundPressed"] = new SolidColorBrush { Color = color, Opacity = 1.0 };
        }

        private static Color GetWpfGalleryAcrylicBackgroundColor()
        {
            var theme = ThemeManager.Current.ApplicationTheme ?? ThemeManager.Current.ActualApplicationTheme;
            if (theme == ApplicationTheme.Light)
            {
                return Color.FromRgb(0xF9, 0xF9, 0xF9);
            }

            if (theme == ApplicationTheme.Dark)
            {
                return Color.FromRgb(0x2C, 0x2C, 0x2C);
            }

            var acrylicBrush = Application.Current.TryFindResource("AcrylicBackgroundFillColorDefaultBrush") as SolidColorBrush;
            return acrylicBrush == null ? Colors.Gray : acrylicBrush.Color;
        }

        private void RootButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Link))
            {
                Process.Start(new ProcessStartInfo(Link) { UseShellExecute = true });
            }
        }

        private sealed class HeaderTileAutomationPeer : FrameworkElementAutomationPeer
        {
            public HeaderTileAutomationPeer(HeaderTile owner)
                : base(owner)
            {
            }

            protected override AutomationControlType GetAutomationControlTypeCore()
            {
                return AutomationControlType.Custom;
            }

            protected override string GetClassNameCore()
            {
                return nameof(HeaderTile);
            }

            protected override bool IsControlElementCore()
            {
                return false;
            }

            protected override List<AutomationPeer> GetChildrenCore()
            {
                var owner = (HeaderTile)Owner;
                var rootButtonPeer = UIElementAutomationPeer.FromElement(owner.RootButton)
                    ?? UIElementAutomationPeer.CreatePeerForElement(owner.RootButton);

                return rootButtonPeer == null
                    ? base.GetChildrenCore()
                    : new List<AutomationPeer> { rootButtonPeer };
            }
        }
    }
}
