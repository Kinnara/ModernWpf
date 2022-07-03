using ModernWpf.Controls.Primitives;
using MS.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public static class AcrylicHelper
    {
        /// <summary>
        /// Checks if the current <see cref="Windows"/> supports Aero.
        /// </summary>
        /// <returns><see langword="true"/> if Aero is supported.</returns>
        public static bool IsSupported()
        {
            if (!OSVersionHelper.IsWindowsNT) { return false; }

            if (new Version(10, 0) <= OSVersionHelper.OSVersion && OSVersionHelper.OSVersion < new Version(10, 0, 22523)) { return true; }

            return false;
        }

        /// <summary>
        /// Checks if the current <see cref="Windows"/> supports selected Acrylic.
        /// </summary>
        /// <returns><see langword="true"/> if Acrylic is supported.</returns>
        public static bool IsAcrylicSupported()
        {
            if (!OSVersionHelper.IsWindowsNT) { return false; }

            if (new Version(10, 0, 17063) <= OSVersionHelper.OSVersion && OSVersionHelper.OSVersion < new Version(10, 0, 22523)) { return true; }

            return false;
        }

        /// <summary>
        /// Applies selected background effect to <see cref="Window"/> when is rendered.
        /// </summary>
        /// <param name="window">Window to apply effect.</param>
        /// <param name="force">Skip the compatibility check.</param>
        public static bool Apply(Window window, bool force = false)
        {
            if (!force && (!IsSupported())) { return false; }

            var windowHandle = new WindowInteropHelper(window).EnsureHandle();

            if (windowHandle == IntPtr.Zero) { return false; }

            if (window.Background is SolidColorBrush brush)
            {
                Apply(windowHandle, brush.Color);
            }
            else
            {
                Apply(windowHandle, Colors.Transparent);
            }

            return true;
        }

        /// <summary>
        /// Applies selected background effect to <c>hWnd</c> by it's pointer.
        /// </summary>
        /// <param name="handle">Pointer to the window handle.</param>
        /// <param name="color">The Gradient Color of Acrylic.</param>
        /// <param name="force">Skip the compatibility check.</param>
        public static bool Apply(IntPtr handle, Color color, bool force = false)
        {
            if (!force && (!IsSupported())) { return false; }

            if (handle == IntPtr.Zero) { return false; }

            return IsAcrylicSupported() ? TryApplyAcrylic(handle, color) : TryApplyAero(handle);
        }

        /// <summary>
        /// Tries to remove background effects if they have been applied to the <see cref="Window"/>.
        /// </summary>
        /// <param name="window">The window from which the effect should be removed.</param>
        public static void Remove(Window window)
        {
            var windowHandle = new WindowInteropHelper(window).EnsureHandle();

            if (windowHandle == IntPtr.Zero) return;

            Remove(windowHandle);
        }

        /// <summary>
        /// Tries to remove all effects if they have been applied to the <c>hWnd</c>.
        /// </summary>
        /// <param name="handle">Pointer to the window handle.</param>
        public static void Remove(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;

            User32.ACCENT_POLICY accentPolicy = new User32.ACCENT_POLICY
            {
                AccentState = User32.ACCENT_STATE.ACCENT_DISABLED,
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
        }

        private static bool TryApplyAero(IntPtr handle)
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

        private static bool TryApplyAcrylic(IntPtr handle, Color backcolor)
        {
            User32.ACCENT_POLICY accentPolicy = new User32.ACCENT_POLICY
            {
                AccentState = User32.ACCENT_STATE.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                GradientColor = (uint)backcolor.ColorToDouble(0.8)
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

        private static int ColorToDouble(this Color value, double scale = 1)
        {
            return
            // Red
            value.R << 0 |
            // Green
            value.G << 8 |
            // Blue
            value.B << 16 |
            // Alpha
            (int)(value.A * scale) << 24;
        }
    }
}
