using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace MUXControlsTestApp
{
    [TestClass]
    public class TestEnvironment
    {
        private static readonly ManualResetEvent _appShutdown = new ManualResetEvent(false);

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            var appActivated = new ManualResetEvent(false);

            var thread = new Thread(() =>
            {
                Application.ResourceAssembly = Assembly.GetExecutingAssembly();

                App app = new App();
                app.InitializeComponent();
                app.Activated += onActivated;
                app.Run();
                _appShutdown.Set();

                void onActivated(object sender, EventArgs e)
                {
                    app.Activated -= onActivated;
                    appActivated.Set();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            appActivated.WaitOne();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            var app = Application.Current;
            if (app != null)
            {
                app.Dispatcher.Invoke(() =>
                {
                    app.Shutdown();
                });
                _appShutdown.WaitOne();
            }
        }
    }
}
