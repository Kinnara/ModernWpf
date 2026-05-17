using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.FlyoutTests;

[TestClass]
public class FlyoutBaseApiTests
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
            var flyout = new Flyout
            {
                Content = new TextBlock { Text = "Flyout content" }
            };
            var events = new List<string>();
            bool cancelClosing = true;

            flyout.Opened += (_, _) => events.Add("Opened");
            flyout.Closing += (_, args) =>
            {
                events.Add($"Closing:{args.Cancel}");
                args.Cancel = cancelClosing;
                events.Add($"Cancel:{args.Cancel}");
            };
            flyout.Closed += (_, _) => events.Add("Closed");

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();

            flyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);

            flyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            AssertEvents(
                events,
                "Opened",
                "Closing:False",
                "Cancel:True");

            cancelClosing = false;
            flyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsFalse(flyout.IsOpen);
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
