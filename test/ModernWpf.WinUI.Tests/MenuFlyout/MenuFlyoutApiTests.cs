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
    public void TargetTracksOpenMenuFlyoutLikeWinUISource()
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
            bool cancelClosing = true;

            menuFlyout.Closing += (_, args) => args.Cancel = cancelClosing;

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();

            Assert.IsNull(menuFlyout.Target);

            menuFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);
            Assert.AreSame(target, menuFlyout.Target);

            menuFlyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);
            Assert.AreSame(target, menuFlyout.Target);

            cancelClosing = false;
            menuFlyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsFalse(menuFlyout.IsOpen);
            Assert.IsNull(menuFlyout.Target);
        });
    }

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

    [TestMethod]
    public void OpeningSecondMenuFlyoutClosesFirstLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var firstTarget = new Button { Content = "First", Width = 120, Height = 36 };
            var secondTarget = new Button { Content = "Second", Width = 120, Height = 36 };
            var root = new StackPanel
            {
                Children =
                {
                    firstTarget,
                    secondTarget
                }
            };
            var firstFlyout = new MenuFlyout();
            var secondFlyout = new MenuFlyout();
            var events = new List<string>();

            firstFlyout.Items.Add(new MenuItem { Header = "Copy" });
            secondFlyout.Items.Add(new MenuItem { Header = "Paste" });
            firstFlyout.Opened += (_, _) => events.Add("FirstOpened");
            firstFlyout.Closed += (_, _) => events.Add("FirstClosed");
            secondFlyout.Opened += (_, _) => events.Add("SecondOpened");

            using var host = new TestWindowHost(root, width: 320, height: 220);
            host.UpdateLayout();

            firstFlyout.ShowAt(firstTarget);
            WpfTestHost.DoEvents();

            Assert.IsTrue(firstFlyout.IsOpen);
            Assert.AreSame(firstTarget, firstFlyout.Target);

            secondFlyout.ShowAt(secondTarget);
            WpfTestHost.DoEvents();

            Assert.IsFalse(firstFlyout.IsOpen);
            Assert.IsNull(firstFlyout.Target);
            Assert.IsTrue(secondFlyout.IsOpen);
            Assert.AreSame(secondTarget, secondFlyout.Target);
            AssertEvents(events, "FirstOpened", "FirstClosed", "SecondOpened");

            secondFlyout.Hide();
            WpfTestHost.DoEvents();
        });
    }

    [TestMethod]
    public void PlacementTargetUnloadedHidesMenuFlyoutLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button { Content = "Target", Width = 120, Height = 36 };
            var root = new StackPanel
            {
                Children =
                {
                    target
                }
            };
            var menuFlyout = new MenuFlyout();
            menuFlyout.Items.Add(new MenuItem { Header = "Copy" });

            using var host = new TestWindowHost(root, width: 320, height: 220);
            host.UpdateLayout();

            menuFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);
            Assert.AreSame(target, menuFlyout.Target);

            root.Children.Remove(target);
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsFalse(menuFlyout.IsOpen);
            Assert.IsNull(menuFlyout.Target);
        });
    }

    private static void AssertEvents(List<string> actual, params string[] expected)
    {
        Assert.AreEqual(string.Join("|", expected), string.Join("|", actual));
    }
}
