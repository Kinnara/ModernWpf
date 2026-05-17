using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.FlyoutTests;

[TestClass]
public class FlyoutBaseApiTests
{
    [TestMethod]
    public void TargetTracksOpenFlyoutLikeWinUISource()
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
            bool cancelClosing = true;

            flyout.Closing += (_, args) => args.Cancel = cancelClosing;

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();

            Assert.IsNull(flyout.Target);

            flyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreSame(target, flyout.Target);

            flyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreSame(target, flyout.Target);

            cancelClosing = false;
            flyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsFalse(flyout.IsOpen);
            Assert.IsNull(flyout.Target);
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

    [TestMethod]
    public void OpeningSecondFlyoutClosesFirstLikeWinUISource()
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
            var firstFlyout = new Flyout
            {
                Content = new TextBlock { Text = "First flyout" }
            };
            var secondFlyout = new Flyout
            {
                Content = new TextBlock { Text = "Second flyout" }
            };
            var events = new List<string>();

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
    public void PlacementTargetUnloadedHidesFlyoutLikeWinUISource()
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
            var flyout = new Flyout
            {
                Content = new TextBlock { Text = "Flyout content" }
            };

            using var host = new TestWindowHost(root, width: 320, height: 220);
            host.UpdateLayout();

            flyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreSame(target, flyout.Target);

            root.Children.Remove(target);
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsFalse(flyout.IsOpen);
            Assert.IsNull(flyout.Target);
        });
    }

    [TestMethod]
    public void PopupPlacementFallbackOrderMatchesWinUISourceMajorPlacementOrder()
    {
        WpfTestHost.Run(() =>
        {
            var placements = CustomPopupPlacementHelper.PositionPopup(
                CustomPlacementMode.RightEdgeAlignedTop,
                popupSize: new Size(20, 10),
                targetSize: new Size(100, 50),
                offset: new Point());

            Assert.AreEqual(4, placements.Length);
            AssertPlacement(placements[0], 100, 0, PopupPrimaryAxis.Vertical);
            AssertPlacement(placements[1], -20, 0, PopupPrimaryAxis.Vertical);
            AssertPlacement(placements[2], 0, -10, PopupPrimaryAxis.Horizontal);
            AssertPlacement(placements[3], 0, 50, PopupPrimaryAxis.Horizontal);
        });
    }

    [TestMethod]
    public void PopupPlacementFallbackKeepsFullPlacementAsSingleChoice()
    {
        WpfTestHost.Run(() =>
        {
            var placements = CustomPopupPlacementHelper.PositionPopup(
                CustomPlacementMode.Full,
                popupSize: new Size(20, 10),
                targetSize: new Size(100, 50),
                offset: new Point());

            Assert.AreEqual(1, placements.Length);
            AssertPlacement(placements[0], 40, 20, PopupPrimaryAxis.None);
        });
    }

    private static void AssertEvents(List<string> actual, params string[] expected)
    {
        Assert.AreEqual(string.Join("|", expected), string.Join("|", actual));
    }

    private static void AssertPlacement(CustomPopupPlacement placement, double x, double y, PopupPrimaryAxis primaryAxis)
    {
        Assert.AreEqual(x, placement.Point.X, 0.1);
        Assert.AreEqual(y, placement.Point.Y, 0.1);
        Assert.AreEqual(primaryAxis, placement.PrimaryAxis);
    }
}
