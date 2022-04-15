using MS.Win32;
using Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public enum BackdropType
    {
        Auto = 1,
        Mica = 2,
        Acrylic = 3,
        Tabbed = 4
    }

    internal static class MicaHelper
    {
        /// <summary>
        /// Checks if the current <see cref="Windows"/> supports selected <see cref="BackgroundType"/>.
        /// </summary>
        /// <param name="type">Background type to check.</param>
        /// <returns><see langword="true"/> if <see cref="BackgroundType"/> is supported.</returns>
        public static bool IsSupported(this BackdropType type)
        {
            if (!OSVersionHelper.IsWindowsNT) { return false; }

            return type switch
            {
                BackdropType.Auto => OSVersionHelper.OSVersion >= new Version(10, 0, 22523), // Insider with new API                
                BackdropType.Tabbed => OSVersionHelper.OSVersion >= new Version(10, 0, 22523),
                BackdropType.Mica => OSVersionHelper.OSVersion >= new Version(10, 0, 22000),
                BackdropType.Acrylic => (OSVersionHelper.OSVersion >= new Version(6, 0) && OSVersionHelper.OSVersion < new Version(6, 3)) || OSVersionHelper.OSVersion >= new Version(10, 0),
                _ => false
            };
        }

        /// <summary>
        /// Applies selected background effect to <see cref="Window"/> when is rendered.
        /// </summary>
        /// <param name="window">Window to apply effect.</param>
        /// <param name="type">Background type.</param>
        /// <param name="force">Skip the compatibility check.</param>
        public static bool Apply(this Window window, BackdropType type, bool force = false)
        {
            if (!force && (!IsSupported(type))) { return false; }

            var windowHandle = new WindowInteropHelper(window).EnsureHandle();

            if (windowHandle == IntPtr.Zero) { return false; }

            void SetStyle()
            {
                if (window.Style != null)
                {
                    foreach (Setter setter in window.Style.Setters)
                    {
                        if (setter.Property == Control.BackgroundProperty && setter.Value == Brushes.Transparent)
                        {
                            goto stylesetted;
                        }
                    }
                    Style style = new Style
                    {
                        TargetType = typeof(Window),
                        BasedOn = window.Style
                    };
                    style.Setters.Add(new Setter
                    {
                        Property = FrameworkElement.TagProperty,
                        Value = true
                    });
                    style.Setters.Add(new Setter
                    {
                        Property = Control.BackgroundProperty,
                        Value = Brushes.Transparent
                    });
                    window.Style = style;
                stylesetted:;
                }
                else
                {
                    Style style = new Style
                    {
                        TargetType = typeof(Window)
                    };
                    style.Setters.Add(new Setter
                    {
                        Property = FrameworkElement.TagProperty,
                        Value = true
                    });
                    style.Setters.Add(new Setter
                    {
                        Property = Control.BackgroundProperty,
                        Value = Brushes.Transparent
                    });
                    window.Style = style;
                }
            }

            if (window.IsLoaded)
            {
                SetStyle();
            }
            else
            {
                window.Loaded += (sender, e) => SetStyle();
            }

            Apply(windowHandle, type);

            return true;
        }

        /// <summary>
        /// Applies selected background effect to <c>hWnd</c> by it's pointer.
        /// </summary>
        /// <param name="handle">Pointer to the window handle.</param>
        /// <param name="type">Background type.</param>
        /// <param name="force">Skip the compatibility check.</param>
        public static bool Apply(this IntPtr handle, BackdropType type, bool force = false)
        {
            if (!force && (!IsSupported(type))) { return false; }

            if (handle == IntPtr.Zero) { return false; }

            if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark) { ApplyDarkMode(handle); }

            return type switch
            {
                BackdropType.Auto => TryApplyAuto(handle),
                BackdropType.Mica => TryApplyMica(handle),
                BackdropType.Acrylic => TryApplyAcrylic(handle),
                BackdropType.Tabbed => TryApplyTabbed(handle),
                _ => false
            };
        }

        /// <summary>
        /// Tries to remove background effects if they have been applied to the <see cref="Window"/>.
        /// </summary>
        /// <param name="window">The window from which the effect should be removed.</param>
        public static void Remove(this Window window)
        {
            var windowHandle = new WindowInteropHelper(window).EnsureHandle();

            if (windowHandle == IntPtr.Zero) return;

            if (window.Style != null)
            {
                foreach(Setter setter in window.Style.Setters)
                {
                    if(setter.Property == FrameworkElement.TagProperty&& setter.Value is bool boolen && boolen)
                    {
                        if (window.Style.BasedOn != null)
                        {
                            window.Style = window.Style.BasedOn;
                        }
                        else
                        {
                            window.ClearValue(FrameworkElement.StyleProperty);
                        }
                    }
                }
            }

            Remove(windowHandle);
        }

        /// <summary>
        /// Tries to remove all effects if they have been applied to the <c>hWnd</c>.
        /// </summary>
        /// <param name="handle">Pointer to the window handle.</param>
        public static void Remove(this IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;

            int pvAttribute = (int)DWMAPI.PvAttribute.Disable;
            int backdropPvAttribute = (int)DWMAPI.DWMSBT.DWMSBT_DISABLE;

            RemoveDarkMode(handle);

            DWMAPI.DwmSetWindowAttribute(handle, DWMAPI.DWMWINDOWATTRIBUTE.DWMWA_MICA_EFFECT, ref pvAttribute,
                Marshal.SizeOf(typeof(int)));

            DWMAPI.DwmSetWindowAttribute(handle, DWMAPI.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                ref backdropPvAttribute,
                Marshal.SizeOf(typeof(int)));
        }

        /// <summary>
        /// Tries to inform the operating system that this window uses dark mode.
        /// </summary>
        /// <param name="window">Window to apply effect.</param>
        public static void ApplyDarkMode(this Window window)
        {
            var windowHandle = new WindowInteropHelper(window).EnsureHandle();

            if (windowHandle == IntPtr.Zero) return;

            ApplyDarkMode(windowHandle);
        }

        /// <summary>
        /// Tries to inform the operating system that this <c>hWnd</c> uses dark mode.
        /// </summary>
        /// <param name="handle">Pointer to the window handle.</param>
        public static void ApplyDarkMode(this IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;

            var pvAttribute = (int)DWMAPI.PvAttribute.Enable;
            var dwAttribute = DWMAPI.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE;

            if (OSVersionHelper.OSVersion < new Version(10, 0, 18985))
            {
                dwAttribute = DWMAPI.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE_OLD;
            }

            DWMAPI.DwmSetWindowAttribute(handle, dwAttribute,
                ref pvAttribute,
                Marshal.SizeOf(typeof(int)));
        }

        /// <summary>
        /// Tries to clear the dark theme usage information.
        /// </summary>
        /// <param name="window">Window to remove effect.</param>
        public static void RemoveDarkMode(this Window window)
        {
            var windowHandle = new WindowInteropHelper(window).EnsureHandle();

            if (windowHandle == IntPtr.Zero) return;

            RemoveDarkMode(windowHandle);
        }

        /// <summary>
        /// Tries to clear the dark theme usage information.
        /// </summary>
        /// <param name="handle">Pointer to the window handle.</param>
        public static void RemoveDarkMode(this IntPtr handle)
        {
            if (handle == IntPtr.Zero) { return; }

            var pvAttribute = (int)DWMAPI.PvAttribute.Disable;
            var dwAttribute = DWMAPI.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE;

            if (OSVersionHelper.OSVersion < new Version(10, 0, 18985))
            {
                dwAttribute = DWMAPI.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE_OLD;
            }

            DWMAPI.DwmSetWindowAttribute(handle, dwAttribute,
                ref pvAttribute,
                Marshal.SizeOf(typeof(int)));
        }

        /// <summary>
        /// Tries to remove default TitleBar from <c>hWnd</c>.
        /// </summary>
        /// <param name="window">Window to remove effect.</param>
        public static void RemoveTitleBar(this Window window)
        {
            var windowHandle = new WindowInteropHelper(window).EnsureHandle();

            if (windowHandle == IntPtr.Zero) return;

            RemoveTitleBar(windowHandle);
        }

        /// <summary>
        /// Tries to remove default TitleBar from <c>hWnd</c>.
        /// </summary>
        /// <param name="handle">Pointer to the window handle.</param>
        /// <returns><see langowrd="false"/> is problem occurs.</returns>
        private static bool RemoveTitleBar(this IntPtr handle)
        {
            // Hide default TitleBar
            // https://stackoverflow.com/questions/743906/how-to-hide-close-button-in-wpf-window
            try
            {
                User32.SetWindowLong(handle, -16, User32.GetWindowLong(handle, -16) & ~0x80000);

                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                return false;
            }
        }

        private static bool TryApplyAuto(this IntPtr handle)
        {
            int backdropPvAttribute = (int)DWMAPI.DWMSBT.DWMSBT_AUTO;

            DWMAPI.DwmSetWindowAttribute(handle, DWMAPI.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                ref backdropPvAttribute,
                Marshal.SizeOf(typeof(int)));

            return true;
        }

        private static bool TryApplyTabbed(this IntPtr handle)
        {
            int backdropPvAttribute = (int)DWMAPI.DWMSBT.DWMSBT_TABBEDWINDOW;

            DWMAPI.DwmSetWindowAttribute(handle, DWMAPI.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                ref backdropPvAttribute,
                Marshal.SizeOf(typeof(int)));

            return true;
        }

        private static bool TryApplyMica(this IntPtr handle)
        {
            int backdropPvAttribute;

            if (OSVersionHelper.OSVersion>= new Version(10,0,22523))
            {
                backdropPvAttribute = (int)DWMAPI.DWMSBT.DWMSBT_MAINWINDOW;

                DWMAPI.DwmSetWindowAttribute(handle, DWMAPI.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                    ref backdropPvAttribute,
                    Marshal.SizeOf(typeof(int)));

                return true;
            }

            if (!RemoveTitleBar(handle)) { return false; }

            backdropPvAttribute = (int)DWMAPI.PvAttribute.Enable;

            DWMAPI.DwmSetWindowAttribute(handle, DWMAPI.DWMWINDOWATTRIBUTE.DWMWA_MICA_EFFECT,
                ref backdropPvAttribute,
                Marshal.SizeOf(typeof(int)));

            return true;
        }

        private static bool TryApplyAcrylic(this IntPtr handle)
        {
            if (OSVersionHelper.OSVersion >= new Version(10, 0, 22523))
            {
                int backdropPvAttribute = (int)DWMAPI.DWMSBT.DWMSBT_TRANSIENTWINDOW;

                DWMAPI.DwmSetWindowAttribute(handle, DWMAPI.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                    ref backdropPvAttribute,
                    Marshal.SizeOf(typeof(int)));

                return true;
            }

            if (OSVersionHelper.OSVersion >= new Version(10, 0, 17763))
            {
                User32.ACCENT_POLICY accentPolicy = new User32.ACCENT_POLICY
                {
                    AccentState = User32.ACCENT_STATE.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                    GradientColor = (0 << 24) | (0x990000 & 0xFFFFFF)
                };

                int accentStructSize = Marshal.SizeOf(accentPolicy);

                IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accentPolicy, accentPtr, false);

                User32.WINCOMPATTRDATA data = new User32.WINCOMPATTRDATA
                {
                    Attribute = User32.WINCOMPATTR.WCA_ACCENT_POLICY,
                    SizeOfData = accentStructSize,
                    Data = accentPtr
                };

                User32.SetWindowCompositionAttribute(handle, ref data);

                Marshal.FreeHGlobal(accentPtr);

                return true;
            }

            if (OSVersionHelper.OSVersion >= new Version(10, 0))
            {
                User32.ACCENT_POLICY accentPolicy = new User32.ACCENT_POLICY
                {
                    AccentState = User32.ACCENT_STATE.ACCENT_ENABLE_BLURBEHIND,
                };

                int accentStructSize = Marshal.SizeOf(accentPolicy);

                IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accentPolicy, accentPtr, false);

                User32.WINCOMPATTRDATA data = new User32.WINCOMPATTRDATA
                {
                    Attribute = User32.WINCOMPATTR.WCA_ACCENT_POLICY,
                    SizeOfData = accentStructSize,
                    Data = accentPtr
                };

                User32.SetWindowCompositionAttribute(handle, ref data);

                Marshal.FreeHGlobal(accentPtr);

                return true;
            }

            if (OSVersionHelper.OSVersion >= new Version(6, 0))
            {
                return true;
            }

            return false;
        }
    }
}
