using MS.Win32;
using Standard;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using NativeMethods = Standard.NativeMethods;

namespace ModernWpf.Controls.Primitives
{
    internal class MaximizedWindowFixer
    {
        #region MaximizedWindowFixer

        public static readonly DependencyProperty MaximizedWindowFixerProperty =
            DependencyProperty.RegisterAttached(
                "MaximizedWindowFixer",
                typeof(MaximizedWindowFixer),
                typeof(MaximizedWindowFixer),
                new PropertyMetadata(OnMaximizedWindowFixerChanged));

        public static MaximizedWindowFixer GetMaximizedWindowFixer(Window window)
        {
            return (MaximizedWindowFixer)window.GetValue(MaximizedWindowFixerProperty);
        }

        public static void SetMaximizedWindowFixer(Window window, MaximizedWindowFixer value)
        {
            window.SetValue(MaximizedWindowFixerProperty, value);
        }

        private static void OnMaximizedWindowFixerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MaximizedWindowFixer oldValue)
            {
                oldValue.UnsetWindow();
            }

            if (e.NewValue is MaximizedWindowFixer newValue)
            {
                newValue.SetWindow((Window)d);
            }
        }

        #endregion

        private Thickness MaximizedWindowBorder => _maximizedWindowBorder ??= GetMaximizedWindowBorder();

        private bool IsWindowPosAdjusted
        {
            get => _isWindowPosAdjusted;
            set
            {
                if (_isWindowPosAdjusted != value)
                {
                    _isWindowPosAdjusted = value;
                    InvalidateMaximizedWindowBorder();
                }
            }
        }

        private void SetWindow(Window window)
        {
            UnsubscribeWindowEvents();

            _window = window;
            _hwnd = new WindowInteropHelper(window).Handle;

            _window.StateChanged += WindowStateChanged;
#if NET462_OR_NEWER
            _window.DpiChanged += WindowDpiChanged;
#endif
            _window.Closed += WindowClosed;

            if (_hwnd != IntPtr.Zero)
            {
                WindowSourceInitialized(null, null);
            }
            else
            {
                _window.SourceInitialized += WindowSourceInitialized;
            }
        }

        private void UnsetWindow()
        {
            UnsubscribeWindowEvents();
        }

        private void UnsubscribeWindowEvents()
        {
            if (_window != null)
            {
                _window.SourceInitialized -= WindowSourceInitialized;
                _window.StateChanged -= WindowStateChanged;
#if NET462_OR_NEWER
                _window.DpiChanged -= WindowDpiChanged;
#endif
                _window.Closed -= WindowClosed;
                _window.ClearValue(Control.PaddingProperty);
                _window = null;
            }

            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(new HwndSourceHook(WindowFilterMessage));
                _hwndSource = null;
            }

            _hwnd = IntPtr.Zero;
            _maximizedWindowBorder = null;
            _isWindowPosAdjusted = false;
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            _hwnd = new WindowInteropHelper(_window).Handle;
            _hwndSource = HwndSource.FromHwnd(_hwnd);
            _hwndSource.AddHook(new HwndSourceHook(WindowFilterMessage));

            UpdateWindowPadding();
        }

        private void WindowStateChanged(object sender, EventArgs e)
        {
            UpdateWindowPadding();
        }

#if NET462_OR_NEWER
        private void WindowDpiChanged(object sender, DpiChangedEventArgs e)
        {
            InvalidateMaximizedWindowBorder();
            UpdateWindowPadding();
        }
