using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace ModernWpf.Gallery.Tests
{
    internal static class WpfTestHost
    {
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
            });

            if (exception != null)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }

            return result;
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
                Application.Current?.Shutdown();
                dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            });

            _thread.Join(TimeSpan.FromSeconds(10));
            _dispatcher = null;
            _thread = null;
            _ready?.Dispose();
            _ready = null;
            _startupException = null;
        }

        private static void RunDispatcher()
        {
            try
            {
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
    }
}
