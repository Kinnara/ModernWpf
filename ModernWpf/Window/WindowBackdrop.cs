using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace ModernWpf.Controls
{
    public static class WindowBackdrop
    {
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.RegisterAttached(
                "Kind",
                typeof(WindowBackdropKind),
                typeof(WindowBackdrop),
                new FrameworkPropertyMetadata(WindowBackdropKind.None, OnBackdropPropertyChanged),
                IsValidKind);

        public static readonly DependencyProperty FallbackBrushProperty =
            DependencyProperty.RegisterAttached(
                "FallbackBrush",
                typeof(Brush),
                typeof(WindowBackdrop),
                new FrameworkPropertyMetadata(null, OnBackdropPropertyChanged));

        private static readonly DependencyPropertyKey EffectiveKindPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "EffectiveKind",
                typeof(WindowBackdropKind),
                typeof(WindowBackdrop),
                new FrameworkPropertyMetadata(WindowBackdropKind.None));

        public static readonly DependencyProperty EffectiveKindProperty =
            EffectiveKindPropertyKey.DependencyProperty;

        private static readonly ConditionalWeakTable<Window, BackdropState> States =
            new ConditionalWeakTable<Window, BackdropState>();

        private static IWindowBackdropPlatform _platform = new DwmWindowBackdropPlatform();

        internal static IWindowBackdropPlatform Platform
        {
            get => _platform;
            set => _platform = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static WindowBackdropKind GetKind(Window window)
        {
            return (WindowBackdropKind)GetWindow(window).GetValue(KindProperty);
        }

        public static void SetKind(Window window, WindowBackdropKind value)
        {
            GetWindow(window).SetValue(KindProperty, value);
        }

        public static Brush GetFallbackBrush(Window window)
        {
            return (Brush)GetWindow(window).GetValue(FallbackBrushProperty);
        }

        public static void SetFallbackBrush(Window window, Brush value)
        {
            GetWindow(window).SetValue(FallbackBrushProperty, value);
        }

        public static WindowBackdropKind GetEffectiveKind(Window window)
        {
            return (WindowBackdropKind)GetWindow(window).GetValue(EffectiveKindProperty);
        }

        internal static void Refresh(Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            GetOrCreateState(window).Apply();
        }

        internal static void ResetPlatformForTests()
        {
            Platform = new DwmWindowBackdropPlatform();
        }

        private static Window GetWindow(Window window)
        {
            return window ?? throw new ArgumentNullException(nameof(window));
        }

        private static bool IsValidKind(object value)
        {
            var kind = (WindowBackdropKind)value;
            return kind == WindowBackdropKind.None ||
                kind == WindowBackdropKind.Mica ||
                kind == WindowBackdropKind.DesktopAcrylic;
        }

        private static void OnBackdropPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Window window))
            {
                throw new InvalidOperationException("WindowBackdrop properties can only be set on a Window.");
            }

            GetOrCreateState(window).Apply();
        }

        private static BackdropState GetOrCreateState(Window window)
        {
            return States.GetValue(window, value => new BackdropState(value));
        }

        internal interface IWindowBackdropPlatform
        {
            bool IsSystemBackdropSupported { get; }

            bool IsCompositionEnabled { get; }

            bool IsHighContrast { get; }

            IntPtr GetWindowHandle(Window window);

            bool TrySetBackdrop(IntPtr windowHandle, WindowBackdropKind kind);

            bool TryExtendFrame(IntPtr windowHandle, bool enabled);
        }

        private sealed class BackdropState
        {
            private const int WmSettingChange = 0x001A;
            private const int WmThemeChanged = 0x031A;
            private const int WmDwmCompositionChanged = 0x031E;

            private readonly Window _window;
            private Brush _restoreBackground;
            private HwndSource _source;
            private Color _restoreCompositionBackground;
            private Brush _lastAppliedBackground;
            private IntPtr _nativeWindowHandle;
            private bool _hasAppliedBackground;
            private bool _hasCompositionBackground;
            private bool _nativeBackdropActive;
            private bool _nativeBackdropAttempted;
            private bool _ownsExtendedFrame;
            private bool _isListeningForSystemParameters;
            private bool _isClosed;

            internal BackdropState(Window window)
            {
                _window = window;
                _restoreBackground = window.Background;
                _window.SourceInitialized += OnSourceInitialized;
                _window.Activated += OnWindowActivationChanged;
                _window.Deactivated += OnWindowActivationChanged;
                _window.Closed += OnWindowClosed;

                if (PresentationSource.FromVisual(window) is HwndSource source)
                {
                    AttachSource(source);
                }
            }

            internal void Apply()
            {
                if (_isClosed)
                {
                    return;
                }

                CaptureExternalBackgroundChange();
                var requestedKind = GetKind(_window);
                if (requestedKind == WindowBackdropKind.None)
                {
                    DisableNativeBackdrop();
                    SetEffectiveKind(WindowBackdropKind.None);
                    ApplyBackground(_restoreBackground);
                    return;
                }

                var platform = Platform;
                if (platform.IsSystemBackdropSupported &&
                    platform.IsCompositionEnabled &&
                    !platform.IsHighContrast &&
                    !_window.AllowsTransparency &&
                    TryApplyNativeBackdrop(requestedKind))
                {
                    SetEffectiveKind(requestedKind);
                    ApplyBackground(Brushes.Transparent);
                }
                else
                {
                    DisableNativeBackdrop();
                    SetEffectiveKind(WindowBackdropKind.None);
                    ApplyBackground(ResolveFallbackBrush());
                }
            }

            private void SetEffectiveKind(WindowBackdropKind value)
            {
                _window.SetValue(EffectiveKindPropertyKey, value);
            }

            private Brush ResolveFallbackBrush()
            {
                return GetFallbackBrush(_window) ??
                    _window.TryFindResource("WindowBackground") as Brush ??
                    SystemColors.WindowBrush;
            }

            private void CaptureExternalBackgroundChange()
            {
                if (_hasAppliedBackground &&
                    !ReferenceEquals(_window.Background, _lastAppliedBackground))
                {
                    _restoreBackground = _window.Background;
                    _hasAppliedBackground = false;
                    _lastAppliedBackground = null;
                }
            }

            private void ApplyBackground(Brush brush)
            {
                _window.SetCurrentValue(Control.BackgroundProperty, brush);
                _lastAppliedBackground = brush;
                _hasAppliedBackground = true;
            }

            private bool TryApplyNativeBackdrop(WindowBackdropKind kind)
            {
                var platform = Platform;
                var handle = platform.GetWindowHandle(_window);
                var windowChrome = WindowChrome.GetWindowChrome(_window);
                if (handle == IntPtr.Zero ||
                    (windowChrome != null &&
                        windowChrome.GlassFrameThickness != WindowChrome.GlassFrameCompleteThickness) ||
                    !PrepareCompositionSurface(handle))
                {
                    return false;
                }

                if (windowChrome == null && !_ownsExtendedFrame)
                {
                    if (!platform.TryExtendFrame(handle, true))
                    {
                        RestoreCompositionSurface();
                        return false;
                    }

                    _ownsExtendedFrame = true;
                }

                _nativeWindowHandle = handle;
                _nativeBackdropAttempted = true;
                if (platform.TrySetBackdrop(handle, kind))
                {
                    _nativeBackdropActive = true;
                    return true;
                }

                DisableNativeBackdrop();
                return false;
            }

            private bool PrepareCompositionSurface(IntPtr handle)
            {
                if (_source == null || _source.Handle != handle)
                {
                    AttachSource(HwndSource.FromHwnd(handle));
                }

                var compositionTarget = _source?.CompositionTarget;
                if (compositionTarget == null)
                {
                    return false;
                }

                if (!_hasCompositionBackground)
                {
                    _restoreCompositionBackground = compositionTarget.BackgroundColor;
                    _hasCompositionBackground = true;
                }

                compositionTarget.BackgroundColor = Colors.Transparent;
                return true;
            }

            private void DisableNativeBackdrop()
            {
                var platform = Platform;
                var handle = platform.GetWindowHandle(_window);
                if (handle == IntPtr.Zero)
                {
                    handle = _nativeWindowHandle;
                }

                if (handle != IntPtr.Zero &&
                    (_nativeBackdropActive || _nativeBackdropAttempted))
                {
                    platform.TrySetBackdrop(handle, WindowBackdropKind.None);
                }

                if (handle != IntPtr.Zero && _ownsExtendedFrame)
                {
                    platform.TryExtendFrame(handle, false);
                }

                _nativeBackdropActive = false;
                _nativeBackdropAttempted = false;
                _ownsExtendedFrame = false;
                _nativeWindowHandle = IntPtr.Zero;
                RestoreCompositionSurface();
            }

            private void RestoreCompositionSurface()
            {
                if (_hasCompositionBackground && _source?.CompositionTarget != null)
                {
                    _source.CompositionTarget.BackgroundColor = _restoreCompositionBackground;
                }

                _hasCompositionBackground = false;
            }

            private void OnSourceInitialized(object sender, EventArgs e)
            {
                if (PresentationSource.FromVisual(_window) is HwndSource source)
                {
                    AttachSource(source);
                }

                Apply();
            }

            private void AttachSource(HwndSource source)
            {
                if (source == null)
                {
                    return;
                }

                if (ReferenceEquals(_source, source))
                {
                    return;
                }

                _source?.RemoveHook(WndProc);
                _source = source;
                _source.AddHook(WndProc);
                if (!_isListeningForSystemParameters)
                {
                    SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;
                    _isListeningForSystemParameters = true;
                }
            }

            private IntPtr WndProc(
                IntPtr hwnd,
                int message,
                IntPtr wParam,
                IntPtr lParam,
                ref bool handled)
            {
                if (message == WmDwmCompositionChanged ||
                    message == WmSettingChange ||
                    message == WmThemeChanged)
                {
                    Apply();
                }

                return IntPtr.Zero;
            }

            private void OnWindowActivationChanged(object sender, EventArgs e)
            {
                Apply();
            }

            private void OnSystemParametersChanged(object sender, PropertyChangedEventArgs e)
            {
                if (string.Equals(e.PropertyName, nameof(SystemParameters.HighContrast), StringComparison.Ordinal) ||
                    string.Equals(e.PropertyName, nameof(SystemParameters.ClientAreaAnimation), StringComparison.Ordinal) ||
                    string.IsNullOrEmpty(e.PropertyName))
                {
                    Apply();
                }
            }

            private void OnWindowClosed(object sender, EventArgs e)
            {
                _isClosed = true;
                _source?.RemoveHook(WndProc);
                _source = null;
                _window.SourceInitialized -= OnSourceInitialized;
                _window.Activated -= OnWindowActivationChanged;
                _window.Deactivated -= OnWindowActivationChanged;
                _window.Closed -= OnWindowClosed;
                if (_isListeningForSystemParameters)
                {
                    SystemParameters.StaticPropertyChanged -= OnSystemParametersChanged;
                    _isListeningForSystemParameters = false;
                }
            }
        }

        private sealed class DwmWindowBackdropPlatform : IWindowBackdropPlatform
        {
            private const int DwmwaSystemBackdropType = 38;
            private const int DwmSystemBackdropAuto = 0;
            private const int DwmSystemBackdropNone = 1;
            private const int DwmSystemBackdropMainWindow = 2;
            private const int DwmSystemBackdropTransientWindow = 3;

            public bool IsSystemBackdropSupported => OSVersionHelper.IsWindows11Build22621OrGreater;

            public bool IsCompositionEnabled
            {
                get
                {
                    try
                    {
                        return DwmIsCompositionEnabled(out var enabled) >= 0 && enabled;
                    }
                    catch (DllNotFoundException)
                    {
                        return false;
                    }
                    catch (EntryPointNotFoundException)
                    {
                        return false;
                    }
                }
            }

            public bool IsHighContrast => SystemParameters.HighContrast;

            public IntPtr GetWindowHandle(Window window)
            {
                return new WindowInteropHelper(window).Handle;
            }

            public bool TrySetBackdrop(IntPtr windowHandle, WindowBackdropKind kind)
            {
                var nativeKind = kind switch
                {
                    WindowBackdropKind.None => DwmSystemBackdropNone,
                    WindowBackdropKind.Mica => DwmSystemBackdropMainWindow,
                    WindowBackdropKind.DesktopAcrylic => DwmSystemBackdropTransientWindow,
                    _ => DwmSystemBackdropAuto
                };

                try
                {
                    return DwmSetWindowAttribute(
                        windowHandle,
                        DwmwaSystemBackdropType,
                        ref nativeKind,
                        Marshal.SizeOf(typeof(int))) >= 0;
                }
                catch (DllNotFoundException)
                {
                    return false;
                }
                catch (EntryPointNotFoundException)
                {
                    return false;
                }
            }

            public bool TryExtendFrame(IntPtr windowHandle, bool enabled)
            {
                var margins = enabled
                    ? new Margins(-1)
                    : new Margins(0);

                try
                {
                    return DwmExtendFrameIntoClientArea(windowHandle, ref margins) >= 0;
                }
                catch (DllNotFoundException)
                {
                    return false;
                }
                catch (EntryPointNotFoundException)
                {
                    return false;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct Margins
            {
                internal Margins(int value)
                {
                    Left = value;
                    Right = value;
                    Top = value;
                    Bottom = value;
                }

                internal int Left;
                internal int Right;
                internal int Top;
                internal int Bottom;
            }

            [DllImport("dwmapi.dll", PreserveSig = true)]
            private static extern int DwmIsCompositionEnabled(
                [MarshalAs(UnmanagedType.Bool)] out bool enabled);

            [DllImport("dwmapi.dll", PreserveSig = true)]
            private static extern int DwmSetWindowAttribute(
                IntPtr hwnd,
                int attribute,
                ref int value,
                int valueSize);

            [DllImport("dwmapi.dll", PreserveSig = true)]
            private static extern int DwmExtendFrameIntoClientArea(
                IntPtr hwnd,
                ref Margins margins);
        }
    }
}
