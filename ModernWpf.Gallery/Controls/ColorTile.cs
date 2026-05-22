using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.Gallery.Controls
{
    public class ColorTile : UserControl
    {
        public static readonly DependencyProperty ColorBrushNameProperty =
            DependencyProperty.Register(nameof(ColorBrushName), typeof(string), typeof(ColorTile), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ColorExplanationProperty =
            DependencyProperty.Register(nameof(ColorExplanation), typeof(string), typeof(ColorTile), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ColorNameProperty =
            DependencyProperty.Register(nameof(ColorName), typeof(string), typeof(ColorTile), new PropertyMetadata(string.Empty, OnColorNameChanged));

        public static readonly DependencyProperty ColorValueProperty =
            DependencyProperty.Register(nameof(ColorValue), typeof(string), typeof(ColorTile), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ShowSeparatorProperty =
            DependencyProperty.Register(nameof(ShowSeparator), typeof(bool), typeof(ColorTile), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowWarningProperty =
            DependencyProperty.Register(nameof(ShowWarning), typeof(bool), typeof(ColorTile), new PropertyMetadata(false));

        public static readonly DependencyProperty TileRadiusProperty =
            DependencyProperty.Register(nameof(TileRadius), typeof(CornerRadius), typeof(ColorTile), new PropertyMetadata(new CornerRadius(0)));

        static ColorTile()
        {
            CommandManager.RegisterClassCommandBinding(typeof(ColorTile), new CommandBinding(ApplicationCommands.Copy, CopyColorBrushName));
        }

        public string ColorBrushName
        {
            get { return (string)GetValue(ColorBrushNameProperty); }
            set { SetValue(ColorBrushNameProperty, value); }
        }

        public string ColorExplanation
        {
            get { return (string)GetValue(ColorExplanationProperty); }
            set { SetValue(ColorExplanationProperty, value); }
        }

        public string ColorName
        {
            get { return (string)GetValue(ColorNameProperty); }
            set { SetValue(ColorNameProperty, value); }
        }

        public string ColorValue
        {
            get { return (string)GetValue(ColorValueProperty); }
            set { SetValue(ColorValueProperty, value); }
        }

        public bool ShowSeparator
        {
            get { return (bool)GetValue(ShowSeparatorProperty); }
            set { SetValue(ShowSeparatorProperty, value); }
        }

        public bool ShowWarning
        {
            get { return (bool)GetValue(ShowWarningProperty); }
            set { SetValue(ShowWarningProperty, value); }
        }

        public CornerRadius TileRadius
        {
            get { return (CornerRadius)GetValue(TileRadiusProperty); }
            set { SetValue(TileRadiusProperty, value); }
        }

        private static void CopyColorBrushName(object sender, ExecutedRoutedEventArgs e)
        {
            var colorTile = sender as ColorTile;
            if (colorTile == null || string.IsNullOrEmpty(colorTile.ColorBrushName))
            {
                return;
            }

            try
            {
                Clipboard.SetText(colorTile.ColorBrushName);
                RaiseNotification(colorTile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error copying to clipboard: " + ex.Message);
            }
        }

        private static void OnColorNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorTile = (ColorTile)d;
            AutomationProperties.SetName(colorTile, e.NewValue as string ?? string.Empty);
        }

        private static void RaiseNotification(ColorTile colorTile)
        {
#if NET8_0_OR_GREATER
            var peer = UIElementAutomationPeer.CreatePeerForElement(colorTile);
            if (peer == null)
            {
                return;
            }

            peer.RaiseNotificationEvent(
                AutomationNotificationKind.Other,
                AutomationNotificationProcessing.ImportantMostRecent,
                "Color Brush Name Copied",
                "ButtonClickedActivity");
#endif
        }
    }
}
