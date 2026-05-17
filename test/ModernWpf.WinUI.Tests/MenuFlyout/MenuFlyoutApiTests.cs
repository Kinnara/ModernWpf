using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.MenuFlyoutTests;

[TestClass]
public class MenuFlyoutApiTests
{
    [TestMethod]
    public void ClosingCanCancelHideLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button
            {
                Content = "Target",
                Width = 120,
                Height = 36
            };
            var menuFlyout = new MenuFlyout();
            menuFlyout.Items.Add(new MenuItem { Header = "Copy" });

            var events = new List<string>();
            bool cancelClosing = true;

            menuFlyout.Opened += (_, _) => events.Add("Opened");
            menuFlyout.Closing += (_, args) =>
            {
                events.Add($"Closing:{args.Cancel}");
                args.Cancel = cancelClosing;
                events.Add($"Cancel:{args.Cancel}");
            };
            menuFlyout.Closed += (_, _) => events.Add("Closed");

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();

            menuFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);

            menuFlyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);
            AssertEvents(
                events,
                "Opened",
                "Closing:False",
                "Cancel:True");

            cancelClosing = false;
            menuFlyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsFalse(menuFlyout.IsOpen);
            AssertEvents(
                events,
                "Opened",
                "Closing:False",
                "Cancel:True",
                "Closing:False",
                "Cancel:False",
                "Closed");
        });
    }

    private static void AssertEvents(List<string> actual, params string[] expected)
    {
        Assert.AreEqual(string.Join("|", expected), string.Join("|", actual));
    }
}
