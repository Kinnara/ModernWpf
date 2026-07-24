using System.Windows.Controls;
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
}
