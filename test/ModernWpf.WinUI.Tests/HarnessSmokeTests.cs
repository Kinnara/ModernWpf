using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests;

[TestClass]
public class HarnessSmokeTests
{
    [TestMethod]
    public void TestHostPageCanLoadAndLayout()
    {
        WpfTestHost.Run(() =>
        {
            using var host = new TestWindowHost(new TestHostPage());
            Assert.IsNotNull(VisualTreeTestHelper.FindDescendant<Grid>(host.Window));
        });
    }

    [TestMethod]
    public void ControlCanLoadWithModernWpfResources()
    {
        WpfTestHost.Run(() =>
        {
            var button = new Button
            {
                Content = "WinUI parity harness"
            };

            using var host = new TestWindowHost(button);
            Assert.AreSame(button, host.Window.Content);
        });
    }

    [TestMethod]
    public void DeferredIdleDrainProcessesExistingIdleWork()
    {
        WpfTestHost.Run(() =>
        {
            var contextIdleWorkRan = false;
            var applicationIdleWorkRan = false;

            Dispatcher.CurrentDispatcher.BeginInvoke(
                () => contextIdleWorkRan = true,
                DispatcherPriority.ContextIdle);
            Dispatcher.CurrentDispatcher.BeginInvoke(
                () => applicationIdleWorkRan = true,
                DispatcherPriority.ApplicationIdle);

            WpfTestHost.DrainDeferredIdleWork();

            Assert.IsTrue(contextIdleWorkRan, "Context-idle work was left queued on the shared test dispatcher.");
            Assert.IsTrue(applicationIdleWorkRan, "Application-idle work was left queued on the shared test dispatcher.");
        });
    }
}
