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
                element.MouseLeave += OnMouseLeave;
            }
            else
            {
                element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
                element.LostMouseCapture -= OnLostMouseCapture;
                element.MouseLeave -= OnMouseLeave;
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
                RoutingStrategy.Bubble,
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

        private static void RaiseTapped(UIElement element, int timestamp)
        {
            var e = new TappedRoutedEventArgs { RoutedEvent = TappedEvent, Source = element, Timestamp = timestamp };
            _lastTappedArgs = e;
            element.RaiseEvent(e);
        }

        #endregion

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;

            if (!GetIsPressed(element))
            {
                SetIsPressed(element, true);
            }
        }

        private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;

            if (GetIsPressed(element))
            {
                SetIsPressed((UIElement)sender, false);

                var lastArgs = _lastTappedArgs;

                if (lastArgs != null && lastArgs.Handled && lastArgs.Timestamp == e.Timestamp)
                {
                    // Handled by a child element, don't raise
                }
                else
                {
                    var elementBounds = new Rect(new Point(), element.RenderSize);
                    if (elementBounds.Contains(e.GetPosition(element)))
                    {
                        RaiseTapped(element, e.Timestamp);
                    }
                }
            }
        }

        private static void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            SetIsPressed((UIElement)sender, false);
        }

        private static void OnMouseLeave(object sender, MouseEventArgs e)
        {
            SetIsPressed((UIElement)sender, false);
        }

        private static TappedRoutedEventArgs _lastTappedArgs;
    }
}
