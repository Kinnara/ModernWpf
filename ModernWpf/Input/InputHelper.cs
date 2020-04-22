using System.Windows;
using System.Windows.Input;

namespace ModernWpf.Input
{
    internal static class InputHelper
    {
        #region IsTapEnabled

        public static readonly DependencyProperty IsTapEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsTapEnabled",
                typeof(bool),
                typeof(InputHelper),
                new PropertyMetadata(false, OnIsTapEnabledChanged));

        public static bool GetIsTapEnabled(UIElement element)
        {
            return (bool)element.GetValue(IsTapEnabledProperty);
        }

        public static void SetIsTapEnabled(UIElement element, bool value)
        {
            element.SetValue(IsTapEnabledProperty, value);
        }

        private static void OnIsTapEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (UIElement)d;
            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;

            if (newValue)
            {
                element.MouseLeftButtonDown += OnMouseLeftButtonDown;
                element.MouseLeftButtonUp += OnMouseLeftButtonUp;
                element.LostMouseCapture += OnLostMouseCapture;
            }
            else
            {
                element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
                element.LostMouseCapture -= OnLostMouseCapture;
            }
        }

        #endregion

        #region IsPressed

        public static readonly DependencyProperty IsPressedProperty =
            DependencyProperty.RegisterAttached(
                "IsPressed",
                typeof(bool),
                typeof(InputHelper),
                new PropertyMetadata(false));

        private static bool GetIsPressed(UIElement element)
        {
            return (bool)element.GetValue(IsPressedProperty);
        }

        private static void SetIsPressed(UIElement element, bool value)
        {
            if (value)
            {
                element.SetValue(IsPressedProperty, value);
            }
            else
            {
                element.ClearValue(IsPressedProperty);
            }
        }

        #endregion

        #region Tapped

        public static readonly RoutedEvent TappedEvent =
            EventManager.RegisterRoutedEvent(
                "Tapped",
                RoutingStrategy.Direct,
                typeof(TappedEventHandler),
                typeof(InputHelper));

        public static void AddTappedHandler(UIElement element, TappedEventHandler handler)
        {
            element.AddHandler(TappedEvent, handler);
        }

        public static void RemoveTappedHandler(UIElement element, TappedEventHandler handler)
        {
            element.RemoveHandler(TappedEvent, handler);
        }

        private static void RaiseTapped(UIElement element)
        {
            element.RaiseEvent(new TappedRoutedEventArgs { RoutedEvent = TappedEvent, Source = element });
        }

        #endregion

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                var element = (UIElement)sender;
                element.CaptureMouse();
                if (element.IsMouseCaptured)
                {
                    if (e.ButtonState == MouseButtonState.Pressed)
                    {
                        if (!GetIsPressed(element))
                        {
                            SetIsPressed(element, true);
                        }
                    }
                }
            }
        }

        private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;

            bool tapped = GetIsPressed(element);

            if (element.IsMouseCaptured)
            {
                element.ReleaseMouseCapture();
            }

            if (tapped)
            {
                RaiseTapped(element);
            }
        }

        private static void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource == sender)
            {
                SetIsPressed((UIElement)sender, false);
            }
        }
    }
}
