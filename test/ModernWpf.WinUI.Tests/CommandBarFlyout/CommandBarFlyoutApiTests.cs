using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
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
                Assert.AreSame(commandBar.TryFindResource("CommandBarFlyoutForeground"), commandBar.Foreground);
                Assert.AreSame(commandBar.TryFindResource("CommandBarFlyoutBorderBrush"), commandBar.BorderBrush);
                Assert.AreEqual(commandBar.TryFindResource("CommandBarFlyoutBorderThemeThickness"), commandBar.BorderThickness);
                Assert.AreEqual(440d, commandBar.MaxWidth);
                Assert.AreEqual(48d, commandBar.Height);
                Assert.AreEqual(commandBar.TryFindResource("OverlayCornerRadius"), commandBar.CornerRadius);
            }
            finally
            {
                HideAndWait(commandBarFlyout);
            }

            AssertThemeResourceReference("Light", "CommandBarFlyoutBackground", "AcrylicInAppFillColorDefaultBrush");
            AssertThemeResourceReference("Dark", "CommandBarFlyoutBackground", "AcrylicInAppFillColorDefaultBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarFlyoutBackground", "SystemControlBackgroundBaseLowBrush");
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

        commandBarFlyout.Hide();
        WaitFor(() => !commandBarFlyout.IsOpen, "CommandBarFlyout did not close.");
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

                commandBar.IsOpen = false;
                host.UpdateLayout();
                WpfTestHost.DoEvents();

                var collapsedWidth = commandBar.FlyoutTemplateSettings.CurrentWidth;
                var collapsedHeight = commandBar.ActualHeight;

                commandBar.IsOpen = true;
                host.UpdateLayout();
                WpfTestHost.DoEvents();

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
        var contentViewbox = FindTemplateChild<System.Windows.Controls.Viewbox>(button, "ContentViewbox");
        var overflowTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "OverflowTextLabel");

        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "Overflow",
            "ContentRoot.MinHeight",
            "ContentRoot.Width",
            "ContentViewbox.Visibility",
            "ContentViewbox.Margin",
            "OverflowTextLabel.Visibility");
        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "OverflowWithToggleButtons",
            "ContentRoot.MinHeight",
            "ContentRoot.Width",
            "ContentViewbox.Visibility",
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

        Assert.AreEqual(40.0, contentRoot.Width);
        Assert.AreEqual(Visibility.Collapsed, overflowTextLabel.Visibility);
        Assert.AreEqual(new Thickness(0, 5, 0, 7), overflowTextLabel.Padding);

        Assert.IsTrue(VisualStateManager.GoToState(button, "OverflowWithToggleButtonsAndMenuIcons", false));

        Assert.AreEqual(0.0, contentRoot.MinHeight);
        Assert.IsTrue(double.IsNaN(contentRoot.Width));
        Assert.AreEqual(HorizontalAlignment.Left, contentViewbox.HorizontalAlignment);
        Assert.AreEqual(VerticalAlignment.Center, contentViewbox.VerticalAlignment);
        Assert.AreEqual(16.0, contentViewbox.Width);
        Assert.AreEqual(16.0, contentViewbox.Height);
        Assert.AreEqual(new Thickness(39, 0, 12, 0), contentViewbox.Margin);
        Assert.AreEqual(Visibility.Visible, overflowTextLabel.Visibility);
        Assert.AreEqual(new Thickness(67, 0, 12, 0), overflowTextLabel.Margin);

        Assert.IsTrue(VisualStateManager.GoToState(button, "FullSize", false));

        Assert.AreEqual(40.0, contentRoot.Width);
        Assert.IsTrue(double.IsNaN(contentViewbox.Width));
        Assert.AreEqual(Visibility.Collapsed, overflowTextLabel.Visibility);

        Assert.IsTrue(VisualStateManager.GoToState(button, "TouchInputMode", false));
        Assert.AreEqual(new Thickness(0, 9, 0, 11), overflowTextLabel.Padding);

        Assert.IsTrue(VisualStateManager.GoToState(button, "InputModeDefault", false));
        Assert.AreEqual(new Thickness(0, 5, 0, 7), overflowTextLabel.Padding);
    }

    private static void VerifyAppBarToggleButtonApplicationViewStates(AppBarToggleButton button)
    {
        var root = FindTemplateChild<System.Windows.Controls.Grid>(button, "Root");
        var contentRoot = FindTemplateChild<System.Windows.Controls.Grid>(button, "ContentRoot");
        var contentViewbox = FindTemplateChild<System.Windows.Controls.Viewbox>(button, "ContentViewbox");
        var overflowCheckGlyph = FindTemplateChild<FrameworkElement>(button, "OverflowCheckGlyph");
        var overflowTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "OverflowTextLabel");

        AssertStateSetter(
            root,
            "ApplicationViewStates",
            "Overflow",
            "ContentRoot.MinHeight",
            "ContentRoot.Width",
            "ContentViewbox.Visibility",
            "OverflowCheckGlyph.Visibility",
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

        Assert.AreEqual(40.0, contentRoot.Width);
        Assert.AreEqual(Visibility.Collapsed, overflowCheckGlyph.Visibility);
        Assert.AreEqual(Visibility.Collapsed, overflowTextLabel.Visibility);
        Assert.AreEqual(new Thickness(15, 4, 14, 4), overflowCheckGlyph.Margin);
        Assert.AreEqual(new Thickness(0, 5, 0, 7), overflowTextLabel.Padding);

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
        Assert.AreEqual(Visibility.Visible, overflowTextLabel.Visibility);
        Assert.AreEqual(new Thickness(67, 0, 12, 0), overflowTextLabel.Margin);

        Assert.IsTrue(VisualStateManager.GoToState(button, "FullSize", false));

        Assert.AreEqual(40.0, contentRoot.Width);
        Assert.IsTrue(double.IsPositiveInfinity(contentViewbox.MaxWidth));
        Assert.AreEqual(Visibility.Collapsed, overflowCheckGlyph.Visibility);
        Assert.AreEqual(Visibility.Collapsed, overflowTextLabel.Visibility);

        Assert.IsTrue(VisualStateManager.GoToState(button, "GameControllerInputMode", false));
        Assert.AreEqual(new Thickness(12, 10, 12, 10), overflowCheckGlyph.Margin);
        Assert.AreEqual(new Thickness(0, 9, 0, 11), overflowTextLabel.Padding);

        Assert.IsTrue(VisualStateManager.GoToState(button, "InputModeDefault", false));
        Assert.AreEqual(new Thickness(15, 4, 14, 4), overflowCheckGlyph.Margin);
        Assert.AreEqual(new Thickness(0, 5, 0, 7), overflowTextLabel.Padding);
    }

    private static void VerifyAppBarButtonCommonStates(AppBarButton button)
    {
        var root = FindTemplateChild<System.Windows.Controls.Grid>(button, "Root");
        var innerBorder = FindTemplateChild<BorderEx>(button, "AppBarButtonInnerBorder");
        var content = FindTemplateChild<ContentPresenterEx>(button, "Content");
        var overflowTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "OverflowTextLabel");
        var keyboardAcceleratorTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "KeyboardAcceleratorTextLabel");

        AssertStateSetter(
            root,
            "CommonStates",
            "PointerOver",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "Pressed",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "Disabled",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
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
        var overflowTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "OverflowTextLabel");
        var keyboardAcceleratorTextLabel = FindTemplateChild<System.Windows.Controls.TextBlock>(button, "KeyboardAcceleratorTextLabel");

        AssertStateSetter(
            root,
            "CommonStates",
            "PointerOver",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "Pressed",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "Disabled",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground",
            "OverflowTextLabel.Foreground",
            "KeyboardAcceleratorTextLabel.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "Checked",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "CheckedPointerOver",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "CheckedPressed",
            "AppBarButtonInnerBorder.Background",
            "Content.Foreground");
        AssertStateSetter(
            root,
            "CommonStates",
            "CheckedDisabled",
            "Content.Foreground",
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
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), overflowTextLabel.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), keyboardAcceleratorTextLabel.Foreground);

        button.IsEnabled = true;
        button.IsChecked = true;

        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonBackgroundChecked"), innerBorder.Background);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundChecked"), content.Foreground);

        button.IsEnabled = false;

        Assert.AreSame(button.Background, innerBorder.Background);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonBackgroundDisabled"), content.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), overflowCheckGlyph.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), overflowTextLabel.Foreground);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundDisabled"), keyboardAcceleratorTextLabel.Foreground);
        Assert.AreEqual(1.0, overflowCheckGlyph.Opacity);

        button.IsEnabled = true;

        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonBackgroundChecked"), innerBorder.Background);
        Assert.AreSame(button.TryFindResource("CommandBarFlyoutAppBarButtonForegroundChecked"), content.Foreground);
        Assert.AreEqual(0.0, overflowCheckGlyph.Opacity);
    }

    private static void VerifyFlyoutCommandBarAvailableAndCombinedStates(CommandBarFlyoutCommandBar commandBar)
    {
        var layoutRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "LayoutRoot");
        var primaryItemsRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "PrimaryItemsRoot");
        var overflowPopup = FindTemplateChild<System.Windows.Controls.Primitives.Popup>(commandBar, "OverflowPopup");
        var outerOverflowContentRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OuterOverflowContentRoot");
        var overflowContentRoot = FindTemplateChild<System.Windows.Controls.Border>(commandBar, "OverflowContentRoot");
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
            "PrimaryItemsRoot.Visibility",
            "OverflowPopup.Placement",
            "SecondaryItemsPanel.Focusable");
        AssertStateSetter(
            layoutRoot,
            "CombinedStates",
            "ExpandedUpWithPrimaryCommands",
            "SecondaryItemsControl.BorderThickness",
            "LayoutRoot.CornerRadius",
            "PrimaryItemsRoot.CornerRadius",
            "OuterOverflowContentRoot.CornerRadius",
            "SecondaryItemsControl.CornerRadius");
        AssertStateSetter(
            layoutRoot,
            "CombinedStates",
            "ExpandedDownWithPrimaryCommands",
            "SecondaryItemsControl.BorderThickness",
            "LayoutRoot.CornerRadius",
            "PrimaryItemsRoot.CornerRadius",
            "OuterOverflowContentRoot.CornerRadius",
            "SecondaryItemsControl.CornerRadius");
        AssertStateSetter(
            layoutRoot,
            "CombinedStates",
            "ExpandedUpWithoutPrimaryCommands",
            "SecondaryItemsControl.BorderThickness",
            "LayoutRoot.CornerRadius",
            "PrimaryItemsRoot.CornerRadius",
            "OuterOverflowContentRoot.CornerRadius",
            "SecondaryItemsControl.CornerRadius");
        AssertStateSetter(
            layoutRoot,
            "CombinedStates",
            "ExpandedDownWithoutPrimaryCommands",
            "SecondaryItemsControl.BorderThickness",
            "LayoutRoot.CornerRadius",
            "PrimaryItemsRoot.CornerRadius",
            "OuterOverflowContentRoot.CornerRadius",
            "SecondaryItemsControl.CornerRadius");

        Assert.AreEqual(Visibility.Visible, overflowContentRoot.Visibility);
        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "PrimaryCommandsOnly", false));
        Assert.AreEqual(Visibility.Collapsed, overflowContentRoot.Visibility);
        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "BothCommands", false));
        Assert.AreEqual(Visibility.Visible, overflowContentRoot.Visibility);

        Assert.AreEqual(Visibility.Visible, primaryItemsRoot.Visibility);
        Assert.AreEqual(PlacementMode.Bottom, overflowPopup.Placement);
        Assert.IsFalse(overflowPanel.Focusable);
        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "SecondaryCommandsOnly", false));
        Assert.AreEqual(Visibility.Collapsed, primaryItemsRoot.Visibility);
        Assert.AreEqual(PlacementMode.Relative, overflowPopup.Placement);
        Assert.IsTrue(overflowPanel.Focusable);
        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "BothCommands", false));
        Assert.AreEqual(Visibility.Visible, primaryItemsRoot.Visibility);
        Assert.AreEqual(PlacementMode.Bottom, overflowPopup.Placement);
        Assert.IsFalse(overflowPanel.Focusable);

        var topCornerRadius = new CornerRadius(2, 4, 0, 0);
        var bottomCornerRadius = new CornerRadius(0, 0, 6, 8);

        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "ExpandedUpWithPrimaryCommands", false));
        Assert.AreEqual(commandBar.TryFindResource("CommandBarFlyoutBorderUpThemeThickness"), secondaryItemsControl.BorderThickness);
        Assert.AreEqual(bottomCornerRadius, layoutRoot.CornerRadius);
        Assert.AreEqual(bottomCornerRadius, primaryItemsRoot.CornerRadius);
        Assert.AreEqual(topCornerRadius, outerOverflowContentRoot.CornerRadius);
        Assert.AreEqual(topCornerRadius, secondaryItemsControl.CornerRadius);

        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "ExpandedDownWithPrimaryCommands", false));
        Assert.AreEqual(commandBar.TryFindResource("CommandBarFlyoutBorderDownThemeThickness"), secondaryItemsControl.BorderThickness);
        Assert.AreEqual(topCornerRadius, layoutRoot.CornerRadius);
        Assert.AreEqual(topCornerRadius, primaryItemsRoot.CornerRadius);
        Assert.AreEqual(bottomCornerRadius, outerOverflowContentRoot.CornerRadius);
        Assert.AreEqual(bottomCornerRadius, secondaryItemsControl.CornerRadius);

        Assert.IsTrue(VisualStateManager.GoToState(commandBar, "ExpandedUpWithoutPrimaryCommands", false));
        Assert.AreEqual(commandBar.TryFindResource("CommandBarFlyoutBorderThemeThickness"), secondaryItemsControl.BorderThickness);
        Assert.AreEqual(commandBar.CornerRadius, layoutRoot.CornerRadius);
        Assert.AreEqual(commandBar.CornerRadius, primaryItemsRoot.CornerRadius);
        Assert.AreEqual(commandBar.CornerRadius, outerOverflowContentRoot.CornerRadius);
        Assert.AreEqual(commandBar.CornerRadius, secondaryItemsControl.CornerRadius);
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
        }
    }
}
