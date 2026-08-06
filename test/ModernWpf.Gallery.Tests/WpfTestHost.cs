using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ModernWpf.Gallery.Tests
{
    internal static class WpfTestHost
    {
        private static readonly IntPtr DpiAwarenessContextUnaware = new IntPtr(-1);
        private static readonly object Gate = new object();
        private static Thread _thread;
        private static Dispatcher _dispatcher;
        private static ManualResetEventSlim _ready;
        private static Exception _startupException;

        public static void EnsureStarted()
        {
            lock (Gate)
            {
                if (_dispatcher != null)
                {
                    return;
                }

                _ready = new ManualResetEventSlim();
                _thread = new Thread(RunDispatcher);
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.IsBackground = true;
                _thread.Name = "ModernWpf Gallery test dispatcher";
                _thread.Start();
            }

            if (!_ready.Wait(TimeSpan.FromSeconds(30)))
            {
                throw new TimeoutException("Timed out starting the WPF test dispatcher.");
            }

            if (_startupException != null)
            {
                ExceptionDispatchInfo.Capture(_startupException).Throw();
            }
        }

        public static void Run(Action action)
        {
            Run<object>(() =>
            {
                action();
                return null;
            });
        }

        public static T Run<T>(Func<T> action)
        {
            EnsureStarted();

            T result = default;
            Exception exception = null;
            _dispatcher.Invoke(() =>
            {
                try
                {
                    result = action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    CleanupUiState();
                }
            });

            if (exception != null)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }

            return result;
        }

        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new DispatcherOperationCallback(delegate
                {
                    frame.Continue = false;
                    return null;
                }),
                null);
            Dispatcher.PushFrame(frame);
        }

        public static void Shutdown()
        {
            var dispatcher = _dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.Invoke(() =>
            {
                CleanupUiState();
                Application.Current?.Shutdown();
            });
            dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);

            if (!_thread.Join(TimeSpan.FromSeconds(10)))
            {
                throw new TimeoutException("Timed out stopping the WPF test dispatcher.");
            }

            _dispatcher = null;
            _thread = null;
            _ready?.Dispose();
            _ready = null;
            _startupException = null;
        }

        private static void CleanupUiState()
        {
            if (!Dispatcher.CurrentDispatcher.CheckAccess())
            {
                throw new InvalidOperationException("WPF test cleanup must run on the host dispatcher.");
            }

            Keyboard.ClearFocus();
            Mouse.Capture(null);

            var app = Application.Current;
            if (app == null)
            {
                return;
            }

            foreach (var window in app.Windows.Cast<Window>().ToArray())
            {
                CloseTransientSurfaces(window);
                window.Content = null;
                window.Close();
            }

            app.MainWindow = null;
            DoEvents();
        }

        private static void CloseTransientSurfaces(DependencyObject root)
        {
            if (root is Popup popup)
            {
                popup.IsOpen = false;
            }

            if (root is FrameworkElement element)
            {
                if (element.ContextMenu is { IsOpen: true } contextMenu)
                {
                    contextMenu.IsOpen = false;
                }

                if (element.ToolTip is ToolTip { IsOpen: true } toolTip)
                {
                    toolTip.IsOpen = false;
                }
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var index = 0; index < childCount; index++)
            {
                CloseTransientSurfaces(VisualTreeHelper.GetChild(root, index));
            }
        }

        private static void RunDispatcher()
        {
            IntPtr previousDpiAwarenessContext = IntPtr.Zero;

            try
            {
                try
                {
                    // Rendering assertions compare exact WPF device-independent geometry and
                    // off-screen bitmaps. Keep those results independent of the runner's monitor
                    // and scaling configuration by initializing the test dispatcher at 96 DPI.
                    previousDpiAwarenessContext = SetThreadDpiAwarenessContext(DpiAwarenessContextUnaware);
                    if (previousDpiAwarenessContext == IntPtr.Zero)
                    {
                        throw new Win32Exception(
                            Marshal.GetLastWin32Error(),
                            "Could not configure the Gallery test dispatcher for deterministic 96-DPI rendering.");
                    }

                    var app = new App();
                    app.InitializeComponent();
                    app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                    _dispatcher = Dispatcher.CurrentDispatcher;
                }
                catch (Exception ex)
                {
                    _startupException = ex;
                }
                finally
                {
                    _ready.Set();
                }

                if (_startupException == null)
                {
                    Dispatcher.Run();
                }
            }
            finally
            {
                if (previousDpiAwarenessContext != IntPtr.Zero)
                {
                    SetThreadDpiAwarenessContext(previousDpiAwarenessContext);
                }
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr dpiContext);
    }
}
