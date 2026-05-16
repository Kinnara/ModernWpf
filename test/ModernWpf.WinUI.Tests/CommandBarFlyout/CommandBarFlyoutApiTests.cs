using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
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

            commandBarFlyout.Hide();
            WpfTestHost.DoEvents();
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
                commandBarFlyout.Hide();
                WpfTestHost.DoEvents();
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

    private static CommandBarFlyoutCommandBar GetCommandBar(CommandBarFlyout commandBarFlyout)
    {
        var presenter = commandBarFlyout.GetPresenter();
        Assert.IsNotNull(presenter);

        var commandBar = presenter.Content as CommandBarFlyoutCommandBar;
        Assert.IsNotNull(commandBar);
        return commandBar!;
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

                var toolBar = FindDescendant<CommandBarFlyoutToolBar>(commandBar);

                commandBar.IsOpen = false;
                host.UpdateLayout();

                var collapsedWidth = toolBar.FlyoutTemplateSettings.CurrentWidth;
                var collapsedHeight = commandBar.ActualHeight;

                commandBar.IsOpen = true;
                host.UpdateLayout();
                WpfTestHost.DoEvents();

                var expandedWidth = toolBar.FlyoutTemplateSettings.CurrentWidth;
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
                commandBarFlyout.Hide();
                WpfTestHost.DoEvents();
            }
        });
    }

    private static CommandBarFlyout CreateSizingFlyout(CommandBarSizingOptions sizingOptions)
    {
        var flyout = new CommandBarFlyout
        {
            Placement = FlyoutPlacementMode.Right
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

    private static void VerifyCommandCollections(CommandBarFlyout commandBarFlyout, CommandBar commandBar)
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

        Assert.AreEqual(40.0, contentRoot.Width);
        Assert.AreEqual(Visibility.Collapsed, overflowTextLabel.Visibility);

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

        Assert.AreEqual(40.0, contentRoot.Width);
        Assert.AreEqual(Visibility.Collapsed, overflowCheckGlyph.Visibility);
        Assert.AreEqual(Visibility.Collapsed, overflowTextLabel.Visibility);

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
