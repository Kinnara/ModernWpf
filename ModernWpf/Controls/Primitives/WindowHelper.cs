using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

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

        #region IsAutoPaddingEnabled

        public static readonly DependencyProperty IsAutoPaddingEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsAutoPaddingEnabled",
                typeof(bool),
                typeof(WindowHelper),
                new PropertyMetadata(false, OnIsAutoPaddingEnabledChanged));

        public static bool GetIsAutoPaddingEnabled(Window window)
        {
            return (bool)window.GetValue(IsAutoPaddingEnabledProperty);
        }

        public static void SetIsAutoPaddingEnabled(Window window, bool value)
        {
            window.SetValue(IsAutoPaddingEnabledProperty, value);
        }

        private static void OnIsAutoPaddingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if ((bool)e.NewValue)
                {
#if NETCOREAPP || NET462
                    window.DpiChanged += OnDpiChanged;
                    window.SetValue(DpiProperty, VisualTreeHelper.GetDpi(window));
#endif
                    var binding = new MultiBinding
                    {
                        Bindings =
                        {
                            new Binding { Path = new PropertyPath(Window.WindowStateProperty), Source = window },
                            new Binding("(SystemParameters.WindowResizeBorderThickness)"),
#if NETCOREAPP || NET462
                            new Binding { Path = new PropertyPath(DpiProperty), Source = window },
#endif
                        },
                        Converter = new PaddingConveter()
                    };
                    window.SetBinding(Control.PaddingProperty, binding);
                }
                else
                {
#if NETCOREAPP || NET462
                    window.DpiChanged -= OnDpiChanged;
                    window.ClearValue(DpiProperty);
#endif
                    window.ClearValue(Control.PaddingProperty);
                }
            }
        }

        #endregion

#if NETCOREAPP || NET462
        private static readonly DependencyProperty DpiProperty =
            DependencyProperty.RegisterAttached(
                "Dpi",
                typeof(DpiScale),
                typeof(WindowHelper),
                new PropertyMetadata(new DpiScale(1, 1)));

        private static void OnDpiChanged(object sender, DpiChangedEventArgs e)
        {
            ((Window)sender).SetValue(DpiProperty, e.NewDpi);
        }
#endif

        private class PaddingConveter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                if (values.Length >= 2 &&
                    values[0] is WindowState windowState)
                {
                    if (windowState == WindowState.Maximized)
                    {
#if NETCOREAPP || NET462
                        if (values.Length == 3 &&
                            values[1] is Thickness windowResizeBorderThickness &&
                            values[2] is DpiScale dpi)
                        {
                            Size frameSize = new Size(NativeMethods.GetSystemMetrics(SM_CXSIZEFRAME),
                                                      NativeMethods.GetSystemMetrics(SM_CYSIZEFRAME));
                            Size frameSizeInDips = Standard.DpiHelper.DeviceSizeToLogical(frameSize, dpi.DpiScaleX, dpi.DpiScaleY);

                            int borderPadding = NativeMethods.GetSystemMetrics(SM_CXPADDEDBORDER);
                            Size borderPaddingSize = new Size(borderPadding, borderPadding);
                            Size borderPaddingSizeInDips = Standard.DpiHelper.DeviceSizeToLogical(borderPaddingSize, dpi.DpiScaleX, dpi.DpiScaleY);

                            double leftRight = frameSizeInDips.Width + borderPaddingSizeInDips.Width;
                            double topBottom = frameSizeInDips.Height + borderPaddingSizeInDips.Height;
                            return new Thickness(leftRight, topBottom, leftRight, topBottom);
                        }
#else
                        if (values.Length == 2 &&
                            values[1] is Thickness windowResizeBorderThickness)
                        {
                            const int borderPadding = 4;
                            return new Thickness(windowResizeBorderThickness.Left + borderPadding,
                                                 windowResizeBorderThickness.Top + borderPadding,
                                                 windowResizeBorderThickness.Right + borderPadding,
                                                 windowResizeBorderThickness.Bottom + borderPadding);
                        }
#endif
                    }
                }

                return new Thickness();
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }

#if NETCOREAPP || NET462
            private static class NativeMethods
            {
                [DllImport("user32.dll")]
                public static extern int GetSystemMetrics(int nIndex);
            }

            private const int SM_CXSIZEFRAME = 32;
            private const int SM_CYSIZEFRAME = 33;
            private const int SM_CXPADDEDBORDER = 92;
#endif
        }
    }
}
