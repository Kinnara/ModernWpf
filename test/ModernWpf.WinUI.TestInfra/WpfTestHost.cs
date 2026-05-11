using System;
using System.Threading;
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
            action();
            return true;
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

    public static void Shutdown()
    {
        var currentDispatcher = dispatcher;
        if (currentDispatcher == null)
        {
            return;
        }

        currentDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
        thread?.Join(TimeSpan.FromSeconds(5));
        dispatcher = null;
        thread = null;
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
