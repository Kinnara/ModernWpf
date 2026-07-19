using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
    public void RootBoundsConstraintSurfaceMatchesWinUISource()
    {
        var flyout = new Flyout();

        Assert.IsTrue(flyout.ShouldConstrainToRootBounds);
        Assert.IsTrue(flyout.IsConstrainedToRootBounds);

        flyout.ShouldConstrainToRootBounds = false;

        Assert.IsFalse(flyout.ShouldConstrainToRootBounds);
        Assert.IsFalse(flyout.IsConstrainedToRootBounds);
    }

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
    public void HideDisconnectsPopupVisualSource()
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

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();

            flyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.IsNotNull(PresentationSource.FromVisual(flyout.InternalPopup.Child));

            flyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsFalse(flyout.IsOpen);
            Assert.IsNull(PresentationSource.FromVisual(flyout.InternalPopup.Child));
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
    public void ShowModeAutoNormalizesToStandardLikeWinUISource()
    {
        var flyout = new Flyout();

        flyout.ShowMode = FlyoutShowMode.Auto;

        Assert.AreEqual(FlyoutShowMode.Standard, flyout.ShowMode);
    }

    [TestMethod]
    public void FlyoutShowOptionsDefaultsMatchWinUISource()
    {
        var options = new FlyoutShowOptions();

        Assert.AreEqual(FlyoutShowMode.Auto, options.ShowMode);
        Assert.AreEqual(FlyoutPlacementMode.Auto, options.Placement);
        Assert.AreEqual(13, (int)FlyoutPlacementMode.Auto);
        Assert.IsNull(options.Position);
        Assert.IsNull(options.ExclusionRect);
    }

    [TestMethod]
    public void ShowAtWithOptionsAppliesTargetPointPlacementAndShowModeLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button { Content = "Target", Width = 120, Height = 36 };
            var flyout = new Flyout
            {
                Content = new TextBlock { Text = "Flyout content" },
                Placement = FlyoutPlacementMode.Bottom
            };
            var options = new FlyoutShowOptions
            {
                Position = new Point(24, 12),
                Placement = FlyoutPlacementMode.Right,
                ShowMode = FlyoutShowMode.Transient
            };

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();
            target.Focus();
            WpfTestHost.DoEvents();

            Assert.AreSame(target, Keyboard.FocusedElement);

            flyout.ShowAt(target, options);
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreSame(target, flyout.Target);
            Assert.AreSame(target, Keyboard.FocusedElement);
            Assert.AreEqual(FlyoutPlacementMode.Right, flyout.GetEffectivePlacement());
            Assert.AreEqual(FlyoutShowMode.Transient, flyout.ShowMode);
            Assert.AreEqual(new Rect(24, 12, 0, 0), flyout.InternalPopup.PlacementRectangle);

            flyout.Hide();
            WpfTestHost.DoEvents();

            Assert.AreEqual(FlyoutPlacementMode.Bottom, flyout.GetEffectivePlacement());
        });
    }

    [TestMethod]
    public void ShowAtWithOptionsSameTargetAppliesStateBeforeNoOpLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button { Content = "Target", Width = 120, Height = 36 };
            var flyout = new Flyout
            {
                Content = new TextBlock { Text = "Flyout content" },
                Placement = FlyoutPlacementMode.Bottom
            };
            int openedCount = 0;
            flyout.Opened += (_, _) => openedCount++;

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();

            flyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreEqual(1, openedCount);
            Assert.AreEqual(FlyoutPlacementMode.Bottom, flyout.GetEffectivePlacement());
            Assert.AreEqual(FlyoutShowMode.Standard, flyout.ShowMode);

            flyout.ShowAt(
                target,
                new FlyoutShowOptions
                {
                    Placement = FlyoutPlacementMode.Right,
                    ShowMode = FlyoutShowMode.Transient
                });
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreSame(target, flyout.Target);
            Assert.AreEqual(1, openedCount);
            Assert.AreEqual(FlyoutPlacementMode.Right, flyout.GetEffectivePlacement());
            Assert.AreEqual(FlyoutShowMode.Transient, flyout.ShowMode);

            flyout.Hide();
            WpfTestHost.DoEvents();

            Assert.AreEqual(FlyoutPlacementMode.Bottom, flyout.GetEffectivePlacement());
            Assert.AreEqual(FlyoutShowMode.Transient, flyout.ShowMode);
        });
    }

    [TestMethod]
    public void ShowAtWithOptionsAllowsNullTargetWhenPositionProvidedLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var root = new Grid { Width = 240, Height = 180 };
            var flyout = new Flyout
            {
                Content = new TextBlock { Text = "Flyout content" },
                Placement = FlyoutPlacementMode.Top
            };
            var options = new FlyoutShowOptions
            {
                Position = new Point(32, 24),
                Placement = FlyoutPlacementMode.Bottom,
                ShowMode = FlyoutShowMode.Transient
            };

            using var host = new TestWindowHost(root, width: 320, height: 220);
            host.UpdateLayout();

            flyout.ShowAt(null, options);
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreSame(root, flyout.Target);
            Assert.AreSame(root, flyout.InternalPopup.PlacementTarget);
            Assert.AreEqual(FlyoutPlacementMode.Bottom, flyout.GetEffectivePlacement());
            Assert.AreEqual(new Rect(32, 24, 0, 0), flyout.InternalPopup.PlacementRectangle);

            flyout.Hide();
            WpfTestHost.DoEvents();
        });
    }

    [TestMethod]
    public void ShowAtWithOptionsRequiresTargetOrPositionLikeWinUISource()
    {
        var flyout = new Flyout();

        Assert.ThrowsException<ArgumentException>(() => flyout.ShowAt(null, null));
        Assert.ThrowsException<ArgumentException>(() => flyout.ShowAt(null, new FlyoutShowOptions()));
    }

    [TestMethod]
    public void ShowAtWithOptionsExclusionRectShiftsTargetPointLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button { Content = "Target", Width = 120, Height = 36 };
            var flyout = new Flyout
            {
                Content = new TextBlock { Text = "Flyout content" },
                Placement = FlyoutPlacementMode.Top
            };
            var options = new FlyoutShowOptions
            {
                Position = new Point(50, 50),
                ExclusionRect = new Rect(40, 45, 20, 10),
                Placement = FlyoutPlacementMode.Bottom
            };

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();

            flyout.ShowAt(target, options);
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreEqual(new Rect(50, 50, 0, 0), flyout.InternalPopup.PlacementRectangle);

            var placements = flyout.PositionPopup(
                popupSize: new Size(30, 10),
                targetSize: new Size(),
                offset: new Point(),
                child: null);

            AssertPlacement(placements[0], -15, 5, PopupPrimaryAxis.Horizontal);

            flyout.Hide();
            WpfTestHost.DoEvents();
        });

        var exclusionRect = new Rect(-10, -5, 20, 10);

        AssertPlacement(
            CustomPopupPlacementHelper.PositionPopup(
                CustomPlacementMode.Top,
                popupSize: new Size(30, 10),
                targetSize: new Size(),
                offset: new Point(),
                exclusionRect: exclusionRect)[0],
            -15,
            -15,
            PopupPrimaryAxis.Horizontal);

        AssertPlacement(
            CustomPopupPlacementHelper.PositionPopup(
                CustomPlacementMode.Right,
                popupSize: new Size(30, 10),
                targetSize: new Size(),
                offset: new Point(),
                exclusionRect: exclusionRect)[0],
            10,
            -5,
            PopupPrimaryAxis.Vertical);

        AssertPlacement(
            CustomPopupPlacementHelper.PositionPopup(
                CustomPlacementMode.Left,
                popupSize: new Size(30, 10),
                targetSize: new Size(),
                offset: new Point(),
                exclusionRect: exclusionRect)[0],
            -40,
            -5,
            PopupPrimaryAxis.Vertical);
    }

    [TestMethod]
    public void StagedShowAtWithOptionsPreservesTargetPointLikeWinUISource()
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
                Content = new TextBlock { Text = "Second flyout" },
                Placement = FlyoutPlacementMode.Top
            };
            var options = new FlyoutShowOptions
            {
                Position = new Point(16, 8),
                Placement = FlyoutPlacementMode.Left,
                ShowMode = FlyoutShowMode.Transient
            };

            using var host = new TestWindowHost(root, width: 320, height: 220);
            host.UpdateLayout();

            firstFlyout.ShowAt(firstTarget);
            WpfTestHost.DoEvents();

            secondFlyout.ShowAt(secondTarget, options);
            WpfTestHost.DoEvents();

            Assert.IsFalse(firstFlyout.IsOpen);
            Assert.IsTrue(secondFlyout.IsOpen);
            Assert.AreSame(secondTarget, secondFlyout.Target);
            Assert.AreEqual(FlyoutPlacementMode.Left, secondFlyout.GetEffectivePlacement());
            Assert.AreEqual(new Rect(16, 8, 0, 0), secondFlyout.InternalPopup.PlacementRectangle);

            secondFlyout.Hide();
            WpfTestHost.DoEvents();
        });
    }

    [TestMethod]
    public void ShowModeTransientKeepsCurrentFocusLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button { Content = "Target", Width = 120, Height = 36 };
            var flyout = new Flyout
            {
                Content = new Button { Content = "Flyout content", Width = 140, Height = 40 },
                ShowMode = FlyoutShowMode.Transient
            };

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();
            target.Focus();
            WpfTestHost.DoEvents();

            Assert.AreSame(target, Keyboard.FocusedElement);

            flyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreSame(target, Keyboard.FocusedElement);

            flyout.Hide();
            WpfTestHost.DoEvents();
        });
    }

    [TestMethod]
    public void ShowModeTransientWithDismissOnPointerMoveAwayUsesWinUIThreshold()
    {
        Assert.AreEqual(3, (int)FlyoutShowMode.TransientWithDismissOnPointerMoveAway);

        var presenterBounds = new Rect(10, 20, 100, 50);

        Assert.IsFalse(FlyoutBase.IsPointerBeyondMoveAwayThreshold(presenterBounds, new Point(10, 20)));
        Assert.IsFalse(FlyoutBase.IsPointerBeyondMoveAwayThreshold(presenterBounds, new Point(110, 70)));
        Assert.IsFalse(FlyoutBase.IsPointerBeyondMoveAwayThreshold(presenterBounds, new Point(190, 45)));
        Assert.IsTrue(FlyoutBase.IsPointerBeyondMoveAwayThreshold(presenterBounds, new Point(191, 45)));
        Assert.IsFalse(FlyoutBase.IsPointerBeyondMoveAwayThreshold(presenterBounds, new Point(150, 110)));
        Assert.IsTrue(FlyoutBase.IsPointerBeyondMoveAwayThreshold(presenterBounds, new Point(168, 128)));
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
    public void SidePlacementNearMonitorBottomAlignsToAnchorBottomLikeCurrentWinUISource()
    {
        Assert.AreEqual(
            640,
            FlyoutBase.ClampSidePlacementVerticalOffset(
                verticalOffset: 900,
                anchorBottom: 940,
                presenterHeight: 300,
                availableTop: 0,
                availableBottom: 1080),
            0.1);
        Assert.AreEqual(
            900,
            FlyoutBase.ClampSidePlacementVerticalOffset(
                verticalOffset: 900,
                anchorBottom: 940,
                presenterHeight: 120,
                availableTop: 0,
                availableBottom: 1080),
            0.1);
        Assert.AreEqual(
            100,
            FlyoutBase.ClampSidePlacementVerticalOffset(
                verticalOffset: 100,
                anchorBottom: 140,
                presenterHeight: 1100,
                availableTop: 100,
                availableBottom: 1000),
            0.1);
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
