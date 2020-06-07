using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public static class WindowHelper
    {
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

        #region FixMaximizedWindow

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty FixMaximizedWindowProperty =
            DependencyProperty.RegisterAttached(
                "FixMaximizedWindow",
                typeof(bool),
                typeof(WindowHelper),
                new PropertyMetadata(false, OnFixMaximizedWindowChanged));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool GetFixMaximizedWindow(Window window)
        {
            return (bool)window.GetValue(FixMaximizedWindowProperty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetFixMaximizedWindow(Window window, bool value)
        {
            window.SetValue(FixMaximizedWindowProperty, value);
        }

        private static void OnFixMaximizedWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if ((bool)e.NewValue)
                {
                    MaximizedWindowFixer.SetMaximizedWindowFixer(window, new MaximizedWindowFixer());
                }
                else
                {
                    window.ClearValue(MaximizedWindowFixer.MaximizedWindowFixerProperty);
                }
            }
        }

        #endregion
    }
}
