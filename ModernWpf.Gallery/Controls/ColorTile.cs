using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.Gallery.Controls
{
    public partial class ColorTile : UserControl
    {
        static ColorTile()
        {
            CommandManager.RegisterClassCommandBinding(typeof(ColorTile), new CommandBinding(ApplicationCommands.Copy, Copy_ColorBrushName));
        }

        public CornerRadius TileRadius
        {
            get { return (CornerRadius)GetValue(TileRadiusProperty); }
            set { SetValue(TileRadiusProperty, value); }
        }

        public static readonly DependencyProperty TileRadiusProperty =
            DependencyProperty.Register("TileRadius", typeof(CornerRadius), typeof(ColorTile), new PropertyMetadata(new CornerRadius(0)));

        public string ColorName
        {
            get { return (string)GetValue(ColorNameProperty); }
            set { SetValue(ColorNameProperty, value); }
        }

        public static readonly DependencyProperty ColorNameProperty =
            DependencyProperty.Register("ColorName", typeof(string), typeof(ColorTile), new PropertyMetadata(""));

        public string ColorExplanation
        {
            get { return (string)GetValue(ColorExplanationProperty); }
            set { SetValue(ColorExplanationProperty, value); }
        }

        public static readonly DependencyProperty ColorExplanationProperty =
            DependencyProperty.Register("ColorExplanation", typeof(string), typeof(ColorTile), new PropertyMetadata(""));

        public string ColorBrushName
        {
            get { return (string)GetValue(ColorBrushNameProperty); }
            set { SetValue(ColorBrushNameProperty, value); }
        }

        public static readonly DependencyProperty ColorBrushNameProperty =
            DependencyProperty.Register("ColorBrushName", typeof(string), typeof(ColorTile), new PropertyMetadata(""));

        public string ColorValue
        {
            get { return (string)GetValue(ColorValueProperty); }
            set { SetValue(ColorValueProperty, value); }
        }

        public static readonly DependencyProperty ColorValueProperty =
            DependencyProperty.Register("ColorValue", typeof(string), typeof(ColorTile), new PropertyMetadata(""));

        public bool ShowSeparator
        {
            get { return (bool)GetValue(ShowSeparatorProperty); }
            set { SetValue(ShowSeparatorProperty, value); }
        }

        public static readonly DependencyProperty ShowSeparatorProperty =
            DependencyProperty.Register("ShowSeparator", typeof(bool), typeof(ColorTile), new PropertyMetadata(true));

        public bool ShowWarning
        {
            get { return (bool)GetValue(ShowWarningProperty); }
            set { SetValue(ShowWarningProperty, value); }
        }

        public static readonly DependencyProperty ShowWarningProperty =
            DependencyProperty.Register("ShowWarning", typeof(bool), typeof(ColorTile), new PropertyMetadata(false));

        private static void Copy_ColorBrushName(object sender, RoutedEventArgs e)
        {
            if (!(sender is ColorTile colorTile))
            {
                return;
            }

            if (string.IsNullOrEmpty(colorTile.ColorBrushName))
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
