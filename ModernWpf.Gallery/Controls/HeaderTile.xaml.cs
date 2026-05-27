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
    public partial class HeaderTile : UserControl
    {
        public HeaderTile()
        {
            InitializeComponent();
            UpdateButtonResources();
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            ThemeManager.Current.ActualApplicationThemeChanged += OnActualApplicationThemeChanged;
            Unloaded += OnUnloaded;
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateButtonResources();
            });
        }

        private void OnActualApplicationThemeChanged(ThemeManager sender, object args)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateButtonResources();
            });
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
            ThemeManager.Current.ActualApplicationThemeChanged -= OnActualApplicationThemeChanged;
            Unloaded -= OnUnloaded;
        }

        private void UpdateButtonResources()
        {
            ApplyButtonResources(SystemParameters.HighContrast);
        }

        internal void ApplyButtonResources(bool highContrast)
        {
            if (!highContrast)
            {
                var color = GetWpfGalleryAcrylicBackgroundColor();
                RootButton.Resources["ButtonBackground"] = new SolidColorBrush { Color = color, Opacity = 0.8 };
                RootButton.Resources["ButtonBackgroundPointerOver"] = new SolidColorBrush { Color = color, Opacity = 0.9 };
                RootButton.Resources["ButtonBackgroundPressed"] = new SolidColorBrush { Color = color, Opacity = 1.0 };
            }
            else
            {
                RootButton.Resources["ButtonBackground"] = SystemColors.ControlBrush;
                RootButton.Resources["ButtonBackgroundPointerOver"] = SystemColors.ControlBrush;
                RootButton.Resources["ButtonBackgroundPressed"] = SystemColors.ControlBrush;
            }
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

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(HeaderTile), new PropertyMetadata(""));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("ColorExplanation", typeof(string), typeof(HeaderTile), new PropertyMetadata(""));

        public string Link
        {
            get { return (string)GetValue(LinkProperty); }
            set { SetValue(LinkProperty, value); }
        }

        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.Register("Link", typeof(string), typeof(HeaderTile), new PropertyMetadata(null));

        public object Source
        {
            get { return (object)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(HeaderTile), new PropertyMetadata(null));

        private void RootButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Link))
            {
                Process.Start(new ProcessStartInfo(Link) { UseShellExecute = true });
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new HeaderTileAutomationPeer(this);
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
