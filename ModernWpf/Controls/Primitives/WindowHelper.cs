using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public static class WindowHelper
    {
        private static readonly CommandBinding MinimizeWindowCommandBinding;
        private static readonly CommandBinding MaximizeWindowCommandBinding;
        private static readonly CommandBinding RestoreWindowCommandBinding;
        private static readonly CommandBinding CloseWindowCommandBinding;

        static WindowHelper()
        {
            MinimizeWindowCommandBinding = new CommandBinding(SystemCommands.MinimizeWindowCommand,
                (sender, _) => SystemCommands.MinimizeWindow((Window)sender));

            MaximizeWindowCommandBinding = new CommandBinding(SystemCommands.MaximizeWindowCommand,
                (sender, _) => SystemCommands.MaximizeWindow((Window)sender));

            RestoreWindowCommandBinding = new CommandBinding(SystemCommands.RestoreWindowCommand,
                (sender, _) => SystemCommands.RestoreWindow((Window)sender));

            CloseWindowCommandBinding = new CommandBinding(SystemCommands.CloseWindowCommand,
                (sender, _) => SystemCommands.CloseWindow((Window)sender));
        }

        #region IsEnabled

        public static bool GetIsEnabled(Window window)
        {
            return (bool)window.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(Window window, bool value)
        {
            window.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(WindowHelper),
                new PropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if ((bool)e.NewValue)
                {
                    window.CommandBindings.Add(MinimizeWindowCommandBinding);
                    window.CommandBindings.Add(MaximizeWindowCommandBinding);
                    window.CommandBindings.Add(RestoreWindowCommandBinding);
                    window.CommandBindings.Add(CloseWindowCommandBinding);

                    window.SetBinding(
                        ApplicationThemeProperty,
                        new Binding
                        {
                            Path = new PropertyPath(ThemeManager.ActualApplicationThemeProperty),
                            Source = ThemeManager.Current
                        });

                    ThemeManager.UpdateWindowActualTheme((Window)d);
                }
                else
                {
                    window.CommandBindings.Remove(MinimizeWindowCommandBinding);
                    window.CommandBindings.Remove(MaximizeWindowCommandBinding);
                    window.CommandBindings.Remove(RestoreWindowCommandBinding);
                    window.CommandBindings.Remove(CloseWindowCommandBinding);

                    window.ClearValue(ApplicationThemeProperty);
                }
            }
        }

        #endregion

        #region IsBackButtonVisible

        public static bool GetIsBackButtonVisible(Window window)
        {
            return (bool)window.GetValue(IsBackButtonVisibleProperty);
        }

        public static void SetIsBackButtonVisible(Window window, bool value)
        {
            window.SetValue(IsBackButtonVisibleProperty, value);
        }

        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            DependencyProperty.RegisterAttached(
                "IsBackButtonVisible",
                typeof(bool),
                typeof(WindowHelper));

        #endregion

        #region IsBackEnabled

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

        /// <summary>
        /// Identifies the IsBackEnabled attached property.
        /// </summary>
        public static readonly DependencyProperty IsBackEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsBackEnabled",
                typeof(bool),
                typeof(WindowHelper));

        #endregion

        #region BackRequested

        /// <summary>
        /// Identifies the BackRequested routed event.
        /// </summary>
        public static readonly RoutedEvent BackRequestedEvent =
            EventManager.RegisterRoutedEvent(
                "BackRequested",
                RoutingStrategy.Tunnel,
                typeof(EventHandler<BackRequestedEventArgs>),
                typeof(WindowHelper));

        public static void AddBackRequestedHandler(Window window, EventHandler<BackRequestedEventArgs> handler)
        {
            window.AddHandler(BackRequestedEvent, handler);
        }

        public static void RemoveBackRequestedHandler(Window window, EventHandler<BackRequestedEventArgs> handler)
        {
            window.RemoveHandler(BackRequestedEvent, handler);
        }

        private static void RaiseBackRequested(Window window)
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
                typeof(WindowHelper));

        internal static void AddInternalBackRequestedHandler(Window window, EventHandler<BackRequestedEventArgs> handler)
        {
            window.AddHandler(InternalBackRequestedEvent, handler);
        }

        internal static void RemoveInternalBackRequestedHandler(Window window, EventHandler<BackRequestedEventArgs> handler)
        {
            window.RemoveHandler(InternalBackRequestedEvent, handler);
        }

        #endregion

        #region IsBackButton

        public static readonly DependencyProperty IsBackButtonProperty =
            DependencyProperty.RegisterAttached(
                "IsBackButton",
                typeof(bool),
                typeof(WindowHelper),
                new PropertyMetadata(OnIsBackButtonChanged));

        public static bool GetIsBackButton(Button button)
        {
            return (bool)button.GetValue(IsBackButtonProperty);
        }

        public static void SetIsBackButton(Button button, bool value)
        {
            button.SetValue(IsBackButtonProperty, value);
        }

        private static void OnIsBackButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (Button)d;
            if ((bool)e.NewValue)
            {
                button.Click += OnBackButtonClick;
            }
            else
            {
                button.Click -= OnBackButtonClick;
            }
        }

        private static void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            var backButton = (Button)sender;
            if (backButton.TemplatedParent is Window window)
            {
                var internalArgs = new BackRequestedEventArgs(InternalBackRequestedEvent, window);
                window.RaiseEvent(internalArgs);
                if (!internalArgs.Handled)
                {
                    RaiseBackRequested(window);
                }
            }
        }

        #endregion

        #region ApplicationTheme

        private static readonly DependencyProperty ApplicationThemeProperty =
            DependencyProperty.RegisterAttached(
                "ApplicationTheme",
                typeof(ApplicationTheme?),
                typeof(WindowHelper),
                new PropertyMetadata(OnApplicationThemeChanged));

        private static void OnApplicationThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ThemeManager.UpdateWindowActualTheme((Window)d);
        }

        #endregion

        #region UseModernWindowStyle

        private const string DefaultWindowStyleKey = "DefaultWindowStyle";

        public static readonly DependencyProperty UseModernWindowStyleProperty =
            DependencyProperty.RegisterAttached(
                "UseModernWindowStyle",
                typeof(bool),
                typeof(WindowHelper),
                new PropertyMetadata(OnUseModernWindowStyleChanged));

        public static bool GetUseModernWindowStyle(Window window)
        {
            return (bool)window.GetValue(UseModernWindowStyleProperty);
        }

        public static void SetUseModernWindowStyle(Window window, bool value)
        {
            window.SetValue(UseModernWindowStyleProperty, value);
        }

        private static void OnUseModernWindowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d))
            {
                if (d is Control control)
                {
                    if (control.TryFindResource(DefaultWindowStyleKey) is Style style)
                    {
                        var dStyle = new Style();

                        foreach (Setter setter in style.Setters)
                        {
                            if (setter.Property == Control.BackgroundProperty ||
                                setter.Property == Control.ForegroundProperty)
                            {
                                dStyle.Setters.Add(setter);
                            }
                        }

                        control.Style = dStyle;
                    }
                }
            }
            else
            {
                var window = (Window)d;
                window.SetResourceReference(FrameworkElement.StyleProperty, DefaultWindowStyleKey);
            }
        }

        #endregion
    }
}
