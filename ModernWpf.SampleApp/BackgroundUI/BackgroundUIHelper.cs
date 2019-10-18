using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace ModernWpf.SampleApp.Controls
{
    public static class BackgroundUIHelper
    {
        private static readonly Lazy<Dispatcher> _dispatcher = new Lazy<Dispatcher>(CreateDispatcher);

        public static Dispatcher Dispatcher => _dispatcher.Value;

        private static Dispatcher CreateDispatcher()
        {
            Dispatcher dispatcher = null;
            using (var sync = new AutoResetEvent(false))
            {
                var backgroundUI = new Thread(() =>
                {
                    dispatcher = Dispatcher.CurrentDispatcher;
                    sync.Set();
                    Dispatcher.Run();
                })
                {
                    Name = "BackgroundUIThread",
                    IsBackground = true
                };
                backgroundUI.SetApartmentState(ApartmentState.STA);
                backgroundUI.Start();
                sync.WaitOne();
            }
            Debug.Assert(dispatcher != null);
            return dispatcher;
        }
    }
}
