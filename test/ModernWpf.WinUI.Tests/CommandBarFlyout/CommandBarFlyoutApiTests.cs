using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using CommandBarFlyout = ModernWpf.Controls.CommandBarFlyout;

namespace ModernWpf.WinUI.Tests.CommandBarFlyouts;

[TestClass]
public class CommandBarFlyoutApiTests
{
    private const double PrimaryCommandActualWidth = 60.0;
    private const double PrimaryCommandHeight = 52.0;
    private const double PrimaryCommandContentMinWidth = 40.0;
    private const double PrimaryLabelPanelWidth = 60.0;
    private const double CommandBarPrimarySurfaceHeight = 60.0;

    private enum CommandBarSizingOptions
    {
        PrimaryItemsLarger,
        SecondaryItemsLarger,
        SecondaryItemsMaxWidth,
        SecondaryItemsMaxHeight
    }

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
            Assert.IsFalse(commandBarFlyout.ShouldConstrainToRootBounds);
            Assert.IsFalse(commandBarFlyout.IsConstrainedToRootBounds);
            Assert.IsFalse(commandBarFlyout.AreOpenCloseAnimationsEnabled);
        });
    }

    [TestMethod]
    public void AlwaysExpandedOpeningUsesStandardShowMode()
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = new CommandBarFlyout
            {
                AlwaysExpanded = true,
                ShowMode = FlyoutShowMode.Transient
            };

            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Label = "Copy" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Select all" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 420, height: 260);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.AreEqual(FlyoutShowMode.Standard, commandBarFlyout.ShowMode);

            var commandBar = GetCommandBar(commandBarFlyout);
            Assert.IsTrue(commandBar.IsOpen);
            Assert.AreEqual(CommandBarOverflowButtonVisibility.Collapsed, commandBar.OverflowButtonVisibility);

            commandBar.SetCurrentValue(CommandBarFlyoutCommandBar.IsOpenProperty, false);
            WpfTestHost.DoEvents();
            Assert.IsTrue(commandBar.IsOpen, "AlwaysExpanded must reject an overflow-collapse request while the flyout is open.");

            HideAndWait(commandBarFlyout);
        });
    }

    [TestMethod]
    public void VerifyFlyoutCommandsArePropagatedToTheCommandBar()
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right
            };

            var cutButton = new AppBarButton { Label = "Cut" };
            var copyButton = new AppBarButton { Label = "Copy" };
            var pasteButton = new AppBarButton { Label = "Paste" };
            var undoButton = new AppBarButton { Label = "Undo" };
            var redoButton = new AppBarButton { Label = "Redo" };

            commandBarFlyout.PrimaryCommands.Add(cutButton);
            commandBarFlyout.PrimaryCommands.Add(copyButton);
            commandBarFlyout.PrimaryCommands.Add(pasteButton);
            commandBarFlyout.SecondaryCommands.Add(undoButton);
            commandBarFlyout.SecondaryCommands.Add(redoButton);

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 420, height: 260);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(commandBarFlyout.IsOpen);

            var commandBar = GetCommandBar(commandBarFlyout);
            VerifyCommandCollections(commandBarFlyout, commandBar);

            var selectAllButton = new AppBarButton { Label = "Select All" };
            commandBarFlyout.SecondaryCommands.Add(selectAllButton);
            WpfTestHost.DoEvents();
            VerifyCommandCollections(commandBarFlyout, commandBar);

            var boldButton = new AppBarButton { Label = "Bold" };
            commandBarFlyout.PrimaryCommands[1] = boldButton;
            WpfTestHost.DoEvents();
            VerifyCommandCollections(commandBarFlyout, commandBar);

            commandBarFlyout.PrimaryCommands.Remove(cutButton);
            commandBarFlyout.SecondaryCommands.Remove(undoButton);
            WpfTestHost.DoEvents();
            VerifyCommandCollections(commandBarFlyout, commandBar);

            HideAndWait(commandBarFlyout);
            Assert.IsFalse(commandBarFlyout.IsOpen);
        });
    }

    [TestMethod]
    public void PrimaryCommandsUseBottomLabelsLikeWinUIReference()
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right
            };

            var shareButton = new AppBarButton { Icon = new SymbolIcon(Symbol.Share), Label = "Share" };
            var saveButton = new AppBarButton { Icon = new SymbolIcon(Symbol.Save), Label = "Save" };
            var deleteButton = new AppBarButton { Icon = new SymbolIcon(Symbol.Delete), Label = "Delete" };

            commandBarFlyout.PrimaryCommands.Add(shareButton);
            commandBarFlyout.PrimaryCommands.Add(saveButton);
            commandBarFlyout.PrimaryCommands.Add(deleteButton);
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Resize" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 520, height: 260);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            var commandBar = GetCommandBar(commandBarFlyout);
            Assert.AreEqual(CommandBarDefaultLabelPosition.Bottom, commandBar.DefaultLabelPosition);
            VerifyPrimaryCommandBottomLabel(shareButton, "Share");
            VerifyPrimaryCommandBottomLabel(saveButton, "Save");
            VerifyPrimaryCommandBottomLabel(deleteButton, "Delete");

            HideAndWait(commandBarFlyout);
        });
    }

    [TestMethod]
    public void WidePrimaryCommandStripMovesExcessCommandsIntoOverflowLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right
            };

            for (var i = 0; i < 20; i++)
            {
                commandBarFlyout.PrimaryCommands.Add(new AppBarButton());
            }

            for (var i = 21; i <= 25; i++)
            {
                commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = $"Item {i}" });
            }

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 620, height: 420);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            var commandBar = GetCommandBar(commandBarFlyout);
            commandBar.ApplyTemplate();
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            var primaryPanel = FindTemplateChild<System.Windows.Controls.Panel>(commandBar, "PrimaryItemsPanel");
            var secondaryPanel = FindTemplateChild<System.Windows.Controls.Panel>(commandBar, "SecondaryItemsPanel");

            Assert.AreEqual(9, primaryPanel.Children.Count);
            Assert.AreEqual(17, secondaryPanel.Children.Count, "Expected 11 moved primary commands, an automatic separator, and five secondary commands.");
            Assert.IsInstanceOfType(secondaryPanel.Children[11], typeof(AppBarSeparator));

            for (var i = 0; i < 11; i++)
            {
                Assert.IsTrue((bool)secondaryPanel.Children[i].GetValue(AppBarElementProperties.IsInOverflowProperty));
            }

            HideAndWait(commandBarFlyout);
        });
    }

    [TestMethod]
    public void DynamicallyInsertedCommandsKeepCurrentFlyoutMenuItemAutomationRoles()
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = new CommandBarFlyout();
            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Label = "Copy" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Select all" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 420, height: 260);
            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            var insertedPrimary = new AppBarButton { Label = "Paste" };
            var insertedSecondary = new AppBarToggleButton { Label = "Bold" };
            commandBarFlyout.PrimaryCommands.Add(insertedPrimary);
            commandBarFlyout.SecondaryCommands.Add(insertedSecondary);
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            var primaryPeer = UIElementAutomationPeer.CreatePeerForElement(insertedPrimary)
                ?? new ModernWpf.Automation.Peers.AppBarButtonAutomationPeer(insertedPrimary);
            var secondaryPeer = UIElementAutomationPeer.CreatePeerForElement(insertedSecondary)
                ?? new ModernWpf.Automation.Peers.AppBarToggleButtonAutomationPeer(insertedSecondary);

            Assert.AreEqual(AutomationControlType.MenuItem, primaryPeer.GetAutomationControlType());
            Assert.AreEqual("menu item", primaryPeer.GetLocalizedControlType());
            Assert.AreEqual(AutomationControlType.MenuItem, secondaryPeer.GetAutomationControlType());
            Assert.AreEqual("menu item", secondaryPeer.GetLocalizedControlType());

            HideAndWait(commandBarFlyout);
        });
    }

    [TestMethod]
    public void CommandsExposeWinUIFlyoutAutomationRolesAndMoreButtonName()
    {
        WpfTestHost.Run(() =>
        {
            var primaryButton = new AppBarButton { Label = "Share" };
            var secondaryButton = new AppBarButton { Label = "Resize" };
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right
            };

            commandBarFlyout.PrimaryCommands.Add(primaryButton);
            commandBarFlyout.SecondaryCommands.Add(secondaryButton);

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 520, height: 300);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            try
            {
                var commandBar = GetCommandBar(commandBarFlyout);
                commandBar.ApplyTemplate();
                host.UpdateLayout();

                var commandBarPeer = UIElementAutomationPeer.CreatePeerForElement(commandBar);
                var primaryPeer = UIElementAutomationPeer.CreatePeerForElement(primaryButton);
                var secondaryPeer = UIElementAutomationPeer.CreatePeerForElement(secondaryButton);
                var moreButton = FindTemplateChild<ButtonBase>(commandBar, "MoreButton");

                Assert.IsNotNull(commandBarPeer);
                Assert.IsNotNull(primaryPeer);
                Assert.IsNotNull(secondaryPeer);
                Assert.AreEqual(AutomationControlType.Menu, commandBarPeer.GetAutomationControlType());
                Assert.AreEqual("menu", commandBarPeer.GetLocalizedControlType());
                Assert.AreEqual(AutomationControlType.MenuItem, primaryPeer.GetAutomationControlType());
                Assert.AreEqual("menu item", primaryPeer.GetLocalizedControlType());
                Assert.AreEqual(AutomationControlType.MenuItem, secondaryPeer.GetAutomationControlType());
                Assert.AreEqual("menu item", secondaryPeer.GetLocalizedControlType());
                commandBar.IsOpen = false;
                host.UpdateLayout();
                WpfTestHost.DoEvents();
                Assert.AreEqual("More app bar", AutomationProperties.GetName(moreButton));

                commandBar.IsOpen = true;
                host.UpdateLayout();
                WpfTestHost.DoEvents();
                Assert.AreEqual("Less app bar", AutomationProperties.GetName(moreButton));
            }
            finally
            {
                HideAndWait(commandBarFlyout);
            }
        });
    }

    [TestMethod]
    public void VerifyCommandBarSizingPrimaryItemsLarger()
    {
        VerifyCommandBarSizing(CommandBarSizingOptions.PrimaryItemsLarger);
    }

    [TestMethod]
    public void VerifyCommandBarSizingSecondaryItemsLarger()
    {
        VerifyCommandBarSizing(CommandBarSizingOptions.SecondaryItemsLarger);
    }

    [TestMethod]
    public void VerifyCommandBarSizingSecondaryItemsMaxWidth()
    {
        VerifyCommandBarSizing(CommandBarSizingOptions.SecondaryItemsMaxWidth);
    }

    [TestMethod]
    public void VerifyCommandBarSizingSecondaryItemsMaxHeight()
    {
        VerifyCommandBarSizing(CommandBarSizingOptions.SecondaryItemsMaxHeight);
    }

    [TestMethod]
    public void GalleryPrimaryStripIsNotClippedWhileFlyoutShadowIsVisible()
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right,
                ShowMode = FlyoutShowMode.Transient
            };
            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Share), Label = "Share" });
            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Save), Label = "Save" });
            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Delete), Label = "Delete" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Resize" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Move" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 180
            };

            using var host = new TestWindowHost(target, width: 720, height: 520);
            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            try
            {
                var commandBar = GetCommandBar(commandBarFlyout);
                commandBar.ApplyTemplate();
                host.UpdateLayout();
                WpfTestHost.DoEvents();
                host.UpdateLayout();

                var primaryItemsRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "PrimaryItemsRoot");
                var moreButton = FindTemplateChild<ToggleButton>(commandBar, "MoreButton");
                var moreButtonRight = moreButton.TranslatePoint(new Point(moreButton.ActualWidth, 0), primaryItemsRoot).X;
                var presenter = commandBarFlyout.GetPresenter();
                var presenterLayoutRoot = FindTemplateChild<BorderEx>(presenter, "LayoutRoot");
                var commandBarRightInPresenter = commandBar.TranslatePoint(new Point(commandBar.ActualWidth, 0), presenterLayoutRoot).X;
                var popupSource = PresentationSource.FromVisual(presenter) as HwndSource
                    ?? throw new AssertFailedException("Expected the CommandBarFlyout presenter to be hosted in a popup HWND.");
                var popupWindowBounds = GetNativeWindowRect(popupSource.Handle);
                var presenterDeviceSize = popupSource.CompositionTarget.TransformToDevice.Transform(
                    new Vector(presenter.ActualWidth, presenter.ActualHeight));

                Assert.IsTrue(
                    moreButtonRight <= primaryItemsRoot.ActualWidth + 0.5 &&
                    commandBarRightInPresenter <= presenterLayoutRoot.ActualWidth + 0.5 &&
                    popupWindowBounds.Width + 1 >= presenterDeviceSize.X &&
                    popupWindowBounds.Height + 1 >= presenterDeviceSize.Y,
                    $"The Gallery CommandBarFlyout primary surface clipped the More button. " +
                    $"PrimaryActualWidth={primaryItemsRoot.ActualWidth}, PrimaryDesiredWidth={primaryItemsRoot.DesiredSize.Width}, " +
                    $"MoreLeft={moreButton.TranslatePoint(new Point(), primaryItemsRoot).X}, MoreWidth={moreButton.ActualWidth}, " +
                    $"MoreRight={moreButtonRight}, CurrentWidth={commandBar.FlyoutTemplateSettings.CurrentWidth}, " +
                    $"ExpandedWidth={commandBar.FlyoutTemplateSettings.ExpandedWidth}, CommandBarActualWidth={commandBar.ActualWidth}, " +
                    $"CommandBarRightInPresenter={commandBarRightInPresenter}, PresenterLayoutWidth={presenterLayoutRoot.ActualWidth}, " +
                    $"PresenterActualSize={presenter.ActualWidth}x{presenter.ActualHeight}, " +
                    $"PresenterDesiredSize={presenter.DesiredSize.Width}x{presenter.DesiredSize.Height}, " +
                    $"PopupHwndSize={popupWindowBounds.Width}x{popupWindowBounds.Height}, " +
                    $"PresenterDeviceSize={presenterDeviceSize.X}x{presenterDeviceSize.Y}.");
            }
            finally
            {
                HideAndWait(commandBarFlyout);
            }
        });
    }

    [TestMethod]
    public void GalleryPrimaryCommandReceivesRealMousePointerStates()
    {
        WpfTestHost.Run(() =>
        {
            var restoreCursor = NativeGetCursorPos(out var originalCursor);
            var shareButton = new AppBarButton { Icon = new SymbolIcon(Symbol.Share), Label = "Share" };
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.RightEdgeAlignedTop,
                ShowMode = FlyoutShowMode.Transient
            };
            commandBarFlyout.PrimaryCommands.Add(shareButton);
            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Save), Label = "Save" });
            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Delete), Label = "Delete" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Resize" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 180
            };

            using var host = new TestWindowHost(target, width: 720, height: 520);
            host.Window.Left = 120;
            host.Window.Top = 120;
            host.UpdateLayout();
            host.Window.Activate();
            NativeSetForegroundWindow(new WindowInteropHelper(host.Window).Handle);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            var mouseIsDown = false;
            try
            {
                host.UpdateLayout();
                MoveNativePointerTo(shareButton);
                WaitFor(
                    () => shareButton.IsMouseOver,
                    $"The primary CommandBarFlyout command did not enter IsMouseOver. DirectlyOver={Mouse.DirectlyOver?.GetType().FullName ?? "<null>"}.");

                var root = FindTemplateChild<System.Windows.Controls.Grid>(shareButton, "Root");
                AssertCurrentState(root, "CommonStates", "PointerOver");

                NativeMouseEvent(NativeMouseLeftDown, 0, 0, 0, UIntPtr.Zero);
                mouseIsDown = true;
                WaitFor(
                    () => shareButton.IsPressed,
                    $"The primary CommandBarFlyout command did not enter IsPressed. DirectlyOver={Mouse.DirectlyOver?.GetType().FullName ?? "<null>"}.");
                AssertCurrentState(root, "CommonStates", "Pressed");
            }
            finally
            {
                if (mouseIsDown)
                {
                    NativeMouseEvent(NativeMouseLeftUp, 0, 0, 0, UIntPtr.Zero);
                }

                HideAndWait(commandBarFlyout);
                if (restoreCursor)
                {
                    NativeSetCursorPos(originalCursor.X, originalCursor.Y);
                }
            }
        });
    }

    [TestMethod]
    public void GallerySecondaryCommandReceivesRealMouseStatesBeforeInvocationDismissesFlyout()
    {
        WpfTestHost.Run(() =>
        {
            var restoreCursor = NativeGetCursorPos(out var originalCursor);
            var resizeButton = new AppBarButton { Label = "Resize" };
            var resizeInvocations = 0;
            resizeButton.Click += delegate { resizeInvocations++; };

            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.RightEdgeAlignedTop,
                ShowMode = FlyoutShowMode.Transient
            };
            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Share), Label = "Share" });
            commandBarFlyout.SecondaryCommands.Add(resizeButton);

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 180
            };

            using var host = new TestWindowHost(target, width: 720, height: 520);
            host.Window.Left = 120;
            host.Window.Top = 120;
            host.UpdateLayout();
            host.Window.Activate();
            NativeSetForegroundWindow(new WindowInteropHelper(host.Window).Handle);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            var mouseIsDown = false;
            try
            {
                var commandBar = GetCommandBar(commandBarFlyout);
                WaitForExpandedOverflow(commandBar, host);
                MoveNativePointerTo(resizeButton);
                WaitFor(
                    () => resizeButton.IsMouseOver,
                    $"The secondary CommandBarFlyout command did not enter IsMouseOver. DirectlyOver={Mouse.DirectlyOver?.GetType().FullName ?? "<null>"}.");

                var root = FindTemplateChild<System.Windows.Controls.Grid>(resizeButton, "Root");
                AssertCurrentState(root, "CommonStates", "PointerOver");

                MoveNativePointerTo(target);
                WaitFor(
                    () => !resizeButton.IsMouseOver,
                    $"The secondary CommandBarFlyout command retained IsMouseOver after the pointer crossed to the owner window. DirectlyOver={Mouse.DirectlyOver?.GetType().FullName ?? "<null>"}.");
                MoveNativePointerTo(resizeButton);
                WaitFor(() => resizeButton.IsMouseOver, "The secondary command did not re-enter pointer-over after crossing the native surface boundary.");

                NativeMouseEvent(NativeMouseLeftDown, 0, 0, 0, UIntPtr.Zero);
                mouseIsDown = true;
                WaitFor(
                    () => resizeButton.IsPressed,
                    $"The secondary CommandBarFlyout command did not enter IsPressed. DirectlyOver={Mouse.DirectlyOver?.GetType().FullName ?? "<null>"}.");
                Assert.IsTrue(commandBarFlyout.IsOpen, "The outer flyout must remain open while a secondary command is held pressed.");
                Assert.IsTrue(commandBar.IsOpen, "The overflow surface must remain open while a secondary command is held pressed.");
                AssertCurrentState(root, "CommonStates", "Pressed");

                NativeMouseEvent(NativeMouseLeftUp, 0, 0, 0, UIntPtr.Zero);
                mouseIsDown = false;
                WaitFor(() => resizeInvocations == 1, "The held secondary command was not invoked on mouse release.");
                WaitFor(() => !commandBarFlyout.IsOpen, "The secondary command did not dismiss the CommandBarFlyout after invocation.");
            }
            finally
            {
                if (mouseIsDown)
                {
                    NativeMouseEvent(NativeMouseLeftUp, 0, 0, 0, UIntPtr.Zero);
                }

                HideAndWait(commandBarFlyout);
                if (restoreCursor)
                {
                    NativeSetCursorPos(originalCursor.X, originalCursor.Y);
                }
            }
        });
    }

    [TestMethod]
    public void GalleryExpandedFlyoutLightDismissesFromOwnerWindowInputOutsideBothSurfaces()
    {
        WpfTestHost.Run(() =>
        {
            var restoreCursor = NativeGetCursorPos(out var originalCursor);
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.RightEdgeAlignedTop,
                ShowMode = FlyoutShowMode.Transient
            };
            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Share), Label = "Share" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Resize" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 180
            };

            using var host = new TestWindowHost(target, width: 720, height: 520);
            host.Window.Left = 120;
            host.Window.Top = 120;
            host.UpdateLayout();
            host.Window.Activate();
            NativeSetForegroundWindow(new WindowInteropHelper(host.Window).Handle);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            var mouseIsDown = false;
            try
            {
                var commandBar = GetCommandBar(commandBarFlyout);
                WaitForExpandedOverflow(commandBar, host);
                Assert.IsTrue(commandBarFlyout.InternalPopup.StaysOpen, "CommandBarFlyout should own light-dismiss across its two native surfaces.");

                var outsidePoint = target.PointToScreen(new Point(10, target.ActualHeight - 10));
                NativeSetCursorPos((int)Math.Round(outsidePoint.X), (int)Math.Round(outsidePoint.Y));
                NativeMouseEvent(NativeMouseMove, 1, 0, 0, UIntPtr.Zero);
                NativeMouseEvent(NativeMouseMove, unchecked((uint)-1), 0, 0, UIntPtr.Zero);
                NativeMouseEvent(NativeMouseLeftDown, 0, 0, 0, UIntPtr.Zero);
                mouseIsDown = true;
                WaitFor(() => !commandBarFlyout.IsOpen, "Owner-window input outside both flyout surfaces did not light-dismiss the CommandBarFlyout.");
                NativeMouseEvent(NativeMouseLeftUp, 0, 0, 0, UIntPtr.Zero);
                mouseIsDown = false;
            }
            finally
            {
                if (mouseIsDown)
                {
                    NativeMouseEvent(NativeMouseLeftUp, 0, 0, 0, UIntPtr.Zero);
                }

                HideAndWait(commandBarFlyout);
                if (restoreCursor)
                {
                    NativeSetCursorPos(originalCursor.X, originalCursor.Y);
                }
            }
        });
    }

    [TestMethod]
    public void GalleryMoreButtonClearsPointerStateAfterCollapsingOverflow()
    {
        WpfTestHost.Run(() =>
        {
            var restoreCursor = NativeGetCursorPos(out var originalCursor);
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.RightEdgeAlignedTop,
                ShowMode = FlyoutShowMode.Transient
            };
            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Share), Label = "Share" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Resize" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 180
            };

            using var host = new TestWindowHost(target, width: 720, height: 520);
            host.Window.Left = 120;
            host.Window.Top = 120;
            host.UpdateLayout();
            host.Window.Activate();
            NativeSetForegroundWindow(new WindowInteropHelper(host.Window).Handle);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            var mouseIsDown = false;
            try
            {
                var commandBar = GetCommandBar(commandBarFlyout);
                WaitForExpandedOverflow(commandBar, host);
                var moreButton = FindTemplateChild<ToggleButton>(commandBar, "MoreButton");
                MoveNativePointerTo(moreButton);
                WaitFor(() => moreButton.IsMouseOver, "The expanded More button did not enter its pointer-over state.");

                NativeMouseEvent(NativeMouseLeftDown, 0, 0, 0, UIntPtr.Zero);
                mouseIsDown = true;
                NativeMouseEvent(NativeMouseLeftUp, 0, 0, 0, UIntPtr.Zero);
                mouseIsDown = false;
                WaitFor(() => !commandBar.IsOpen, "The More-button click did not collapse the overflow surface.");

                MoveNativePointerTo(target);
                WaitFor(
                    () => !moreButton.IsMouseOver && !moreButton.IsPressed,
                    $"The collapsed More button retained pointer visuals. IsMouseOver={moreButton.IsMouseOver}, IsPressed={moreButton.IsPressed}.");
            }
            finally
            {
                if (mouseIsDown)
                {
                    NativeMouseEvent(NativeMouseLeftUp, 0, 0, 0, UIntPtr.Zero);
                }

                HideAndWait(commandBarFlyout);
                if (restoreCursor)
                {
                    NativeSetCursorPos(originalCursor.X, originalCursor.Y);
                }
            }
        });
    }

    [TestMethod]
    public void SecondaryCommandPropertyChangeRefreshesOpenSizing()
    {
        WpfTestHost.Run(() =>
        {
            var secondaryCommand = new AppBarButton { Label = "Short" };
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right,
                ShowMode = FlyoutShowMode.Transient
            };

            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Label = "Copy" });
            commandBarFlyout.SecondaryCommands.Add(secondaryCommand);

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 640, height: 420);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            try
            {
                var commandBar = GetCommandBar(commandBarFlyout);
                commandBar.ApplyTemplate();
                host.UpdateLayout();

                commandBar.IsOpen = true;
                host.UpdateLayout();
                WpfTestHost.DoEvents();

                var initialWidth = commandBar.FlyoutTemplateSettings.CurrentWidth;

                secondaryCommand.Label = "Item with a label much wider than the primary command strip";
                host.UpdateLayout();
                WpfTestHost.DoEvents();
                host.UpdateLayout();

                var updatedWidth = commandBar.FlyoutTemplateSettings.CurrentWidth;
                Assert.IsTrue(
                    updatedWidth > initialWidth,
                    $"Expected secondary command property change to refresh open width from {initialWidth} to a larger value, actual {updatedWidth}.");
            }
            finally
            {
                HideAndWait(commandBarFlyout);
            }
        });
    }

    [TestMethod]
    public void SecondaryCommandKeyboardAcceleratorChangeRefreshesOpenSizingLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            var secondaryCommand = new AppBarButton { Label = "Item" };
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right,
                ShowMode = FlyoutShowMode.Transient
            };

            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Label = "Copy" });
            commandBarFlyout.SecondaryCommands.Add(secondaryCommand);

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 640, height: 420);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            try
            {
                var commandBar = GetCommandBar(commandBarFlyout);
                commandBar.ApplyTemplate();
                host.UpdateLayout();

                commandBar.IsOpen = true;
                host.UpdateLayout();
                WpfTestHost.DoEvents();

                var initialWidth = commandBar.FlyoutTemplateSettings.CurrentWidth;

                secondaryCommand.KeyboardAcceleratorTextOverride = "Ctrl+Shift+Alt+F12";
                host.UpdateLayout();
                WpfTestHost.DoEvents();
                host.UpdateLayout();

                var updatedWidth = commandBar.FlyoutTemplateSettings.CurrentWidth;
                Assert.IsTrue(
                    updatedWidth > initialWidth,
                    $"Expected secondary command keyboard accelerator property change to refresh open width from {initialWidth} to a larger value, actual {updatedWidth}.");
            }
            finally
            {
                HideAndWait(commandBarFlyout);
            }
        });
    }

    [TestMethod]
    public void CloseAnimationCancelsFirstClosingLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right,
                ShowMode = FlyoutShowMode.Standard
            };

            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Label = "Copy" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Select all" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 420, height: 260);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            var commandBar = GetCommandBar(commandBarFlyout);
            commandBar.ApplyTemplate();
            host.UpdateLayout();

            int closingCount = 0;
            bool sawCanceledClosing = false;
            commandBarFlyout.Closing += (_, args) =>
            {
                closingCount++;
                sawCanceledClosing |= args.Cancel;
            };

            commandBarFlyout.Hide();

            if (commandBar.HasCloseAnimation())
            {
                Assert.IsTrue(sawCanceledClosing);
                Assert.AreEqual(1, closingCount);
                Assert.IsTrue(commandBarFlyout.IsOpen);
                Assert.IsTrue(commandBar.IsOpen);

                WaitFor(() => !commandBarFlyout.IsOpen, "CommandBarFlyout close animation did not complete.");

                Assert.AreEqual(2, closingCount);
                Assert.IsFalse(commandBar.IsOpen);
            }
            else
            {
                WpfTestHost.DoEvents();
                Assert.IsFalse(commandBarFlyout.IsOpen);
                Assert.IsFalse(commandBar.IsOpen);
            }
        });
    }

    [TestMethod]
    public void FlyoutAnimationsFollowWinUISourceStoryboards()
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right,
                ShowMode = FlyoutShowMode.Standard
            };

            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Share), Label = "Share" });
            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Save), Label = "Save" });
            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Delete), Label = "Delete" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Resize" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Move" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 720, height: 420);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            try
            {
                var commandBar = GetCommandBar(commandBarFlyout);
                commandBar.ApplyTemplate();
                host.UpdateLayout();

                var layoutRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "LayoutRoot");
                var openingStoryboard = layoutRoot.Resources["OpeningOpacityStoryboard"] as Storyboard;
                var closingStoryboard = layoutRoot.Resources["ClosingOpacityStoryboard"] as Storyboard;
                var collapsedToExpandedUpStoryboard = layoutRoot.Resources["CollapsedToExpandedUpStoryboard"] as Storyboard;
                var expandedUpToCollapsedStoryboard = layoutRoot.Resources["ExpandedUpToCollapsedStoryboard"] as Storyboard;
                var collapsedToExpandedDownStoryboard = layoutRoot.Resources["CollapsedToExpandedDownStoryboard"] as Storyboard;
                var expandedDownToCollapsedStoryboard = layoutRoot.Resources["ExpandedDownToCollapsedStoryboard"] as Storyboard;

                Assert.IsNotNull(openingStoryboard);
                Assert.IsNotNull(closingStoryboard);
                Assert.IsNotNull(collapsedToExpandedUpStoryboard);
                Assert.IsNotNull(expandedUpToCollapsedStoryboard);
                Assert.IsNotNull(collapsedToExpandedDownStoryboard);
                Assert.IsNotNull(expandedDownToCollapsedStoryboard);

                AssertStoryboardTargets(openingStoryboard!, "LayoutRoot", "Opacity");
                AssertStoryboardTargets(openingStoryboard!, "OuterOverflowContentRootShadowChrome", "Opacity");
                AssertStoryboardTargets(closingStoryboard!, "LayoutRoot", "Opacity");
                AssertStoryboardTargets(closingStoryboard!, "OuterOverflowContentRootShadowChrome", "Opacity");

                VerifySecondaryMenuStoryboard(collapsedToExpandedUpStoryboard!, shouldShow: true);
                VerifySecondaryMenuStoryboard(expandedUpToCollapsedStoryboard!, shouldShow: false);
                VerifySecondaryMenuStoryboard(collapsedToExpandedDownStoryboard!, shouldShow: true);
                VerifySecondaryMenuStoryboard(expandedDownToCollapsedStoryboard!, shouldShow: false);
            }
            finally
            {
                HideAndWait(commandBarFlyout);
            }
        });
    }

    [TestMethod]
    public void ExpandedFlyoutOverflowAlignsAndSurvivesSecondOpen()
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right,
                ShowMode = FlyoutShowMode.Standard
            };

            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Copy), Label = "Copy" });
            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Icon = new SymbolIcon(Symbol.Save), Label = "Save" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Resize image" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Move to another folder" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 720, height: 420);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            var commandBar = GetCommandBar(commandBarFlyout);
            commandBar.ApplyTemplate();
            WaitForExpandedOverflow(commandBar, host);
            AssertOverflowEdgesAligned(commandBar);
            AssertLayoutOpacity(commandBar, 1.0);

            HideAndWait(commandBarFlyout);
            AssertLayoutOpacity(commandBar, 1.0);
            Assert.IsFalse(FindTemplateChild<WindowedPopup>(commandBar, "OverflowPopup").IsOpen);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            commandBar = GetCommandBar(commandBarFlyout);
            commandBar.ApplyTemplate();
            WaitForExpandedOverflow(commandBar, host);
            AssertOverflowEdgesAligned(commandBar);
            AssertLayoutOpacity(commandBar, 1.0);

            HideAndWait(commandBarFlyout);
        });
    }

    [TestMethod]
    public void PresenterShadowFollowsWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right,
                ShowMode = FlyoutShowMode.Transient
            };

            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Label = "Copy" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Select all" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 420, height: 260);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            FlyoutPresenter? presenter = null;

            try
            {
                presenter = commandBarFlyout.GetPresenter();
                Assert.IsNotNull(presenter);
                presenter!.ApplyTemplate();
                host.UpdateLayout();

                Assert.IsTrue(presenter.IsDefaultShadowEnabled);

                if (System.Windows.Media.VisualTreeHelper.GetChild(presenter, 0) is not ThemeShadowChrome chrome)
                {
                    Assert.Fail("Expected FlyoutPresenter template root to be ThemeShadowChrome.");
                    return;
                }

                Assert.AreEqual(32.0, chrome.Depth);
                Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Medium, chrome.WindowedPopupInsetMode);
                Assert.AreEqual(new Thickness(10, 2, 10, 18), chrome.PopupShadowPadding);

                var commandBar = GetCommandBar(commandBarFlyout);
                commandBar.ApplyTemplate();
                host.UpdateLayout();

                commandBar.IsOpen = true;

                if (commandBar.HasSecondaryOpenCloseAnimations())
                {
                    Assert.IsFalse(presenter.IsDefaultShadowEnabled);
                }
            }
            finally
            {
                HideAndWait(commandBarFlyout);
            }

            Assert.IsFalse(presenter?.IsDefaultShadowEnabled ?? true);
            Assert.IsNull(commandBarFlyout.GetPresenter());
        });
    }

    [TestMethod]
    public void PresenterShadowStaysDisabledWithoutPrimaryCommandsLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right
            };

            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Select all" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 420, height: 260);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            try
            {
                var presenter = commandBarFlyout.GetPresenter();
                Assert.IsNotNull(presenter);
                Assert.IsFalse(presenter!.IsDefaultShadowEnabled);

                var commandBar = GetCommandBar(commandBarFlyout);
                commandBar.ApplyTemplate();
                host.UpdateLayout();

                commandBar.IsOpen = true;
                host.UpdateLayout();
                WpfTestHost.DoEvents();
                host.UpdateLayout();

                var overflowShadow = FindTemplateChild<ThemeShadowChrome>(commandBar, "OuterOverflowContentRootShadowChrome");
                WaitFor(
                    () => overflowShadow.IsShadowEnabled,
                    "CommandBarFlyout overflow shadow did not turn on for the no-primary-command source path.");

                Assert.AreEqual(32.0, overflowShadow.Depth);
                Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Medium, overflowShadow.WindowedPopupInsetMode);
                Assert.IsFalse(overflowShadow.ReservesShadowSpace);
                commandBar.ClearShadow();
                Assert.IsFalse(overflowShadow.IsShadowEnabled);
            }
            finally
            {
                HideAndWait(commandBarFlyout);
            }
        });
    }

    [TestMethod]
    public void VerifyCommandBarFlyoutStyleAndWinUI2Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var commandBarFlyout = new CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right
            };

            commandBarFlyout.PrimaryCommands.Add(new AppBarButton { Label = "Copy" });
            commandBarFlyout.SecondaryCommands.Add(new AppBarButton { Label = "Select all" });

            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 420, height: 260);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            try
            {
                var commandBar = GetCommandBar(commandBarFlyout);
                commandBar.ApplyTemplate();
                host.UpdateLayout();

                Assert.AreSame(commandBar.TryFindResource("CommandBarFlyoutBackground"), commandBar.Background);
                Assert.AreSame(commandBar.TryFindResource("CommandBarFlyoutSystemBackdrop"), commandBar.SystemBackdrop);
                Assert.AreSame(commandBar.TryFindResource("CommandBarFlyoutForeground"), commandBar.Foreground);
                Assert.AreSame(commandBar.TryFindResource("CommandBarFlyoutBorderBrush"), commandBar.BorderBrush);
                Assert.AreEqual(commandBar.TryFindResource("CommandBarFlyoutBorderThemeThickness"), commandBar.BorderThickness);
                Assert.AreEqual(440d, commandBar.MaxWidth);
                Assert.AreEqual(CommandBarPrimarySurfaceHeight, commandBar.Height);
                Assert.AreEqual(commandBar.TryFindResource("OverlayCornerRadius"), commandBar.CornerRadius);

                var contentRoot = FindTemplateChild<System.Windows.Controls.Grid>(commandBar, "ContentRoot");
                var primaryBackdropRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "PrimaryItemsSystemBackdropRoot");
                var overflowBackdropRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OverflowPopupSystemBackdropRoot");
                var overflowContentRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OverflowContentRoot");
                Assert.AreSame(commandBar.Background, contentRoot.Background);
                Assert.AreSame(commandBar.SystemBackdrop, primaryBackdropRoot.Background);
                Assert.AreSame(commandBar.SystemBackdrop, overflowBackdropRoot.Background);
                Assert.AreSame(commandBar.Background, overflowContentRoot.Background);
            }
            finally
            {
                HideAndWait(commandBarFlyout);
            }

            AssertThemeResourceReference("Light", "CommandBarFlyoutBackground", "DesktopAcrylicTransparentBrush");
            AssertThemeResourceReference("Dark", "CommandBarFlyoutBackground", "DesktopAcrylicTransparentBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutBackground", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("Light", "CommandBarFlyoutSystemBackdrop", "AcrylicBackgroundFillColorDefaultBackdrop");
            AssertThemeResourceReference("Dark", "CommandBarFlyoutSystemBackdrop", "AcrylicBackgroundFillColorDefaultBackdrop");
            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutSystemBackdrop", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("Light", "AcrylicBackgroundFillColorDefaultBackdrop", "AcrylicBackgroundFillColorDefaultBrush");
            AssertThemeResourceReference("Dark", "AcrylicBackgroundFillColorDefaultBackdrop", "AcrylicBackgroundFillColorDefaultBrush");
            AssertThemeResourceReference("HighContrast", "AcrylicBackgroundFillColorDefaultBackdrop", "SystemControlBackgroundBaseLowBrush");
            AssertThemeSolidColorBrush("Light", "DesktopAcrylicTransparentBrush", Color.FromArgb(0, 0, 0, 0));
            AssertThemeSolidColorBrush("Dark", "DesktopAcrylicTransparentBrush", Color.FromArgb(0, 0, 0, 0));
            AssertThemeSolidColorBrush("Light", "AcrylicBackgroundFillColorDefaultBrush", Color.FromRgb(0xF9, 0xF9, 0xF9));
            AssertThemeSolidColorBrush("Dark", "AcrylicBackgroundFillColorDefaultBrush", Color.FromRgb(0x2C, 0x2C, 0x2C));
            AssertThemeResourceValue("Light", "CommandBarFlyoutBorderThemeThickness", new Thickness(1));
            AssertThemeResourceValue("Dark", "CommandBarFlyoutBorderThemeThickness", new Thickness(1));
            AssertThemeResourceValue("HighContrast", "CommandBarFlyoutBorderThemeThickness", new Thickness(1));

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "CommandBarFlyoutAppBarButtonSubItemChevronPointerOverForeground", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "CommandBarFlyoutAppBarButtonSubItemChevronPressedForeground", "TextFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "CommandBarFlyoutAppBarButtonSubItemChevronSubMenuOpenedForeground", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "CommandBarFlyoutAppBarButtonSubItemChevronDisabledForeground", "TextFillColorDisabledBrush");
            }

            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutAppBarButtonBackgroundPointerOver", "SystemControlHighlightListLowBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutAppBarButtonBackgroundPressed", "SystemControlHighlightListMediumBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutAppBarButtonForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutAppBarButtonForegroundPressed", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutAppBarButtonKeyboardTextLabelForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutAppBarButtonKeyboardTextLabelForegroundPressed", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutAppBarButtonSubItemChevronPointerOverForeground", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutAppBarButtonSubItemChevronPressedForeground", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutAppBarButtonSubItemChevronSubMenuOpenedForeground", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutAppBarButtonSubItemChevronDisabledForeground", "SystemControlDisabledBaseMediumLowBrush");
        });
    }

    [TestMethod]
    public void AppBarButtonFlyoutTemplateUsesWinUIInnerChrome()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var icon = new SymbolIcon(Symbol.Accept);
            var resources = CreateCommandBarFlyoutResources();
            var button = new AppBarButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarButtonStyle"],
                Icon = icon,
                Label = "Accept",
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            var root = CreateTemplateHost(button, resources);
            using var host = new TestWindowHost(root, width: 180, height: 120);
            host.UpdateLayout();

            var innerBorder = FindTemplateChild<BorderEx>(button, "AppBarButtonInnerBorder");
            var content = FindTemplateChild<ContentPresenterEx>(button, "Content");

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, innerBorder.BackgroundSizing);
            Assert.AreEqual(button.CornerRadius, innerBorder.CornerRadius);
            Assert.IsNotNull(innerBorder.BackgroundTransition);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), innerBorder.BackgroundTransition.Duration);
            Assert.AreSame(icon, content.Content);
            Assert.AreEqual(button.Foreground, content.Foreground);
        });
    }

    [TestMethod]
    public void AppBarToggleButtonFlyoutTemplateUsesWinUIInnerChrome()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var icon = new SymbolIcon(Symbol.Accept);
            var resources = CreateCommandBarFlyoutResources();
            var button = new AppBarToggleButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarToggleButtonStyle"],
                Icon = icon,
                Label = "Accept",
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            var root = CreateTemplateHost(button, resources);
            using var host = new TestWindowHost(root, width: 180, height: 120);
            host.UpdateLayout();

            var innerBorder = FindTemplateChild<BorderEx>(button, "AppBarButtonInnerBorder");
            var content = FindTemplateChild<ContentPresenterEx>(button, "Content");

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, innerBorder.BackgroundSizing);
            Assert.IsNotNull(innerBorder.BackgroundTransition);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), innerBorder.BackgroundTransition.Duration);
            Assert.AreSame(icon, content.Content);
            Assert.AreEqual(button.Foreground, content.Foreground);
        });
    }

    [TestMethod]
    public void FlyoutAppBarTemplatesRenderExplicitContent()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = CreateCommandBarFlyoutResources();
            var buttonContent = new System.Windows.Controls.Border { Width = 20, Height = 20 };
            var toggleContent = new System.Windows.Controls.Border { Width = 20, Height = 20 };
            var button = new AppBarButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarButtonStyle"],
                Content = buttonContent,
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Content"
            };
            var toggleButton = new AppBarToggleButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarToggleButtonStyle"],
                Content = toggleContent,
                Icon = new SymbolIcon(Symbol.Pin),
                Label = "Content"
            };
            var panel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                Children = { button, toggleButton }
            };

            var root = CreateTemplateHost(panel, resources);
            using var host = new TestWindowHost(root, width: 220, height: 120);
            host.UpdateLayout();

            Assert.AreSame(buttonContent, FindTemplateChild<ContentPresenterEx>(button, "Content").Content);
            Assert.AreSame(toggleContent, FindTemplateChild<ContentPresenterEx>(toggleButton, "Content").Content);
            Assert.IsTrue(buttonContent.RenderSize.Width > 0);
            Assert.IsTrue(toggleContent.RenderSize.Width > 0);
        });
    }

    [TestMethod]
    public void FlyoutButtonKeyboardAcceleratorVisibilityUsesVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = CreateCommandBarFlyoutResources();
            var button = new AppBarButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarButtonStyle"],
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept",
                InputGestureText = "Ctrl+A"
            };
            var toggleButton = new AppBarToggleButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarToggleButtonStyle"],
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept",
                InputGestureText = "Ctrl+A"
            };

            var root = new System.Windows.Controls.StackPanel();
            root.Resources.MergedDictionaries.Add(resources);
            root.Children.Add(button);
            root.Children.Add(toggleButton);

            using var host = new TestWindowHost(root, width: 220, height: 180);
            host.UpdateLayout();

            VerifyKeyboardAcceleratorVisibilityState(button);
            VerifyKeyboardAcceleratorVisibilityState(toggleButton);
        });
    }

    [TestMethod]
    public void FlyoutButtonApplicationViewStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = CreateCommandBarFlyoutResources();
            var button = new AppBarButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarButtonStyle"],
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept"
            };
            var toggleButton = new AppBarToggleButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarToggleButtonStyle"],
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept"
            };

            var root = new System.Windows.Controls.StackPanel();
            root.Resources.MergedDictionaries.Add(resources);
            root.Children.Add(button);
            root.Children.Add(toggleButton);

            using var host = new TestWindowHost(root, width: 220, height: 180);
            host.UpdateLayout();

            VerifyAppBarButtonApplicationViewStates(button);
            VerifyAppBarToggleButtonApplicationViewStates(toggleButton);
        });
    }

    [TestMethod]
    public void FlyoutAppBarButtonCommonStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = CreateCommandBarFlyoutResources();
            var button = new AppBarButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarButtonStyle"],
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept",
                InputGestureText = "Ctrl+A"
            };

            var rootHost = CreateTemplateHost(button, resources);
            using var host = new TestWindowHost(rootHost, width: 220, height: 120);
            host.UpdateLayout();

            VerifyAppBarButtonCommonStates(button);
        });
    }

    [TestMethod]
    public void FlyoutAppBarToggleButtonCommonStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = CreateCommandBarFlyoutResources();
            var button = new AppBarToggleButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarToggleButtonStyle"],
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept",
                InputGestureText = "Ctrl+A"
            };

            var rootHost = CreateTemplateHost(button, resources);
            using var host = new TestWindowHost(rootHost, width: 220, height: 120);
            host.UpdateLayout();

            VerifyAppBarToggleButtonCommonStates(button);
        });
    }

    [TestMethod]
    public void FlyoutCommandBarAvailableAndCombinedStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = CreateCommandBarFlyoutResources();
            var commandBar = new CommandBarFlyoutCommandBar
            {
                CornerRadius = new CornerRadius(2, 4, 6, 8),
                BorderThickness = new Thickness(1),
                CommandBarOverflowPresenterStyle = (Style)resources["CommandBarFlyoutCommandBarOverflowPresenterStyle"],
                Width = 220,
                Height = 48
            };
            commandBar.PrimaryCommands.Add(new AppBarButton { Label = "Primary" });
            commandBar.SecondaryCommands.Add(new AppBarButton { Label = "Secondary" });

            var rootHost = CreateTemplateHost(commandBar, resources);
            using var host = new TestWindowHost(rootHost, width: 260, height: 160);
            host.UpdateLayout();
            WaitForExpandedOverflow(commandBar, host);
            commandBar.ClearShadow();

            VerifyFlyoutCommandBarAvailableAndCombinedStates(commandBar);
        });
    }

    [TestMethod]
    public void FlyoutCommandBarSecondaryPanelDoesNotUseWpfToolBarPanel()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = CreateCommandBarFlyoutResources();
            var plainButton = new AppBarButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarButtonStyle"],
                Label = "Undo"
            };
            var toggleButton = new AppBarToggleButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarToggleButtonStyle"],
                Label = "Pin"
            };
            var iconButton = new AppBarButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarButtonStyle"],
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept"
            };
            var commandBar = new CommandBarFlyoutCommandBar
            {
                CommandBarOverflowPresenterStyle = (Style)resources["CommandBarFlyoutCommandBarOverflowPresenterStyle"],
                Width = 220
            };
            commandBar.SecondaryCommands.Add(plainButton);
            commandBar.SecondaryCommands.Add(toggleButton);
            commandBar.SecondaryCommands.Add(iconButton);

            var rootHost = CreateTemplateHost(commandBar, resources);
            using var host = new TestWindowHost(rootHost, width: 260, height: 180);
            host.UpdateLayout();

            var secondaryPanel = FindTemplateChild<CommandBarFlyoutOverflowPanel>(commandBar, "SecondaryItemsPanel");

            AssertTypeHierarchyDoesNotContain(secondaryPanel, "ToolBarOverflowPanel");
            Assert.AreEqual(3, secondaryPanel.Children.Count);
            Assert.AreSame(plainButton, secondaryPanel.Children[0]);
            Assert.AreSame(toggleButton, secondaryPanel.Children[1]);
            Assert.AreSame(iconButton, secondaryPanel.Children[2]);
            Assert.IsTrue(plainButton.IsInOverflow);
            Assert.IsTrue(toggleButton.IsInOverflow);
            Assert.IsTrue(iconButton.IsInOverflow);
        });
    }

    [TestMethod]
    public void FlyoutSecondaryCommandsUseTouchInputModeWhenOpenedByTouchLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = CreateCommandBarFlyoutResources();
            var primaryButton = new AppBarButton { Label = "Primary" };
            var secondaryButton = new AppBarButton { Label = "Secondary" };
            var secondaryToggleButton = new AppBarToggleButton { Label = "Toggle" };
            var commandBar = new CommandBarFlyoutCommandBar
            {
                CommandBarOverflowPresenterStyle = (Style)resources["CommandBarFlyoutCommandBarOverflowPresenterStyle"],
                Width = 220,
                Height = 48
            };
            commandBar.PrimaryCommands.Add(primaryButton);
            commandBar.SecondaryCommands.Add(secondaryButton);
            commandBar.SecondaryCommands.Add(secondaryToggleButton);

            var rootHost = CreateTemplateHost(commandBar, resources);
            using var host = new TestWindowHost(rootHost, width: 260, height: 180);
            host.UpdateLayout();

            commandBar.SetLastInputModeForTesting(AppBarButtonInputMode.Touch);
            WaitForExpandedOverflow(commandBar, host);

            var primaryRoot = FindTemplateChild<System.Windows.Controls.Grid>(primaryButton, "Root");
            var secondaryRoot = FindTemplateChild<System.Windows.Controls.Grid>(secondaryButton, "Root");
            var secondaryToggleRoot = FindTemplateChild<System.Windows.Controls.Grid>(secondaryToggleButton, "Root");

            AssertCurrentState(primaryRoot, "InputModeStates", "InputModeDefault");
            AssertCurrentState(secondaryRoot, "InputModeStates", "TouchInputMode");
            AssertCurrentState(secondaryToggleRoot, "InputModeStates", "TouchInputMode");

            commandBar.IsOpen = false;
            host.UpdateLayout();
            WaitFor(() => !FindTemplateChild<WindowedPopup>(commandBar, "OverflowPopup").IsOpen, "CommandBarFlyout overflow popup did not close.");

            secondaryRoot = FindTemplateChild<System.Windows.Controls.Grid>(secondaryButton, "Root");
            secondaryToggleRoot = FindTemplateChild<System.Windows.Controls.Grid>(secondaryToggleButton, "Root");

            AssertCurrentState(secondaryRoot, "InputModeStates", "InputModeDefault");
            AssertCurrentState(secondaryToggleRoot, "InputModeStates", "InputModeDefault");
        });
    }

    [TestMethod]
    public void FlyoutOverflowPanelComputesOverflowApplicationViewStates()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = CreateCommandBarFlyoutResources();
            var plainButton = new AppBarButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarButtonStyle"],
                Label = "Undo"
            };
            var toggleButton = new AppBarToggleButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarToggleButtonStyle"],
                Label = "Pin"
            };
            var iconButton = new AppBarButton
            {
                Style = (Style)resources["CommandBarFlyoutAppBarButtonStyle"],
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept"
            };
            AppBarElementProperties.SetIsInOverflow(plainButton, true);
            AppBarElementProperties.SetIsInOverflow(toggleButton, true);
            AppBarElementProperties.SetIsInOverflow(iconButton, true);

            var overflowPanel = new CommandBarFlyoutOverflowPanel();
            overflowPanel.Children.Add(plainButton);
            overflowPanel.Children.Add(toggleButton);
            overflowPanel.Children.Add(iconButton);

            var rootHost = CreateTemplateHost(overflowPanel, resources);
            using var host = new TestWindowHost(rootHost, width: 260, height: 180);
            host.UpdateLayout();

            overflowPanel.UpdateChildrenApplicationViewState();

            AssertCurrentState(
                FindTemplateChild<System.Windows.Controls.Grid>(plainButton, "Root"),
                "ApplicationViewStates",
                "OverflowWithToggleButtonsAndMenuIcons");
            AssertCurrentState(
                FindTemplateChild<System.Windows.Controls.Grid>(toggleButton, "Root"),
                "ApplicationViewStates",
                "OverflowWithMenuIcons");
            AssertCurrentState(
                FindTemplateChild<System.Windows.Controls.Grid>(iconButton, "Root"),
                "ApplicationViewStates",
                "OverflowWithToggleButtonsAndMenuIcons");
        });
    }

    private static void AssertTypeHierarchyDoesNotContain(object value, string typeName)
    {
        for (var type = value.GetType(); type != null; type = type.BaseType)
        {
            Assert.AreNotEqual(typeName, type.Name);
        }
    }

    private static CommandBarFlyoutCommandBar GetCommandBar(CommandBarFlyout commandBarFlyout)
    {
        var presenter = commandBarFlyout.GetPresenter();
        Assert.IsNotNull(presenter);

        var commandBar = presenter.Content as CommandBarFlyoutCommandBar;
        Assert.IsNotNull(commandBar);
        return commandBar!;
    }

    private static void WaitFor(Func<bool> predicate, string failureMessage, int timeoutMilliseconds = 1500)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);

        while (DateTime.UtcNow < deadline)
        {
            if (predicate())
            {
                return;
            }

            Thread.Sleep(10);
            WpfTestHost.DoEvents();
        }

        Assert.Fail(failureMessage);
    }

    private static void HideAndWait(CommandBarFlyout commandBarFlyout)
    {
        if (!commandBarFlyout.IsOpen)
        {
            return;
        }

        var commandBar = commandBarFlyout.GetPresenter()?.Content as CommandBarFlyoutCommandBar;

        commandBarFlyout.Hide();
        WaitFor(
            () => !commandBarFlyout.IsOpen && (commandBar == null || IsLayoutOpacity(commandBar, 1.0)),
            "CommandBarFlyout did not close and reset opacity.");
    }

    private static void WaitForExpandedOverflow(CommandBarFlyoutCommandBar commandBar, TestWindowHost host)
    {
        var overflowPopup = FindTemplateChild<WindowedPopup>(commandBar, "OverflowPopup");
        var primaryItemsRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "PrimaryItemsRoot");
        var overflowContentRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OuterOverflowContentRoot");

        commandBar.IsOpen = true;
        host.UpdateLayout();
        WpfTestHost.DoEvents();
        host.UpdateLayout();

        WaitFor(
            () => commandBar.IsOpen &&
                  overflowPopup.IsOpen &&
                  IsLayoutOpacity(commandBar, 1.0) &&
                  primaryItemsRoot.ActualWidth > 0 &&
                  overflowContentRoot.ActualWidth > 0 &&
                  overflowContentRoot.IsVisible,
            "CommandBarFlyout overflow did not open with measurable layout.");
    }

    private static void AssertOverflowEdgesAligned(CommandBarFlyoutCommandBar commandBar)
    {
        var primaryItemsRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "PrimaryItemsRoot");
        var outerOverflowContentRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OuterOverflowContentRoot");
        var overflowContentRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OverflowContentRoot");
        var overflowShadow = FindTemplateChild<ThemeShadowChrome>(commandBar, "OuterOverflowContentRootShadowChrome");

        var primaryLeft = primaryItemsRoot.PointToScreen(new Point(0, 0)).X;
        var primaryRight = primaryItemsRoot.PointToScreen(new Point(primaryItemsRoot.ActualWidth, 0)).X;
        var primaryBottom = primaryItemsRoot.PointToScreen(new Point(0, primaryItemsRoot.ActualHeight)).Y;
        var outerOverflowLeft = outerOverflowContentRoot.PointToScreen(new Point(0, 0)).X;
        var outerOverflowRight = outerOverflowContentRoot.PointToScreen(new Point(outerOverflowContentRoot.ActualWidth, 0)).X;
        var overflowLeft = overflowContentRoot.PointToScreen(new Point(0, 0)).X;
        var overflowRight = overflowContentRoot.PointToScreen(new Point(overflowContentRoot.ActualWidth, 0)).X;
        var overflowTop = overflowContentRoot.PointToScreen(new Point(0, 0)).Y;

        Assert.AreEqual(
            primaryRight - primaryLeft,
            overflowRight - overflowLeft,
            1.0,
            $"Expected expanded CommandBarFlyout visible overflow width to match the primary command strip width. PrimaryWidth={primaryRight - primaryLeft}, OverflowWidth={overflowRight - overflowLeft}, OuterOverflowWidth={outerOverflowRight - outerOverflowLeft}.");
        const double expectedHorizontalGap = 2.0;
        var horizontalGap = overflowLeft - primaryLeft;
        Assert.IsTrue(
            Math.Abs(horizontalGap - expectedHorizontalGap) <= 1.0,
            $"Expected expanded CommandBarFlyout visible overflow surface to align with the primary command strip. PrimaryLeft={primaryLeft}, OverflowLeft={overflowLeft}, OuterOverflowLeft={outerOverflowLeft}, HorizontalGap={horizontalGap}, ExpectedHorizontalGap={expectedHorizontalGap}.");
        var joinGap = overflowTop - primaryBottom;
        Assert.IsTrue(
            joinGap <= 1.0 && joinGap >= -4.0,
            $"Expected expanded CommandBarFlyout overflow to touch the primary command strip with no visible gap. PrimaryBottom={primaryBottom}, OverflowTop={overflowTop}, JoinGap={joinGap}.");
    }

    private static void AssertLayoutOpacity(CommandBarFlyoutCommandBar commandBar, double expected)
    {
        var layoutRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "LayoutRoot");
        var overflowShadow = FindTemplateChild<ThemeShadowChrome>(commandBar, "OuterOverflowContentRootShadowChrome");
        var overflowContentRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OverflowContentRoot");

        Assert.AreEqual(expected, layoutRoot.Opacity, 0.001, "CommandBarFlyout layout root opacity was not reset.");
        Assert.AreEqual(expected, overflowShadow.Opacity, 0.001, "CommandBarFlyout overflow popup chrome opacity was not reset.");
        Assert.AreEqual(expected, overflowContentRoot.Opacity, 0.001, "CommandBarFlyout overflow content opacity was not reset.");
    }

    private static bool IsLayoutOpacity(CommandBarFlyoutCommandBar commandBar, double expected)
    {
        var layoutRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "LayoutRoot");
        var overflowShadow = FindTemplateChild<ThemeShadowChrome>(commandBar, "OuterOverflowContentRootShadowChrome");
        var overflowContentRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OverflowContentRoot");

        return Math.Abs(layoutRoot.Opacity - expected) <= 0.001 &&
               Math.Abs(overflowShadow.Opacity - expected) <= 0.001 &&
               Math.Abs(overflowContentRoot.Opacity - expected) <= 0.001;
    }

    private static bool StoryboardTargets(Storyboard storyboard, string targetName, string targetProperty)
    {
        return storyboard.Children.Any(timeline =>
            Storyboard.GetTargetName(timeline) == targetName &&
            (Storyboard.GetTargetProperty(timeline)?.Path?.Contains(targetProperty, StringComparison.Ordinal) ?? false));
    }

    private static void VerifySecondaryMenuStoryboard(Storyboard storyboard, bool shouldShow)
    {
        AssertStoryboardTargets(storyboard, "MoreButtonTransform", "X");
        AssertStoryboardTargets(storyboard, "ContentRootClipTransform", "X");
        AssertStoryboardTargets(storyboard, "OverflowContentRootClipTransform", "X");
        AssertStoryboardTargets(storyboard, "OverflowContentRootClipTransform", "Y");
        Assert.IsTrue(
            StoryboardTargets(storyboard, "OuterOverflowContentRoot", "Opacity"),
            shouldShow
                ? "CommandBarFlyout secondary menu open animation should expose the overflow root."
                : "CommandBarFlyout secondary menu close animation should hide the overflow root.");
    }

    private static void AssertStoryboardTargets(Storyboard storyboard, string targetName, string targetProperty)
    {
        Assert.IsTrue(
            StoryboardTargets(storyboard, targetName, targetProperty),
            $"Expected CommandBarFlyout storyboard to animate {targetName}.{targetProperty} like the WinUI source template.");
    }

    private static void VerifyCommandBarSizing(CommandBarSizingOptions sizingOptions)
    {
        WpfTestHost.Run(() =>
        {
            var commandBarFlyout = CreateSizingFlyout(sizingOptions);
            var target = new System.Windows.Controls.Button
            {
                Content = "Show CommandBarFlyout",
                Width = 180,
                Height = 36
            };

            using var host = new TestWindowHost(target, width: 640, height: 520);

            commandBarFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            try
            {
                var commandBar = GetCommandBar(commandBarFlyout);
                commandBar.ApplyTemplate();
                host.UpdateLayout();
                var overflowPopup = FindTemplateChild<WindowedPopup>(commandBar, "OverflowPopup");

                commandBar.IsOpen = false;
                host.UpdateLayout();
                WpfTestHost.DoEvents();
                WaitFor(
                    () =>
                    {
                        host.UpdateLayout();
                        return !overflowPopup.IsOpen && IsLayoutOpacity(commandBar, 1.0);
                    },
                    "CommandBarFlyout did not settle into its collapsed sizing state.");

                var collapsedWidth = commandBar.FlyoutTemplateSettings.CurrentWidth;
                var collapsedHeight = commandBar.ActualHeight;

                WaitForExpandedOverflow(commandBar, host);

                var expandedWidth = commandBar.FlyoutTemplateSettings.CurrentWidth;
                var overflowPresenter = FindDescendant<CommandBarOverflowPresenter>(commandBar);

                if (sizingOptions == CommandBarSizingOptions.PrimaryItemsLarger ||
                    sizingOptions == CommandBarSizingOptions.SecondaryItemsMaxHeight)
                {
                    Assert.AreEqual(collapsedWidth, expandedWidth, 0.5);
                    Assert.AreEqual(expandedWidth, overflowPresenter.ActualWidth, 0.5);
                }
                else
                {
                    Assert.IsTrue(
                        expandedWidth > collapsedWidth,
                        $"Expected expanded width {expandedWidth} to be greater than collapsed width {collapsedWidth}.");
                    Assert.AreEqual(expandedWidth, overflowPresenter.ActualWidth, 0.5);
                }

                Assert.AreEqual(collapsedHeight, commandBar.ActualHeight, 0.5);

                if (sizingOptions == CommandBarSizingOptions.SecondaryItemsMaxWidth)
                {
                    Assert.AreEqual(commandBar.MaxWidth, expandedWidth, 0.5);
                }
                else if (sizingOptions == CommandBarSizingOptions.SecondaryItemsMaxHeight)
                {
                    Assert.AreEqual(overflowPresenter.MaxHeight, overflowPresenter.ActualHeight, 0.5);
                }
            }
            finally
            {
                HideAndWait(commandBarFlyout);
            }
        });
    }

    private static CommandBarFlyout CreateSizingFlyout(CommandBarSizingOptions sizingOptions)
    {
        var flyout = new CommandBarFlyout
        {
            Placement = FlyoutPlacementMode.Right,
            ShowMode = FlyoutShowMode.Transient
        };

        for (var i = 1; i <= 6; i++)
        {
            flyout.PrimaryCommands.Add(new AppBarButton { Label = $"Primary {i}" });
        }

        flyout.SecondaryCommands.Add(new AppBarButton { Label = "Undo" });
        flyout.SecondaryCommands.Add(new AppBarButton { Label = "Redo" });
        flyout.SecondaryCommands.Add(new AppBarButton { Label = "Select all" });

        switch (sizingOptions)
        {
            case CommandBarSizingOptions.SecondaryItemsLarger:
                flyout.SecondaryCommands.Add(new AppBarButton { Label = "Item with a label much wider than the primary command strip" });
                break;

            case CommandBarSizingOptions.SecondaryItemsMaxWidth:
                flyout.SecondaryCommands.Add(new AppBarButton { Label = "Item with a really really really long label that will not fit in the space provided" });
                break;

            case CommandBarSizingOptions.SecondaryItemsMaxHeight:
                for (var i = 0; i < 20; i++)
                {
                    flyout.SecondaryCommands.Add(new AppBarButton { Label = "Do another thing" });
                }
                break;
        }

        return flyout;
    }

    private static void VerifyCommandCollections(CommandBarFlyout commandBarFlyout, CommandBarFlyoutCommandBar commandBar)
    {
        Assert.AreEqual(commandBarFlyout.PrimaryCommands.Count, commandBar.PrimaryCommands.Count);
        for (var i = 0; i < commandBarFlyout.PrimaryCommands.Count; i++)
        {
            Assert.AreSame(commandBarFlyout.PrimaryCommands[i], commandBar.PrimaryCommands[i]);
        }

        Assert.AreEqual(commandBarFlyout.SecondaryCommands.Count, commandBar.SecondaryCommands.Count);
        for (var i = 0; i < commandBarFlyout.SecondaryCommands.Count; i++)
        {
            Assert.AreSame(commandBarFlyout.SecondaryCommands[i], commandBar.SecondaryCommands[i]);
        }
    }

    private static void VerifyPrimaryCommandBottomLabel(AppBarButton button, string expectedLabel)
    {
        var root = FindTemplateChild<System.Windows.Controls.Grid>(button, "Root");
        var contentRoot = FindTemplateChild<System.Windows.Controls.Grid>(button, "ContentRoot");
        var iconAndLabelPanel = FindTemplateChild<System.Windows.Controls.Grid>(button, "IconAndLabelPanel");
        var textLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "TextLabel");
        var overflowTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "OverflowTextLabel");

        AssertCurrentState(root, "ApplicationViewStates", "FullSize");
        AssertCurrentState(root, "PrimaryLabelStates", "HasPrimaryLabels");
        Assert.AreEqual(PrimaryCommandActualWidth, button.ActualWidth);
        Assert.AreEqual(PrimaryCommandHeight, button.ActualHeight);
        Assert.IsTrue(double.IsNaN(contentRoot.Width));
        Assert.AreEqual(PrimaryCommandContentMinWidth, contentRoot.MinWidth);
        Assert.AreEqual(0.0, contentRoot.MinHeight);
        Assert.AreEqual(PrimaryLabelPanelWidth, iconAndLabelPanel.Width);
        Assert.AreEqual(new Thickness(0, 9, 0, 0), iconAndLabelPanel.Margin);
        Assert.AreEqual(VerticalAlignment.Top, iconAndLabelPanel.VerticalAlignment);
        Assert.AreEqual(expectedLabel, textLabel.Text);
        Assert.AreEqual(Visibility.Visible, textLabel.Visibility);
        Assert.AreEqual(Visibility.Collapsed, overflowTextLabel.Visibility);
    }

    private static ResourceDictionary CreateCommandBarFlyoutResources()
    {
        return new ResourceDictionary
        {
            Source = new Uri("/ModernWpf.Controls;component/CommandBarFlyout/CommandBarFlyout.xaml", UriKind.Relative)
        };
    }

    private static FrameworkElement CreateTemplateHost(UIElement child, ResourceDictionary resources)
    {
        var root = new System.Windows.Controls.Grid();
        root.Resources.MergedDictionaries.Add(resources);
        root.Children.Add(child);
        return root;
    }

    private static void VerifyKeyboardAcceleratorVisibilityState(System.Windows.Controls.Control control)
    {
        var root = FindTemplateChild<System.Windows.Controls.Grid>(control, "Root");
        var label = FindTemplateChild<System.Windows.Controls.TextBlock>(control, "KeyboardAcceleratorTextLabel");

        AssertStateSetter(
            root,
            "KeyboardAcceleratorTextVisibility",
            "KeyboardAcceleratorTextVisible",
            "KeyboardAcceleratorTextLabel.Visibility");

        Assert.AreEqual(Visibility.Collapsed, label.Visibility);
        Assert.IsTrue(VisualStateManager.GoToState(control, "KeyboardAcceleratorTextVisible", false));
        Assert.AreEqual(Visibility.Visible, label.Visibility);
        Assert.IsTrue(VisualStateManager.GoToState(control, "KeyboardAcceleratorTextCollapsed", false));
        Assert.AreEqual(Visibility.Collapsed, label.Visibility);
    }

    private static void VerifyAppBarButtonApplicationViewStates(AppBarButton button)
    {
        var root = FindTemplateChild<System.Windows.Controls.Grid>(button, "Root");
        var contentRoot = FindTemplateChild<System.Windows.Controls.Grid>(button, "ContentRoot");
        var iconAndLabelPanel = FindTemplateChild<System.Windows.Controls.Grid>(button, "IconAndLabelPanel");
        var contentViewbox = FindTemplateChild<System.Windows.Controls.Viewbox>(button, "ContentViewbox");
        var textLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "TextLabel");
        var overflowTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "OverflowTextLabel");

        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "Compact",
            "ContentRoot.Width",
            "TextLabel.Visibility");
        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "LabelOnRight",
            "ContentRoot.Width",
            "ContentViewbox.Margin",
            "TextLabel.(Grid.Row)",
            "TextLabel.(Grid.Column)",
            "TextLabel.TextAlignment",
            "TextLabel.Margin",
            "TextLabel.VerticalAlignment");
        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "LabelCollapsed",
            "ContentRoot.Width",
            "TextLabel.Visibility");
        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "Overflow",
            "ContentRoot.MinHeight",
            "ContentRoot.Width",
            "ContentViewbox.Visibility",
            "ContentViewbox.Margin",
            "TextLabel.Visibility",
            "OverflowTextLabel.Visibility");
        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "OverflowWithToggleButtons",
            "ContentRoot.MinHeight",
            "ContentRoot.Width",
            "ContentViewbox.Visibility",
            "TextLabel.Visibility",
            "OverflowTextLabel.Visibility",
            "OverflowTextLabel.Margin");
        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "OverflowWithMenuIcons",
            "ContentRoot.MinHeight",
            "ContentRoot.Width",
            "ContentViewbox.HorizontalAlignment",
            "ContentViewbox.VerticalAlignment",
            "ContentViewbox.Width",
            "ContentViewbox.Height",
            "ContentViewbox.Margin",
            "TextLabel.Visibility",
            "OverflowTextLabel.Visibility",
            "OverflowTextLabel.Margin");
        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "OverflowWithToggleButtonsAndMenuIcons",
            "ContentRoot.MinHeight",
            "ContentRoot.Width",
            "ContentViewbox.HorizontalAlignment",
            "ContentViewbox.VerticalAlignment",
            "ContentViewbox.Width",
            "ContentViewbox.Height",
            "ContentViewbox.Margin",
            "TextLabel.Visibility",
            "OverflowTextLabel.Visibility",
            "OverflowTextLabel.Margin");
        AssertStateSetter(
            root,
            "InputModeStates",
            "TouchInputMode",
            "OverflowTextLabel.Padding");
        AssertStateSetter(
            root,
            "InputModeStates",
            "GameControllerInputMode",
            "OverflowTextLabel.Padding");
        AssertStateSetter(
            root,
            "PrimaryLabelStates",
            "HasPrimaryLabels",
            "IconAndLabelPanel.Margin",
            "IconAndLabelPanel.VerticalAlignment",
            "IconAndLabelPanel.Width",
            "TextLabel.Visibility");

        Assert.IsTrue(double.IsNaN(button.Width));
        Assert.IsTrue(double.IsNaN(button.Height));
        Assert.IsTrue(double.IsNaN(contentRoot.Width));
        Assert.AreEqual(PrimaryCommandContentMinWidth, contentRoot.MinWidth);
        Assert.AreEqual(0.0, contentRoot.MinHeight);
        Assert.AreEqual(Visibility.Collapsed, textLabel.Visibility);
        Assert.AreEqual("Accept", textLabel.Text);
        Assert.AreEqual(Visibility.Collapsed, overflowTextLabel.Visibility);
        Assert.AreEqual(new Thickness(0, 6, 0, 7), overflowTextLabel.Padding);

        Assert.IsTrue(VisualStateManager.GoToState(button, "HasPrimaryLabels", false));
        Assert.AreEqual(PrimaryLabelPanelWidth, iconAndLabelPanel.Width);
        Assert.AreEqual(new Thickness(0, 9, 0, 0), iconAndLabelPanel.Margin);
        Assert.AreEqual(VerticalAlignment.Top, iconAndLabelPanel.VerticalAlignment);
        Assert.AreEqual(Visibility.Visible, textLabel.Visibility);

        Assert.IsTrue(VisualStateManager.GoToState(button, "OverflowWithToggleButtonsAndMenuIcons", false));

        Assert.AreEqual(0.0, contentRoot.MinHeight);
        Assert.IsTrue(double.IsNaN(contentRoot.Width));
        Assert.AreEqual(HorizontalAlignment.Left, contentViewbox.HorizontalAlignment);
        Assert.AreEqual(VerticalAlignment.Center, contentViewbox.VerticalAlignment);
        Assert.AreEqual(16.0, contentViewbox.Width);
        Assert.AreEqual(16.0, contentViewbox.Height);
        Assert.AreEqual(new Thickness(39, 0, 12, 0), contentViewbox.Margin);
        Assert.AreEqual(Visibility.Collapsed, textLabel.Visibility);
        Assert.AreEqual(Visibility.Visible, overflowTextLabel.Visibility);
        Assert.AreEqual(new Thickness(67, 0, 12, 0), overflowTextLabel.Margin);

        Assert.IsTrue(VisualStateManager.GoToState(button, "NoPrimaryLabels", false));
        Assert.IsTrue(VisualStateManager.GoToState(button, "FullSize", false));

        Assert.IsTrue(double.IsNaN(contentRoot.Width));
        Assert.AreEqual(Visibility.Collapsed, textLabel.Visibility);
        Assert.IsTrue(double.IsNaN(contentViewbox.Width));
        Assert.AreEqual(Visibility.Collapsed, overflowTextLabel.Visibility);

        Assert.IsTrue(VisualStateManager.GoToState(button, "TouchInputMode", false));
        Assert.AreEqual(new Thickness(0, 9, 0, 11), overflowTextLabel.Padding);

        Assert.IsTrue(VisualStateManager.GoToState(button, "InputModeDefault", false));
        Assert.AreEqual(new Thickness(0, 6, 0, 7), overflowTextLabel.Padding);
    }

    private static void VerifyAppBarToggleButtonApplicationViewStates(AppBarToggleButton button)
    {
        var root = FindTemplateChild<System.Windows.Controls.Grid>(button, "Root");
        var contentRoot = FindTemplateChild<System.Windows.Controls.Grid>(button, "ContentRoot");
        var iconAndLabelPanel = FindTemplateChild<System.Windows.Controls.Grid>(button, "IconAndLabelPanel");
        var contentViewbox = FindTemplateChild<System.Windows.Controls.Viewbox>(button, "ContentViewbox");
        var overflowCheckGlyph = FindTemplateChild<FrameworkElement>(button, "OverflowCheckGlyph");
        var textLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "TextLabel");
        var overflowTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "OverflowTextLabel");

        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "Compact",
            "ContentRoot.Width",
            "TextLabel.Visibility");
        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "LabelOnRight",
            "ContentRoot.Width",
            "ContentViewbox.Margin",
            "TextLabel.(Grid.Row)",
            "TextLabel.(Grid.Column)",
            "TextLabel.TextAlignment",
            "TextLabel.Margin",
            "TextLabel.VerticalAlignment");
        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "LabelCollapsed",
            "ContentRoot.Width",
            "TextLabel.Visibility");
        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "Overflow",
            "ContentRoot.MinHeight",
            "ContentRoot.Width",
            "ContentViewbox.Visibility",
            "OverflowCheckGlyph.Visibility",
            "TextLabel.Visibility",
            "OverflowTextLabel.Visibility");
        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "OverflowWithMenuIcons",
            "ContentRoot.MinHeight",
            "ContentRoot.Width",
            "ContentViewbox.Visibility",
            "ContentViewbox.HorizontalAlignment",
            "ContentViewbox.VerticalAlignment",
            "ContentViewbox.MaxWidth",
            "ContentViewbox.MaxHeight",
            "ContentViewbox.Margin",
            "OverflowCheckGlyph.Visibility",
            "TextLabel.Visibility",
            "OverflowTextLabel.Visibility",
            "OverflowTextLabel.Margin");
        AssertStateSetter(
            root,
            "InputModeStates",
            "TouchInputMode",
            "OverflowTextLabel.Padding",
            "OverflowCheckGlyph.Margin");
        AssertStateSetter(
            root,
            "InputModeStates",
            "GameControllerInputMode",
            "OverflowTextLabel.Padding",
            "OverflowCheckGlyph.Margin");
        AssertStateSetter(
            root,
            "PrimaryLabelStates",
            "HasPrimaryLabels",
            "IconAndLabelPanel.Margin",
            "IconAndLabelPanel.VerticalAlignment",
            "IconAndLabelPanel.Width",
            "TextLabel.Visibility");

        Assert.IsTrue(double.IsNaN(button.Width));
        Assert.IsTrue(double.IsNaN(button.Height));
        Assert.IsTrue(double.IsNaN(contentRoot.Width));
        Assert.AreEqual(PrimaryCommandContentMinWidth, contentRoot.MinWidth);
        Assert.AreEqual(0.0, contentRoot.MinHeight);
        Assert.AreEqual(Visibility.Collapsed, overflowCheckGlyph.Visibility);
        Assert.AreEqual(Visibility.Collapsed, textLabel.Visibility);
        Assert.AreEqual("Accept", textLabel.Text);
        Assert.AreEqual(Visibility.Collapsed, overflowTextLabel.Visibility);
        Assert.AreEqual(new Thickness(15, 4, 14, 4), overflowCheckGlyph.Margin);
        Assert.AreEqual(new Thickness(0, 6, 0, 7), overflowTextLabel.Padding);

        Assert.IsTrue(VisualStateManager.GoToState(button, "HasPrimaryLabels", false));
        Assert.AreEqual(PrimaryLabelPanelWidth, iconAndLabelPanel.Width);
        Assert.AreEqual(new Thickness(0, 9, 0, 0), iconAndLabelPanel.Margin);
        Assert.AreEqual(VerticalAlignment.Top, iconAndLabelPanel.VerticalAlignment);
        Assert.AreEqual(Visibility.Visible, textLabel.Visibility);

        Assert.IsTrue(VisualStateManager.GoToState(button, "OverflowWithMenuIcons", false));

        Assert.AreEqual(0.0, contentRoot.MinHeight);
        Assert.IsTrue(double.IsNaN(contentRoot.Width));
        Assert.AreEqual(Visibility.Visible, contentViewbox.Visibility);
        Assert.AreEqual(HorizontalAlignment.Left, contentViewbox.HorizontalAlignment);
        Assert.AreEqual(VerticalAlignment.Center, contentViewbox.VerticalAlignment);
        Assert.AreEqual(16.0, contentViewbox.MaxWidth);
        Assert.AreEqual(16.0, contentViewbox.MaxHeight);
        Assert.AreEqual(new Thickness(39, 0, 12, 0), contentViewbox.Margin);
        Assert.AreEqual(Visibility.Visible, overflowCheckGlyph.Visibility);
        Assert.AreEqual(Visibility.Collapsed, textLabel.Visibility);
        Assert.AreEqual(Visibility.Visible, overflowTextLabel.Visibility);
        Assert.AreEqual(new Thickness(67, 0, 12, 0), overflowTextLabel.Margin);

        Assert.IsTrue(VisualStateManager.GoToState(button, "NoPrimaryLabels", false));
        Assert.IsTrue(VisualStateManager.GoToState(button, "FullSize", false));

        Assert.IsTrue(double.IsNaN(contentRoot.Width));
        Assert.IsTrue(double.IsPositiveInfinity(contentViewbox.MaxWidth));
        Assert.AreEqual(Visibility.Collapsed, overflowCheckGlyph.Visibility);
        Assert.AreEqual(Visibility.Collapsed, textLabel.Visibility);
        Assert.AreEqual(Visibility.Collapsed, overflowTextLabel.Visibility);

        Assert.IsTrue(VisualStateManager.GoToState(button, "GameControllerInputMode", false));
        Assert.AreEqual(new Thickness(12, 10, 12, 10), overflowCheckGlyph.Margin);
        Assert.AreEqual(new Thickness(0, 9, 0, 11), overflowTextLabel.Padding);

        Assert.IsTrue(VisualStateManager.GoToState(button, "InputModeDefault", false));
        Assert.AreEqual(new Thickness(15, 4, 14, 4), overflowCheckGlyph.Margin);
        Assert.AreEqual(new Thickness(0, 6, 0, 7), overflowTextLabel.Padding);
    }

    private static void VerifyAppBarButtonCommonStates(AppBarButton button)
    {
        var root = FindTemplateChild<System.Windows.Controls.Grid>(button, "Root");
        var innerBorder = FindTemplateChild<BorderEx>(button, "AppBarButtonInnerBorder");
        var content = FindTemplateChild<ContentPresenterEx>(button, "Content");
        var textLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "TextLabel");
        var overflowTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "OverflowTextLabel");
        var keyboardAcceleratorTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "KeyboardAcceleratorTextLabel");

        AssertStateSetter(
            root,
            "CommonStates",
            "PointerOver",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "TextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "Pressed",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "TextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "Disabled",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "TextLabel.Foreground",
            "OverflowTextLabel.Foreground",
            "KeyboardAcceleratorTextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "OverflowPointerOver",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "OverflowTextLabel.Foreground",
            "KeyboardAcceleratorTextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "OverflowPressed",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "OverflowTextLabel.Foreground",
            "KeyboardAcceleratorTextLabel.Foreground");

        Assert.AreSame(button.Background, innerBorder.Background);
        Assert.AreSame(button.Foreground, content.Foreground);

        button.IsEnabled = false;

        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonBackgroundDisabled"), innerBorder.Background);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), content.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), textLabel.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), overflowTextLabel.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), keyboardAcceleratorTextLabel.Foreground);

        button.IsEnabled = true;

        Assert.AreSame(button.Background, innerBorder.Background);
        Assert.AreSame(button.Foreground, content.Foreground);
    }

    private static void VerifyAppBarToggleButtonCommonStates(AppBarToggleButton button)
    {
        var root = FindTemplateChild<System.Windows.Controls.Grid>(button, "Root");
        var innerBorder = FindTemplateChild<BorderEx>(button, "AppBarButtonInnerBorder");
        var content = FindTemplateChild<ContentPresenterEx>(button, "Content");
        var overflowCheckGlyph = FindTemplateChild<FontIconFallback>(button, "OverflowCheckGlyph");
        var textLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "TextLabel");
        var overflowTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "OverflowTextLabel");
        var keyboardAcceleratorTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "KeyboardAcceleratorTextLabel");

        AssertStateSetter(
            root,
            "CommonStates",
            "PointerOver",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "TextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "Pressed",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "TextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "Disabled",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "TextLabel.Foreground",
            "OverflowTextLabel.Foreground",
            "KeyboardAcceleratorTextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "Checked",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "TextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "CheckedPointerOver",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "TextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "CheckedPressed",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "TextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "CheckedDisabled",
            "Content.Foreground",
            "TextLabel.Foreground",
            "OverflowCheckGlyph.Foreground",
            "OverflowTextLabel.Foreground",
            "KeyboardAcceleratorTextLabel.Foreground",
            "OverflowCheckGlyph.Opacity");
        AssertStateSetter(
            root,
            "CommonStates",
            "OverflowPointerOver",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "OverflowTextLabel.Foreground",
            "KeyboardAcceleratorTextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "OverflowPressed",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "OverflowTextLabel.Foreground",
            "KeyboardAcceleratorTextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "OverflowChecked",
            "OverflowCheckGlyph.Opacity");
        AssertStateSetter(
            root,
            "CommonStates",
            "OverflowCheckedPointerOver",
            "OverflowCheckGlyph.Opacity",
            "OverflowCheckGlyph.Foreground",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "OverflowTextLabel.Foreground",
            "KeyboardAcceleratorTextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "OverflowCheckedPressed",
            "OverflowCheckGlyph.Opacity",
            "OverflowCheckGlyph.Foreground",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "OverflowTextLabel.Foreground",
            "KeyboardAcceleratorTextLabel.Foreground");

        Assert.AreSame(button.Background, innerBorder.Background);
        Assert.AreSame(button.Foreground, content.Foreground);
        Assert.AreEqual(0.0, overflowCheckGlyph.Opacity);

        button.IsEnabled = false;

        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonBackgroundDisabled"), innerBorder.Background);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), content.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), textLabel.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), overflowTextLabel.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), keyboardAcceleratorTextLabel.Foreground);

        button.IsEnabled = true;
        button.IsChecked = true;

        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonBackgroundChecked"), innerBorder.Background);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundChecked"), content.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundChecked"), textLabel.Foreground);

        button.IsEnabled = false;

        Assert.AreSame(button.Background, innerBorder.Background);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonBackgroundDisabled"), content.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), textLabel.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), overflowCheckGlyph.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), overflowTextLabel.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), keyboardAcceleratorTextLabel.Foreground);
        Assert.AreEqual(1.0, overflowCheckGlyph.Opacity);

        button.IsEnabled = true;

        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonBackgroundChecked"), innerBorder.Background);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundChecked"), content.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundChecked"), textLabel.Foreground);
        Assert.AreEqual(0.0, overflowCheckGlyph.Opacity);
    }

    private static void VerifyFlyoutCommandBarAvailableAndCombinedStates(CommandBarFlyoutCommandBar commandBar)
    {
        var layoutRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "LayoutRoot");
        var primaryItemsRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "PrimaryItemsRoot");
        var overflowPopup = FindTemplateChild<WindowedPopup>(commandBar, "OverflowPopup");
        var overflowShadow = FindTemplateChild<ThemeShadowChrome>(commandBar, "OuterOverflowContentRootShadowChrome");
        var outerOverflowContentRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OuterOverflowContentRoot");
        var overflowContentRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OverflowContentRoot");
        var overflowTopJoinSeparator = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OverflowTopJoinSeparator");
        var overflowBottomJoinSeparator = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OverflowBottomJoinSeparator");
        var secondaryItemsControl = FindTemplateChild<CommandBarOverflowPresenter>(commandBar, "SecondaryItemsControl");
        var overflowPanel = FindTemplateChild<CommandBarFlyoutOverflowPanel>(commandBar, "SecondaryItemsPanel");

        AssertStateSetter(
            layoutRoot,
            "AvailableCommandsStates",
            "PrimaryCommandsOnly",
            "OverflowContentRoot.Visibility");
        AssertStateSetter(
            layoutRoot,
            "AvailableCommandsStates",
            "SecondaryCommandsOnly",
            "PrimaryItemsRoot.Opacity",
            "PrimaryItemsRoot.IsHitTestVisible",
            "PrimaryItemsRoot.Height",
            "SecondaryItemsPanel.Focusable");
        AssertStateSetter(
            layoutRoot,
            "CombinedStates",
            "ExpandedUpWithPrimaryCommands",
            "SecondaryItemsControl.BorderThickness",
            "LayoutRoot.CornerRadius",
            "PrimaryItemsRoot.CornerRadius",
            "OuterOverflowContentRoot.CornerRadius",
            "SecondaryItemsControl.CornerRadius",
            "OverflowTopJoinSeparator.Visibility",
            "OverflowBottomJoinSeparator.Visibility");
        AssertStateSetter(
            layoutRoot,
            "CombinedStates",
            "ExpandedDownWithPrimaryCommands",
            "SecondaryItemsControl.BorderThickness",
            "LayoutRoot.CornerRadius",
            "PrimaryItemsRoot.CornerRadius",
            "OuterOverflowContentRoot.CornerRadius",
            "SecondaryItemsControl.CornerRadius",
            "OverflowTopJoinSeparator.Visibility",
            "OverflowBottomJoinSeparator.Visibility");
        AssertStateSetter(
            layoutRoot,
            "CombinedStates",
            "ExpandedUpWithoutPrimaryCommands",
            "SecondaryItemsControl.BorderThickness",
            "LayoutRoot.CornerRadius",
            "PrimaryItemsRoot.CornerRadius",
            "OuterOverflowContentRoot.CornerRadius",
            "SecondaryItemsControl.CornerRadius",
            "OverflowTopJoinSeparator.Visibility",
            "OverflowBottomJoinSeparator.Visibility");
        AssertStateSetter(
            layoutRoot,
            "CombinedStates",
            "ExpandedDownWithoutPrimaryCommands",
            "SecondaryItemsControl.BorderThickness",
            "LayoutRoot.CornerRadius",
            "PrimaryItemsRoot.CornerRadius",
            "OuterOverflowContentRoot.CornerRadius",
            "SecondaryItemsControl.CornerRadius",
            "OverflowTopJoinSeparator.Visibility",
            "OverflowBottomJoinSeparator.Visibility");
        AssertStateSetter(
            layoutRoot,
            "OuterOverflowContentRootShadowStates",
            "OuterOverflowContentRootShadow",
            "OuterOverflowContentRootShadowChrome.IsShadowEnabled");

        Assert.IsFalse(overflowShadow.IsShadowEnabled);
        Assert.AreEqual(32.0, overflowShadow.Depth);
        Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Medium, overflowShadow.WindowedPopupInsetMode);
        Assert.IsFalse(overflowShadow.ReservesShadowSpace);
        Assert.AreEqual(new Thickness(10, 2, 10, 18), overflowShadow.PopupShadowPadding);
        Assert.AreEqual(outerOverflowContentRoot.CornerRadius, overflowShadow.CornerRadius);

        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "OuterOverflowContentRootShadow", false));
        Assert.IsTrue(overflowShadow.IsShadowEnabled);
        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "NoOuterOverflowContentRootShadow", false));
        Assert.IsFalse(overflowShadow.IsShadowEnabled);
        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "OuterOverflowContentRootShadow", false));
        commandBar.ClearShadow();
        Assert.IsFalse(overflowShadow.IsShadowEnabled);

        Assert.AreEqual(Visibility.Visible, overflowContentRoot.Visibility);
        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "PrimaryCommandsOnly", false));
        Assert.AreEqual(Visibility.Collapsed, overflowContentRoot.Visibility);
        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "BothCommands", false));
        Assert.AreEqual(Visibility.Visible, overflowContentRoot.Visibility);

        Assert.AreEqual(Visibility.Visible, primaryItemsRoot.Visibility);
        Assert.AreEqual(1.0, primaryItemsRoot.Opacity);
        Assert.IsTrue(primaryItemsRoot.IsHitTestVisible);
        Assert.IsTrue(double.IsNaN(primaryItemsRoot.Height));
        Assert.AreEqual(PopupPlacementMode.BottomEdgeAlignedLeft, overflowPopup.DesiredPlacement);
        Assert.AreSame(primaryItemsRoot, overflowPopup.PlacementTarget);
        Assert.IsFalse(overflowPanel.Focusable);
        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "SecondaryCommandsOnly", false));
        Assert.AreEqual(Visibility.Visible, primaryItemsRoot.Visibility);
        Assert.AreEqual(0.0, primaryItemsRoot.Opacity);
        Assert.IsFalse(primaryItemsRoot.IsHitTestVisible);
        Assert.AreEqual(0.0, primaryItemsRoot.Height);
        Assert.AreEqual(PopupPlacementMode.BottomEdgeAlignedLeft, overflowPopup.DesiredPlacement);
        Assert.AreSame(primaryItemsRoot, overflowPopup.PlacementTarget);
        Assert.IsTrue(overflowPanel.Focusable);
        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "BothCommands", false));
        Assert.AreEqual(Visibility.Visible, primaryItemsRoot.Visibility);
        Assert.AreEqual(1.0, primaryItemsRoot.Opacity);
        Assert.IsTrue(primaryItemsRoot.IsHitTestVisible);
        Assert.IsTrue(double.IsNaN(primaryItemsRoot.Height));
        Assert.AreEqual(PopupPlacementMode.BottomEdgeAlignedLeft, overflowPopup.DesiredPlacement);
        Assert.AreSame(primaryItemsRoot, overflowPopup.PlacementTarget);
        Assert.IsFalse(overflowPanel.Focusable);

        var topCornerRadius = new CornerRadius(2, 4, 0, 0);
        var bottomCornerRadius = new CornerRadius(0, 0, 6, 8);

        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "ExpandedUpWithPrimaryCommands", false));
        Assert.AreEqual(commandBar.TryFindResource("CommandBarFlyoutBorderUpThemeThickness"), secondaryItemsControl.BorderThickness);
        Assert.AreEqual(bottomCornerRadius, layoutRoot.CornerRadius);
        Assert.AreEqual(bottomCornerRadius, primaryItemsRoot.CornerRadius);
        Assert.AreEqual(topCornerRadius, outerOverflowContentRoot.CornerRadius);
        Assert.AreEqual(topCornerRadius, secondaryItemsControl.CornerRadius);
        Assert.AreEqual(Visibility.Collapsed, overflowTopJoinSeparator.Visibility);
        Assert.AreEqual(Visibility.Visible, overflowBottomJoinSeparator.Visibility);

        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "ExpandedDownWithPrimaryCommands", false));
        Assert.AreEqual(commandBar.TryFindResource("CommandBarFlyoutBorderDownThemeThickness"), secondaryItemsControl.BorderThickness);
        Assert.AreEqual(topCornerRadius, layoutRoot.CornerRadius);
        Assert.AreEqual(topCornerRadius, primaryItemsRoot.CornerRadius);
        Assert.AreEqual(bottomCornerRadius, outerOverflowContentRoot.CornerRadius);
        Assert.AreEqual(bottomCornerRadius, secondaryItemsControl.CornerRadius);
        Assert.AreEqual(Visibility.Visible, overflowTopJoinSeparator.Visibility);
        Assert.AreEqual(Visibility.Collapsed, overflowBottomJoinSeparator.Visibility);

        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "ExpandedUpWithoutPrimaryCommands", false));
        Assert.AreEqual(commandBar.TryFindResource("CommandBarFlyoutBorderThemeThickness"), secondaryItemsControl.BorderThickness);
        Assert.AreEqual(commandBar.CornerRadius, layoutRoot.CornerRadius);
        Assert.AreEqual(commandBar.CornerRadius, primaryItemsRoot.CornerRadius);
        Assert.AreEqual(commandBar.CornerRadius, outerOverflowContentRoot.CornerRadius);
        Assert.AreEqual(commandBar.CornerRadius, secondaryItemsControl.CornerRadius);
        Assert.AreEqual(Visibility.Collapsed, overflowTopJoinSeparator.Visibility);
        Assert.AreEqual(Visibility.Collapsed, overflowBottomJoinSeparator.Visibility);
    }

    private static VisualStateEx AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        params string[] expectedTargets)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(candidate => candidate.Name == groupName);
        var state = group.States
            .OfType<VisualState>()
            .Single(candidate => candidate.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        CollectionAssert.AreEquivalent(expectedTargets, stateEx.Setters.Select(setter => setter.Target).ToArray());
        return stateEx;
    }

    private static void AssertCurrentState(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(candidate => candidate.Name == groupName);

        Assert.AreEqual(stateName, group.CurrentState?.Name);
    }

    private static T FindTemplateChild<T>(System.Windows.Controls.Control control, string name)
        where T : DependencyObject
    {
        control.ApplyTemplate();

        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template child '{name}' to be {typeof(T).Name}.");
    }

    private static Rect GetNativeWindowRect(IntPtr hwnd)
    {
        var rect = new NativeRect();
        if (!NativeGetWindowRect(hwnd, ref rect))
        {
            throw new AssertFailedException("GetWindowRect failed for the CommandBarFlyout popup HWND.");
        }

        return new Rect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    private static void MoveNativePointerTo(FrameworkElement element, double offsetY = 0)
    {
        var point = element.PointToScreen(new Point(element.ActualWidth / 2, element.ActualHeight / 2 + offsetY));
        NativeSetCursorPos((int)Math.Round(point.X), (int)Math.Round(point.Y));
        NativeMouseEvent(NativeMouseMove, 1, 0, 0, UIntPtr.Zero);
        NativeMouseEvent(NativeMouseMove, unchecked((uint)-1), 0, 0, UIntPtr.Zero);
        WpfTestHost.DoEvents();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowRect", SetLastError = true)]
    private static extern bool NativeGetWindowRect(IntPtr hWnd, ref NativeRect rect);

    [DllImport("user32.dll", EntryPoint = "SetCursorPos", SetLastError = true)]
    private static extern bool NativeSetCursorPos(int x, int y);

    [DllImport("user32.dll", EntryPoint = "GetCursorPos", SetLastError = true)]
    private static extern bool NativeGetCursorPos(out NativePoint point);

    [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
    private static extern bool NativeSetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "mouse_event")]
    private static extern void NativeMouseEvent(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    private const uint NativeMouseMove = 0x0001;
    private const uint NativeMouseLeftDown = 0x0002;
    private const uint NativeMouseLeftUp = 0x0004;

    private static void AssertThemeResourceReference(string themeName, string resourceKey, string expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static void AssertThemeSolidColorBrush(string themeName, string resourceKey, Color expectedColor)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        var brush = themeDictionary[resourceKey] as SolidColorBrush
            ?? throw new AssertFailedException($"{themeName}:{resourceKey} should be a SolidColorBrush.");
        Assert.AreEqual(expectedColor, brush.Color, $"{themeName}:{resourceKey}");
    }

    private static T FindDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        return EnumerateDescendantsIncludingPopupChildren(root)
            .OfType<T>()
            .Single();
    }

    private static IEnumerable<DependencyObject> EnumerateDescendantsIncludingPopupChildren(DependencyObject root)
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            yield return descendant;

            if (descendant is Popup { Child: { } child })
            {
                yield return child;

                foreach (var popupChildDescendant in EnumerateDescendantsIncludingPopupChildren(child))
                {
                    yield return popupChildDescendant;
                }
            }

            if (descendant is WindowedPopup { Child: { } windowedPopupChild })
            {
                yield return windowedPopupChild;

                foreach (var popupChildDescendant in EnumerateDescendantsIncludingPopupChildren(windowedPopupChild))
                {
                    yield return popupChildDescendant;
                }
            }
        }
    }
}
