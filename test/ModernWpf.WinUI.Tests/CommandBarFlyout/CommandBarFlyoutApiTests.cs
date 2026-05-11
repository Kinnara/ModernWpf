using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;
using CommandBarFlyout = ModernWpf.Controls.CommandBarFlyout;

namespace ModernWpf.WinUI.Tests.CommandBarFlyouts;

[TestClass]
public class CommandBarFlyoutApiTests
{
    [TestMethod]
    public void VerifyFlyoutDefaultPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = new CommandBarFlyout();

            Assert.IsNotNull(commandBarFlyout);
            Assert.IsNotNull(commandBarFlyout.PrimaryCommands);
            Assert.AreEqual(0, commandBarFlyout.PrimaryCommands.Count);
            Assert.IsNotNull(commandBarFlyout.SecondaryCommands);
            Assert.AreEqual(0, commandBarFlyout.SecondaryCommands.Count);
        });
    }
}
