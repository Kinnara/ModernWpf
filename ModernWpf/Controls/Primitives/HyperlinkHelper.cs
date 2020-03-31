using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public static class HyperlinkHelper
    {
        #region IsPressEnabled

        private static readonly DependencyProperty IsPressEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsPressEnabled",
                typeof(bool),
                typeof(HyperlinkHelper),
                new PropertyMetadata(false, OnIsPressEnabledChanged));

        public static bool GetIsPressEnabled(Hyperlink hyperlink)
        {
            return (bool)hyperlink.GetValue(IsPressEnabledProperty);
        }

        public static void SetIsPressEnabled(Hyperlink hyperlink, bool value)
        {
            hyperlink.SetValue(IsPressEnabledProperty, value);
        }

        private static void OnIsPressEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var hyperlink = (Hyperlink)d;

            if ((bool)e.NewValue)
            {
                hyperlink.AddHandler(Hyperlink.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
                hyperlink.AddHandler(Hyperlink.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp), true);
            }
            else
            {
                hyperlink.RemoveHandler(Hyperlink.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown));
                hyperlink.RemoveHandler(Hyperlink.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp));
            }
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {     
            var hyperlink = (Hyperlink)sender;
            if (hyperlink.IsMouseCaptured && e.ButtonState == MouseButtonState.Pressed)
            {
                hyperlink.SetValue(IsPressedPropertyKey, true);
            }
        }

        private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;
            if (GetIsPressed(hyperlink))
            {
                hyperlink.SetValue(IsPressedPropertyKey, false);
            }
        }

        #endregion

        #region IsPressed

        private static readonly DependencyPropertyKey IsPressedPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsPressed",
                typeof(bool),
                typeof(HyperlinkHelper),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;

        public static bool GetIsPressed(Hyperlink hyperlink)
        {
            return (bool)hyperlink.GetValue(IsPressedProperty);
        }

        #endregion
    }
}
