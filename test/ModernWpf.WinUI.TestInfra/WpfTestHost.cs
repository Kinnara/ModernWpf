using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ModernWpf.WinUI.TestInfra;

public static class WpfTestHost
{
    private static readonly object Gate = new();
    private static Dispatcher? dispatcher;
    private static Thread? thread;

    public static Dispatcher Dispatcher
    {
        get
        {
            EnsureStarted();
            return dispatcher!;
        }
    }

    public static void Run(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        Invoke(() =>
        {
            try
            {
                action();
                return true;
            }
            finally
            {
                CleanupUiState();
            }
        });
    }

    public static T Invoke<T>(Func<T> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        EnsureStarted();

        if (Dispatcher.CheckAccess())
        {
            return action();
        }

        return Dispatcher.Invoke(action);
    }

    public static void DoEvents()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new DispatcherOperationCallback(_ =>
            {
                frame.Continue = false;
                return null;
            }),
            null);
        Dispatcher.PushFrame(frame);
    }

    public static void DrainDeferredIdleWork()
    {
        if (!Dispatcher.CurrentDispatcher.CheckAccess())
        {
            throw new InvalidOperationException("WPF deferred-work cleanup must run on the host dispatcher.");
        }

        var frame = new DispatcherFrame();
        var drainComplete = Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.ApplicationIdle,
            new DispatcherOperationCallback(_ =>
            {
                frame.Continue = false;
                return null;
            }),
            null);
        var timeout = new DispatcherTimer(
            DispatcherPriority.Send,
            Dispatcher.CurrentDispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        EventHandler stopFrame = (_, _) => frame.Continue = false;
        timeout.Tick += stopFrame;
        timeout.Start();

        try
        {
            Dispatcher.PushFrame(frame);
        }
        finally
        {
            timeout.Stop();
            timeout.Tick -= stopFrame;

            if (drainComplete.Status == DispatcherOperationStatus.Pending)
            {
                drainComplete.Abort();
            }
        }
    }

    public static void Shutdown()
    {
        var currentDispatcher = dispatcher;
        if (currentDispatcher == null)
        {
            return;
        }

        var currentThread = thread;
        currentDispatcher.Invoke(() =>
        {
            CleanupUiState();
            Application.Current?.Shutdown();
        });
        currentDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);

        if (currentThread != null && !currentThread.Join(TimeSpan.FromSeconds(10)))
        {
            throw new TimeoutException("Timed out stopping the WPF test dispatcher.");
        }

        dispatcher = null;
        thread = null;
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

        // Several product paths intentionally defer layout, popup placement,
        // and navigation work below Background priority. Give work already
        // queued by this test one bounded idle pass after its windows are gone
        // so it cannot run against a later test's UI. The timeout prevents
        // animations or recurring layout producers from starving cleanup.
        DrainDeferredIdleWork();
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

    private static void EnsureStarted()
    {
        if (dispatcher != null)
        {
            return;
        }

        lock (Gate)
        {
            if (dispatcher != null)
            {
                return;
            }

            using var ready = new ManualResetEventSlim();
            thread = new Thread(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                ready.Set();
                Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Name = "ModernWpf WinUI Test Host";
            thread.Start();
            ready.Wait();
        }
    }
}
