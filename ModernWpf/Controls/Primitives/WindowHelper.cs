using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ModernWpf.Controls.Primitives
{
    public static class WindowHelper
    {
        private const string DefaultWindowStyleKey = "DefaultWindowStyle";
        private const string AeroWindowStyleKey = "AeroWindowStyle";
        private const string AcrylicWindowStyleKey = "AcrylicWindowStyle";
        private const string SnapWindowStyleKey = "SnapWindowStyle";

        #region UseModernWindowStyle

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
                SetWindowStyle(window);
            }
        }

        #endregion

        #region UseAeroBackdrop

        public static readonly DependencyProperty UseAeroBackdropProperty =
            DependencyProperty.RegisterAttached(
                "UseAeroBackdrop",
                typeof(bool),
                typeof(WindowHelper),
                new PropertyMetadata(OnUseAeroBackdropChanged));

        public static bool GetUseAeroBackdrop(Window window)
        {
            return (bool)window.GetValue(UseAeroBackdropProperty);
        }

        public static void SetUseAeroBackdrop(Window window, bool value)
        {
            window.SetValue(UseAeroBackdropProperty, value);
        }

        private static void OnUseAeroBackdropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (OSVersionHelper.OSVersion < new Version(6, 0) || new Version(6, 3) <= OSVersionHelper.OSVersion)
            {
                return;
            }

            if (d is Window window)
            {
                SetWindowStyle(window);
            }
        }

        #endregion

        #region UseAcrylicBackdrop

        public static readonly DependencyProperty UseAcrylicBackdropProperty =
            DependencyProperty.RegisterAttached(
                "UseAcrylicBackdrop",
                typeof(bool),
                typeof(WindowHelper),
                new PropertyMetadata(OnUseAcrylicBackdropChanged));

        public static bool GetUseAcrylicBackdrop(Window window)
        {
            return (bool)window.GetValue(UseAcrylicBackdropProperty);
        }

        public static void SetUseAcrylicBackdrop(Window window, bool value)
        {
            window.SetValue(UseAcrylicBackdropProperty, value);
        }

        private static void OnUseAcrylicBackdropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!AcrylicHelper.IsSupported())
            {
                return;
            }

            if (d is Window window)
            {
                var handler = new RoutedEventHandler(async (sender, e) =>
                {
                    await Task.Delay(1);
                    window.Apply();
                });

                SetWindowStyle(window);

                if ((bool)e.NewValue)
                {
                    window.Apply();

                    if (!window.IsLoaded)
                    {
                        window.Loaded += (sender, e) => window.Apply();
                    }

                    if (AcrylicHelper.IsAcrylicSupported())
                    {
                        ThemeManager.RemoveActualThemeChangedHandler(window, handler);
                        ThemeManager.AddActualThemeChangedHandler(window, handler);
                    }
                }
                else
                {
                    AcrylicHelper.Remove(window);
                    ThemeManager.RemoveActualThemeChangedHandler(window, handler);
                }
            }
        }

        #endregion

        #region SystemBackdropType

        public static readonly DependencyProperty SystemBackdropTypeProperty =
            DependencyProperty.RegisterAttached(
                "SystemBackdropType",
                typeof(BackdropType),
                typeof(WindowHelper),
                new PropertyMetadata(OnSystemBackdropTypeChanged));

        public static BackdropType GetSystemBackdropType(Window window)
        {
            return (BackdropType)window.GetValue(SystemBackdropTypeProperty);
        }

        public static void SetSystemBackdropType(Window window, BackdropType value)
        {
            window.SetValue(SystemBackdropTypeProperty, value);
        }

        private static void OnSystemBackdropTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!MicaHelper.IsSupported((BackdropType)e.NewValue))
            {
                return;
            }

            if (d is Window window)
            {
                SetWindowStyle(window);
                window.Apply((BackdropType)e.NewValue);
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

        public static void SetWindowStyle(Window window)
        {
            bool isModern = DependencyPropertyHelper.GetValueSource(window, UseModernWindowStyleProperty).BaseValueSource != BaseValueSource.Default && GetUseModernWindowStyle(window);
            bool isUseMica = DependencyPropertyHelper.GetValueSource(window, SystemBackdropTypeProperty).BaseValueSource != BaseValueSource.Default;
            bool isUseAcrylic = DependencyPropertyHelper.GetValueSource(window, UseAcrylicBackdropProperty).BaseValueSource != BaseValueSource.Default && GetUseAcrylicBackdrop(window);
            bool isUseAero = DependencyPropertyHelper.GetValueSource(window, UseAeroBackdropProperty).BaseValueSource != BaseValueSource.Default && GetUseAeroBackdrop(window);

            bool isSetMica = false;
            bool isSetAcrylic = false;
            bool isSetAero = false;

            var handler = new RoutedEventHandler((sender, e) =>
            {
                var theme = ThemeManager.GetActualTheme(window);

                bool IsDark(ElementTheme theme)
                {
                    return theme == ElementTheme.Default
                        ? ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark
                        : theme == ElementTheme.Dark;
                }

                if (IsDark(theme))
                {
                    window.ApplyDarkMode();
                }
                else
                {
                    window.RemoveDarkMode();
                }
            });

            if (isModern)
            {
                if (window.IsLoaded)
                {
                    window.RemoveTitleBar();
                }
                else
                {
                    window.Loaded += (sender, e) => window.RemoveTitleBar();
                    ThemeManager.RemoveActualThemeChangedHandler(window, handler);
                    ThemeManager.AddActualThemeChangedHandler(window, handler);
                }

                if (isUseMica)
                {
                    var type = GetSystemBackdropType(window);
                    if (MicaHelper.IsSupported(type))
                    {
                        isSetMica = true;
                        window.SetResourceReference(FrameworkElement.StyleProperty, AeroWindowStyleKey);
                    }
                }

                if (!isSetMica && isUseAcrylic)
                {
                    if (AcrylicHelper.IsAcrylicSupported())
                    {
                        isSetAcrylic = true;
                        window.SetResourceReference(FrameworkElement.StyleProperty, AcrylicWindowStyleKey);
                    }
                    else if (AcrylicHelper.IsSupported())
                    {
                        isSetAcrylic = true;
                        window.SetResourceReference(FrameworkElement.StyleProperty, AeroWindowStyleKey);
                    }
                }

                if (!isSetMica && !isSetAcrylic && isUseAero)
                {
                    if (new Version(6, 0) <= OSVersionHelper.OSVersion && OSVersionHelper.OSVersion < new Version(6, 3))
                    {
                        isSetAero = true;
                        window.SetResourceReference(FrameworkElement.StyleProperty, AeroWindowStyleKey);
                    }
                }

                if (!isSetMica && !isSetAcrylic && !isSetAero)
                {
                    if (OSVersionHelper.IsWindows11OrGreater)
                    {
                        window.SetResourceReference(FrameworkElement.StyleProperty, SnapWindowStyleKey);
                    }
                    else
                    {
                        window.SetResourceReference(FrameworkElement.StyleProperty, DefaultWindowStyleKey);
                    }
                }
            }
            else
            {
                window.ClearValue(FrameworkElement.StyleProperty);
                window.RemoveDarkMode();
                ThemeManager.RemoveActualThemeChangedHandler(window, handler);
            }
        }
    }
}
