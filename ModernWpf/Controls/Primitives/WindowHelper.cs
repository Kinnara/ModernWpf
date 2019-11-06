using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModernWpf.Controls.Primitives
{
    public static class WindowHelper
    {
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
                    window.ClearValue(ApplicationThemeProperty);
                }
            }
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
            bool newValue = (bool)e.NewValue;

            if (DesignerProperties.GetIsInDesignMode(d))
            {
                if (d is Control control)
                {
                    if (newValue)
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
                    else
                    {
                        control.ClearValue(FrameworkElement.StyleProperty);
                    }
                }
            }
            else
            {
                var window = (Window)d;
                if (newValue)
                {
                    window.SetResourceReference(FrameworkElement.StyleProperty, DefaultWindowStyleKey);
                }
                else
                {
                    window.ClearValue(FrameworkElement.StyleProperty);
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
    }
}
