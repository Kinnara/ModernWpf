using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace ModernWpf.Gallery.Controls
{
    public sealed partial class HeaderTile : UserControl
    {
        public HeaderTile()
        {
            InitializeComponent();
            UpdateAutomationName();
            UpdateButtonResources();
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            Unloaded += OnUnloaded;
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(HeaderTile), new PropertyMetadata(string.Empty, OnTitleChanged));

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

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            Dispatcher.Invoke(UpdateButtonResources);
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HeaderTile)d).UpdateAutomationName();
        }

        private void UpdateAutomationName()
        {
            if (RootButton != null)
            {
                AutomationProperties.SetName(RootButton, Title);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
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

            var acrylicBrush = Application.Current.TryFindResource("AcrylicBackgroundFillColorDefaultBrush") as SolidColorBrush;
            var color = acrylicBrush == null ? Colors.Gray : acrylicBrush.Color;
            RootButton.Resources["ButtonBackground"] = new SolidColorBrush { Color = color, Opacity = 0.8 };
            RootButton.Resources["ButtonBackgroundPointerOver"] = new SolidColorBrush { Color = color, Opacity = 0.9 };
            RootButton.Resources["ButtonBackgroundPressed"] = new SolidColorBrush { Color = color, Opacity = 1.0 };
        }

        private void RootButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Link))
            {
                Process.Start(new ProcessStartInfo(Link) { UseShellExecute = true });
            }
        }
    }
}
