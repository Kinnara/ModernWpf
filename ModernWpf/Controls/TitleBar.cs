using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public static class TitleBar
    {
        #region Background

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.RegisterAttached(
                "Background",
                typeof(Brush),
                typeof(TitleBar));

        public static Brush GetBackground(Window window)
        {
            return (Brush)window.GetValue(BackgroundProperty);
        }

        public static void SetBackground(Window window, Brush value)
        {
            window.SetValue(BackgroundProperty, value);
        }

        #endregion

        #region Foreground

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.RegisterAttached(
                "Foreground",
                typeof(Brush),
                typeof(TitleBar));

        public static Brush GetForeground(Window window)
        {
            return (Brush)window.GetValue(ForegroundProperty);
        }

        public static void SetForeground(Window window, Brush value)
        {
            window.SetValue(ForegroundProperty, value);
        }

        #endregion

        #region ButtonBackground

        public static readonly DependencyProperty ButtonBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "ButtonBackground",
                typeof(Brush),
                typeof(TitleBar));

        public static Brush GetButtonBackground(Window window)
        {
            return (Brush)window.GetValue(ButtonBackgroundProperty);
        }

        public static void SetButtonBackground(Window window, Brush value)
        {
            window.SetValue(ButtonBackgroundProperty, value);
        }

        #endregion

        #region ButtonForeground

        public static readonly DependencyProperty ButtonForegroundProperty =
            DependencyProperty.RegisterAttached(
                "ButtonForeground",
                typeof(Brush),
                typeof(TitleBar));

        public static Brush GetButtonForeground(Window window)
        {
            return (Brush)window.GetValue(ButtonForegroundProperty);
        }

        public static void SetButtonForeground(Window window, Brush value)
        {
            window.SetValue(ButtonForegroundProperty, value);
        }

        #endregion

        #region ButtonHoverBackground

        public static readonly DependencyProperty ButtonHoverBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "ButtonHoverBackground",
                typeof(Brush),
                typeof(TitleBar));

        public static Brush GetButtonHoverBackground(Window window)
        {
            return (Brush)window.GetValue(ButtonHoverBackgroundProperty);
        }

        public static void SetButtonHoverBackground(Window window, Brush value)
        {
            window.SetValue(ButtonHoverBackgroundProperty, value);
        }

        #endregion

        #region ButtonHoverForeground

        public static readonly DependencyProperty ButtonHoverForegroundProperty =
            DependencyProperty.RegisterAttached(
                "ButtonHoverForeground",
                typeof(Brush),
                typeof(TitleBar));

        public static Brush GetButtonHoverForeground(Window window)
        {
            return (Brush)window.GetValue(ButtonHoverForegroundProperty);
        }

        public static void SetButtonHoverForeground(Window window, Brush value)
        {
            window.SetValue(ButtonHoverForegroundProperty, value);
        }

        #endregion

        #region ButtonPressedBackground

        public static readonly DependencyProperty ButtonPressedBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "ButtonPressedBackground",
                typeof(Brush),
                typeof(TitleBar));

        public static Brush GetButtonPressedBackground(Window window)
        {
            return (Brush)window.GetValue(ButtonPressedBackgroundProperty);
        }

        public static void SetButtonPressedBackground(Window window, Brush value)
        {
            window.SetValue(ButtonPressedBackgroundProperty, value);
        }

        #endregion

        #region ButtonPressedForeground

        public static readonly DependencyProperty ButtonPressedForegroundProperty =
            DependencyProperty.RegisterAttached(
                "ButtonPressedForeground",
                typeof(Brush),
                typeof(TitleBar));

        public static Brush GetButtonPressedForeground(Window window)
        {
            return (Brush)window.GetValue(ButtonPressedForegroundProperty);
        }

        public static void SetButtonPressedForeground(Window window, Brush value)
        {
            window.SetValue(ButtonPressedForegroundProperty, value);
        }

        #endregion

        #region InactiveBackground

        public static readonly DependencyProperty InactiveBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "InactiveBackground",
                typeof(Brush),
                typeof(TitleBar));

        public static Brush GetInactiveBackground(Window window)
        {
            return (Brush)window.GetValue(InactiveBackgroundProperty);
        }

        public static void SetInactiveBackground(Window window, Brush value)
        {
            window.SetValue(InactiveBackgroundProperty, value);
        }

        #endregion

        #region InactiveForeground

        public static readonly DependencyProperty InactiveForegroundProperty =
            DependencyProperty.RegisterAttached(
                "InactiveForeground",
                typeof(Brush),
                typeof(TitleBar));

        public static Brush GetInactiveForeground(Window window)
        {
            return (Brush)window.GetValue(InactiveForegroundProperty);
        }

        public static void SetInactiveForeground(Window window, Brush value)
        {
            window.SetValue(InactiveForegroundProperty, value);
        }

        #endregion

        #region ButtonInactiveBackground

        public static readonly DependencyProperty ButtonInactiveBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "ButtonInactiveBackground",
                typeof(Brush),
                typeof(TitleBar));

        public static Brush GetButtonInactiveBackground(Window window)
        {
            return (Brush)window.GetValue(ButtonInactiveBackgroundProperty);
        }

        public static void SetButtonInactiveBackground(Window window, Brush value)
        {
            window.SetValue(ButtonInactiveBackgroundProperty, value);
        }

        #endregion

        #region ButtonInactiveForeground

        public static readonly DependencyProperty ButtonInactiveForegroundProperty =
            DependencyProperty.RegisterAttached(
                "ButtonInactiveForeground",
                typeof(Brush),
                typeof(TitleBar));

        public static Brush GetButtonInactiveForeground(Window window)
        {
            return (Brush)window.GetValue(ButtonInactiveForegroundProperty);
        }

        public static void SetButtonInactiveForeground(Window window, Brush value)
        {
            window.SetValue(ButtonInactiveForegroundProperty, value);
        }

        #endregion

        #region IsIconVisible

        public static readonly DependencyProperty IsIconVisibleProperty =
            DependencyProperty.RegisterAttached(
                "IsIconVisible",
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(false));

        public static bool GetIsIconVisible(Window window)
        {
            return (bool)window.GetValue(IsIconVisibleProperty);
        }

        public static void SetIsIconVisible(Window window, bool value)
        {
            window.SetValue(IsIconVisibleProperty, value);
        }

        #endregion

        #region IsBackButtonVisible

        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            DependencyProperty.RegisterAttached(
                "IsBackButtonVisible",
                typeof(bool),
                typeof(TitleBar));

        public static bool GetIsBackButtonVisible(Window window)
        {
            return (bool)window.GetValue(IsBackButtonVisibleProperty);
        }

        public static void SetIsBackButtonVisible(Window window, bool value)
        {
            window.SetValue(IsBackButtonVisibleProperty, value);
        }

        #endregion

        #region IsBackEnabled

        /// <summary>
        /// Identifies the IsBackEnabled attached property.
        /// </summary>
        public static readonly DependencyProperty IsBackEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsBackEnabled",
                typeof(bool),
                typeof(TitleBar));

        /// <summary>
        /// Gets a value that indicates whether the back button is enabled or disabled.
        /// </summary>
        /// <param name="window">The element from which to read the property value.</param>
        /// <returns>true if the back button is enabled; otherwise, false. The default is false.</returns>
        public static bool GetIsBackEnabled(Window window)
        {
            return (bool)window.GetValue(IsBackEnabledProperty);
        }

        /// <summary>
        /// Sets a value that indicates whether the back button is enabled or disabled.
        /// </summary>
        /// <param name="window">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetIsBackEnabled(Window window, bool value)
        {
            window.SetValue(IsBackEnabledProperty, value);
        }

        #endregion

        #region BackButtonCommand

        public static readonly DependencyProperty BackButtonCommandProperty =
            DependencyProperty.RegisterAttached(
                "BackButtonCommand",
                typeof(ICommand),
                typeof(TitleBar));

        public static ICommand GetBackButtonCommand(Window window)
        {
            return (ICommand)window.GetValue(BackButtonCommandProperty);
        }

        public static void SetBackButtonCommand(Window window, ICommand value)
        {
            window.SetValue(BackButtonCommandProperty, value);
        }

        #endregion

        #region BackButtonCommandParameter

        public static readonly DependencyProperty BackButtonCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "BackButtonCommandParameter",
                typeof(object),
                typeof(TitleBar));

        public static object GetBackButtonCommandParameter(Window window)
        {
            return window.GetValue(BackButtonCommandParameterProperty);
        }

        public static void SetBackButtonCommandParameter(Window window, object value)
        {
            window.SetValue(BackButtonCommandParameterProperty, value);
        }

        #endregion

        #region ExtendViewIntoTitleBar

        public static readonly DependencyProperty ExtendViewIntoTitleBarProperty =
            DependencyProperty.RegisterAttached(
                "ExtendViewIntoTitleBar",
                typeof(bool),
                typeof(TitleBar),
                new PropertyMetadata(false));

        public static bool GetExtendViewIntoTitleBar(Window window)
        {
            return (bool)window.GetValue(ExtendViewIntoTitleBarProperty);
        }

        public static void SetExtendViewIntoTitleBar(Window window, bool value)
        {
            window.SetValue(ExtendViewIntoTitleBarProperty, value);
        }

        #endregion

        #region BackRequested

        /// <summary>
        /// Identifies the BackRequested routed event.
        /// </summary>
        public static readonly RoutedEvent BackRequestedEvent =
            EventManager.RegisterRoutedEvent(
                "BackRequested",
                RoutingStrategy.Bubble,
                typeof(EventHandler<BackRequestedEventArgs>),
                typeof(TitleBar));

        public static void AddBackRequestedHandler(Window window, EventHandler<BackRequestedEventArgs> handler)
        {
            window.AddHandler(BackRequestedEvent, handler);
        }

        public static void RemoveBackRequestedHandler(Window window, EventHandler<BackRequestedEventArgs> handler)
        {
            window.RemoveHandler(BackRequestedEvent, handler);
        }

        internal static void RaiseBackRequested(Window window)
        {
            window.RaiseEvent(new BackRequestedEventArgs(window));
        }

        #endregion

        #region InternalBackRequested

        internal static readonly RoutedEvent InternalBackRequestedEvent =
            EventManager.RegisterRoutedEvent(
                "InternalBackRequested",
                RoutingStrategy.Tunnel,
                typeof(EventHandler<BackRequestedEventArgs>),
                typeof(TitleBar));

        internal static void AddInternalBackRequestedHandler(Window window, EventHandler<BackRequestedEventArgs> handler)
        {
            window.AddHandler(InternalBackRequestedEvent, handler);
        }

        internal static void RemoveInternalBackRequestedHandler(Window window, EventHandler<BackRequestedEventArgs> handler)
        {
            window.RemoveHandler(InternalBackRequestedEvent, handler);
        }

        #endregion
    }
}