#endif

        private void WindowClosed(object sender, EventArgs e)
        {
            UnsetWindow();
        }

        private IntPtr WindowFilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            IntPtr retInt = IntPtr.Zero;
            WindowMessage message = (WindowMessage)msg;

            switch (message)
            {
                case WindowMessage.WM_SETTINGCHANGE:
                    InvalidateMaximizedWindowBorder();
                    UpdateWindowPadding();
                    break;
                case WindowMessage.WM_WINDOWPOSCHANGING:
                    OnWindowPosChanging(lParam);
                    break;
                case WindowMessage.WM_WINDOWPOSCHANGED:
                    if (!_maximizedWindowBorder.HasValue)
                    {
                        UpdateWindowPadding();
                    }
                    break;
            }

            return retInt;
        }

        private void OnWindowPosChanging(IntPtr lParam)
        {
            var pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
            if ((pos.flags & (int)SWP.NOSIZE) == 0)
            {
                bool windowPosAdjusted = false;

                WINDOWPLACEMENT placement = NativeMethods.GetWindowPlacement(pos.hwnd);
                if (placement.showCmd == SW.MAXIMIZE)
                {
                    if (GetTaskbarAutoHide(out ABEdge edge))
                    {
                        var rect = new MS.Win32.NativeMethods.RECT(pos.x, pos.y, pos.x + pos.cx, pos.y + pos.cy);
                        IntPtr monitor = SafeNativeMethods.MonitorFromRect(ref rect, MONITOR_DEFAULTTONEAREST);
                        if (monitor != IntPtr.Zero)
                        {
                            MONITORINFO info = NativeMethods.GetMonitorInfo(monitor);
                            bool primary = (info.dwFlags & MONITORINFOF_PRIMARY) != 0;
                            if (primary)
                            {
                                if (pos.x < 0 &&
                                    pos.y < 0 &&
                                    pos.cx > info.rcMonitor.Width &&
                                    pos.cy > info.rcMonitor.Height)
                                {
                                    AdjustWindowPosForTaskbarAutoHide(ref pos, edge);
                                    Marshal.StructureToPtr(pos, lParam, true);
                                    windowPosAdjusted = true;
                                }
                                else if (pos.x == 0 && pos.y == 0)
                                {
                                    windowPosAdjusted = true;
                                }
                            }
                        }
                    }
                }

                IsWindowPosAdjusted = windowPosAdjusted;
            }
        }

        private Thickness GetMaximizedWindowBorder()
        {
            if (IsWindowPosAdjusted)
            {
                return new Thickness();
            }

            double dpiScaleX, dpiScaleY;
#if NET462_OR_NEWER
            DpiScale dpi = VisualTreeHelper.GetDpi(_window);
            dpiScaleX = dpi.DpiScaleX;
            dpiScaleY = dpi.DpiScaleY;
#else
            Matrix transformToDevice = _hwndSource.CompositionTarget.TransformToDevice;
            dpiScaleX = transformToDevice.M11;
            dpiScaleY = transformToDevice.M22;
#endif

            int frameWidth = NativeMethods.GetSystemMetrics(SM.CXSIZEFRAME);
            int frameHeight = NativeMethods.GetSystemMetrics(SM.CYSIZEFRAME);
            int borderPadding = NativeMethods.GetSystemMetrics(SM.CXPADDEDBORDER);
            Size borderSize = new Size(frameWidth + borderPadding, frameHeight + borderPadding);
            Size borderSizeInDips = DpiHelper.DeviceSizeToLogical(borderSize, dpiScaleX, dpiScaleY);

            return new Thickness(borderSizeInDips.Width, borderSizeInDips.Height, borderSizeInDips.Width, borderSizeInDips.Height);
        }

        private void InvalidateMaximizedWindowBorder()
        {
            _maximizedWindowBorder = null;
        }

        private void UpdateWindowPadding()
        {
            if (_hwndSource == null || _hwndSource.IsDisposed || _hwndSource.CompositionTarget == null)
            {
                return;
            }

            if (_window.WindowState == WindowState.Maximized)
            {
                _window.Padding = MaximizedWindowBorder;
            }
            else
            {
                _window.ClearValue(Control.PaddingProperty);
            }
        }

        private static bool GetTaskbarAutoHide(out ABEdge edge)
        {
            IntPtr trayWnd = FindWindow("Shell_TrayWnd", null);
            if (trayWnd != IntPtr.Zero)
            {
                APPBARDATA abd = new APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = trayWnd;
                SHAppBarMessage(ABMsg.ABM_GETTASKBARPOS, ref abd);
                bool autoHide = Convert.ToBoolean(SHAppBarMessage(ABMsg.ABM_GETSTATE, ref abd));
                edge = autoHide ? GetEdge(abd.rc) : default;
                return autoHide;
            }

            edge = default;
            return false;

            static ABEdge GetEdge(RECT rc)
            {
                if (rc.Top == rc.Left && rc.Bottom > rc.Right)
                {
                    return ABEdge.ABE_LEFT;
                }
                else if (rc.Top == rc.Left && rc.Bottom < rc.Right)
                {
                    return ABEdge.ABE_TOP;
                }
                else if (rc.Top > rc.Left)
                {
                    return ABEdge.ABE_BOTTOM;
                }
                else
                {
                    return ABEdge.ABE_RIGHT;
                }
            }
        }

        private static void AdjustWindowPosForTaskbarAutoHide(ref WINDOWPOS pos, ABEdge edge)
        {
            pos.cx += pos.x * 2;
            pos.cy += pos.y * 2;
            pos.x = 0;
            pos.y = 0;

            switch (edge)
            {
                case ABEdge.ABE_LEFT:
                    pos.x = 2;
                    pos.cx -= 2;
                    break;
                case ABEdge.ABE_TOP:
                    pos.y = 2;
                    pos.cy -= 2;
                    break;
                case ABEdge.ABE_RIGHT:
                    pos.cx -= 2;
                    break;
                case ABEdge.ABE_BOTTOM:
                    pos.cy -= 2;
                    break;
            }
        }

        #region Win32 Interop

        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;
        private const int MONITORINFOF_PRIMARY = 0x00000001;

        private enum WindowMessage
        {
            WM_SETTINGCHANGE = 0x001A,
            WM_WINDOWPOSCHANGING = 0x0046,
            WM_WINDOWPOSCHANGED = 0x0047,
        }

        private enum ABEdge
        {
            ABE_LEFT = 0,
            ABE_TOP = 1,
            ABE_RIGHT = 2,
            ABE_BOTTOM = 3
        }

        private enum ABMsg
        {
            ABM_GETSTATE = 4,
            ABM_GETTASKBARPOS = 5,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public bool lParam;
        }

        [DllImport("shell32", CallingConvention = CallingConvention.StdCall)]
        private static extern uint SHAppBarMessage(ABMsg dwMessage, ref APPBARDATA pData);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        #endregion

        private Window _window;
        private IntPtr _hwnd;
        private HwndSource _hwndSource;
        private Thickness? _maximizedWindowBorder;
        private bool _isWindowPosAdjusted;
    }
}
