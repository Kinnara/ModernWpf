using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommandBars;

[TestClass]
public class CommandBarApiTests
{
    [TestMethod]
    public void VerifyCommandBarDefaultPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var commandBar = new ModernWpf.Controls.CommandBar();

            Assert.IsNotNull(commandBar.PrimaryCommands);
            Assert.AreEqual(0, commandBar.PrimaryCommands.Count);
            Assert.IsNotNull(commandBar.SecondaryCommands);
            Assert.AreEqual(0, commandBar.SecondaryCommands.Count);
            Assert.IsNull(commandBar.Content);
            Assert.IsNull(commandBar.ContentTemplate);
            Assert.IsNull(commandBar.CommandBarOverflowPresenterStyle);
            Assert.IsFalse(commandBar.IsOpen);
            Assert.IsFalse(commandBar.IsSticky);
            Assert.IsTrue(commandBar.IsDynamicOverflowEnabled);
            Assert.AreEqual(CommandBarDefaultLabelPosition.Right, commandBar.DefaultLabelPosition);
            Assert.AreEqual(CommandBarOverflowButtonVisibility.Auto, commandBar.OverflowButtonVisibility);
        });
    }

    [TestMethod]
    public void VerifyCommandBarPropertyGettersAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var overflowStyle = new Style(typeof(CommandBarOverflowPresenter));
            var commandBar = new ModernWpf.Controls.CommandBar
            {
                Content = "Title",
                ContentTemplate = new DataTemplate(),
                CommandBarOverflowPresenterStyle = overflowStyle,
                IsOpen = true,
                IsSticky = true,
                IsDynamicOverflowEnabled = false,
                DefaultLabelPosition = CommandBarDefaultLabelPosition.Collapsed,
                OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Visible
            };

            Assert.AreEqual("Title", commandBar.Content);
            Assert.IsNotNull(commandBar.ContentTemplate);
            Assert.AreSame(overflowStyle, commandBar.CommandBarOverflowPresenterStyle);
            Assert.IsTrue(commandBar.IsOpen);
            Assert.IsTrue(commandBar.IsSticky);
            Assert.IsFalse(commandBar.IsDynamicOverflowEnabled);
            Assert.AreEqual(CommandBarDefaultLabelPosition.Collapsed, commandBar.DefaultLabelPosition);
            Assert.AreEqual(CommandBarOverflowButtonVisibility.Visible, commandBar.OverflowButtonVisibility);
        });
    }

    [TestMethod]
    public void CommandBarIsStickyControlsOverflowLightDismissPolicy()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var commandBar = new TestCommandBar();
            commandBar.SecondaryCommands.Add(new AppBarButton { Label = "Settings", InputGestureText = "Ctrl+I" });

            using var host = new TestWindowHost(commandBar, width: 320, height: 160);
            host.UpdateLayout();

            var popup = FindTemplateChild<Popup>(commandBar, "OverflowPopup");
            Assert.AreSame(FindTemplateChild<Grid>(commandBar, "ContentRoot"), popup.PlacementTarget);
            Assert.IsTrue(popup.StaysOpen);

            commandBar.IsSticky = true;
            host.UpdateLayout();
            Assert.IsTrue(popup.StaysOpen);

            commandBar.IsSticky = false;
            commandBar.IsOpen = true;
            commandBar.IsSticky = true;
            host.UpdateLayout();
            WpfTestHost.DoEvents();
            Assert.IsTrue(commandBar.IsOpen);
            Assert.IsTrue(popup.IsOpen);
            var settingsButton = (AppBarButton)commandBar.SecondaryCommands[0];
            var acceleratorText = FindTemplateChild<TextBlock>(settingsButton, "KeyboardAcceleratorTextLabel");
            Assert.AreEqual(Visibility.Visible, acceleratorText.Visibility);

            commandBar.InvokeKeyDown(CreateKeyEventArgs(commandBar, Key.Escape));
            Assert.IsTrue(commandBar.IsOpen, "Escape must not dismiss a sticky WinUI CommandBar.");
            Assert.IsFalse(commandBar.TryLightDismissForTesting(new Button()));
            Assert.IsTrue(commandBar.IsOpen, "Outside input must not dismiss a sticky WinUI CommandBar.");

            commandBar.IsSticky = false;
            host.UpdateLayout();
            Assert.IsTrue(popup.StaysOpen, "ModernWpf owns WinUI light-dismiss semantics instead of WPF Popup capture.");
            Assert.IsFalse(commandBar.TryLightDismissForTesting(settingsButton));
            Assert.IsTrue(commandBar.IsOpen, "Input inside overflow must not light-dismiss the CommandBar.");

            commandBar.InvokeKeyDown(CreateKeyEventArgs(commandBar, Key.Escape));
            Assert.IsFalse(commandBar.IsOpen, "Escape must dismiss a non-sticky WinUI CommandBar.");

            commandBar.IsOpen = true;
            host.UpdateLayout();
            Assert.IsTrue(commandBar.TryLightDismissForTesting(new Button()));
            Assert.IsFalse(commandBar.IsOpen, "Outside input must dismiss a non-sticky WinUI CommandBar.");
        });
    }

    [TestMethod]
    public void CommandCollectionsApplyWinUIOverflowStyleRouting()
    {
        WpfTestHost.Run(() =>
        {
            var commandBar = new ModernWpf.Controls.CommandBar
            {
                IsDynamicOverflowEnabled = false
            };
            var primary = new AppBarButton();
            var secondary = new AppBarButton();

            commandBar.PrimaryCommands.Add(primary);
            commandBar.SecondaryCommands.Add(secondary);

            Assert.IsFalse(primary.IsInOverflow);
            Assert.IsTrue(secondary.IsInOverflow);

            commandBar.IsDynamicOverflowEnabled = false;
            Assert.IsFalse(primary.IsInOverflow);

            var secondPrimary = new AppBarButton();
            commandBar.PrimaryCommands.Add(secondPrimary);
            Assert.IsFalse(secondPrimary.IsInOverflow);

            commandBar.IsDynamicOverflowEnabled = true;
            Assert.IsFalse(primary.IsInOverflow);
            Assert.IsFalse(secondPrimary.IsInOverflow);
            Assert.IsTrue(secondary.IsInOverflow);
        });
    }

    [TestMethod]
    public void CommandBarDynamicOverflowUsesVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var commandBar = new ModernWpf.Controls.CommandBar
            {
                Content = "Title",
                IsDynamicOverflowEnabled = false
            };

            using var host = new TestWindowHost(commandBar, width: 300, height: 80);
            host.UpdateLayout();

            var contentColumn = FindTemplateChild<ColumnDefinition>(commandBar, "ContentControlColumnDefinition");
            var primaryColumn = FindTemplateChild<ColumnDefinition>(commandBar, "PrimaryItemsControlColumnDefinition");

            Assert.AreEqual(GridUnitType.Star, contentColumn.Width.GridUnitType);
            Assert.AreEqual(GridUnitType.Auto, primaryColumn.Width.GridUnitType);

            commandBar.IsDynamicOverflowEnabled = true;
            host.UpdateLayout();

            Assert.AreEqual(GridUnitType.Auto, contentColumn.Width.GridUnitType);
            Assert.AreEqual(GridUnitType.Star, primaryColumn.Width.GridUnitType);

            commandBar.IsDynamicOverflowEnabled = false;
            host.UpdateLayout();

            Assert.AreEqual(GridUnitType.Star, contentColumn.Width.GridUnitType);
            Assert.AreEqual(GridUnitType.Auto, primaryColumn.Width.GridUnitType);
        });
    }

    [TestMethod]
    public void AppBarElementsExposeCurrentWinUIDynamicOverflowOrderContract()
    {
        WpfTestHost.Run(() =>
        {
            ICommandBarElement[] elements =
            {
                new AppBarButton(),
                new AppBarToggleButton(),
                new AppBarSeparator(),
                new AppBarElementContainer()
            };

            foreach (var element in elements)
            {
                Assert.AreEqual(0, element.DynamicOverflowOrder);
                Assert.IsFalse(element.IsInOverflow);
            }

            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].DynamicOverflowOrder = i + 1;
                Assert.AreEqual(i + 1, elements[i].DynamicOverflowOrder);
            }

            Assert.AreSame(AppBarButton.DynamicOverflowOrderProperty, AppBarToggleButton.DynamicOverflowOrderProperty);
            Assert.AreSame(AppBarButton.DynamicOverflowOrderProperty, AppBarSeparator.DynamicOverflowOrderProperty);
            Assert.AreSame(AppBarButton.DynamicOverflowOrderProperty, AppBarElementContainer.DynamicOverflowOrderProperty);
        });
    }

    [TestMethod]
    public void CommandBarDynamicOverflowUsesSourceOrderGroupsAndReactsToOrderChanges()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var first = new AppBarButton { Label = "First" };
            var second = new AppBarButton { Label = "Second" };
            var third = new AppBarButton { Label = "Third", DynamicOverflowOrder = 1 };
            var fourth = new AppBarButton { Label = "Fourth", DynamicOverflowOrder = 2 };
            var commandBar = new ModernWpf.Controls.CommandBar();
            commandBar.PrimaryCommands.Add(first);
            commandBar.PrimaryCommands.Add(second);
            commandBar.PrimaryCommands.Add(third);
            commandBar.PrimaryCommands.Add(fourth);

            using var host = new TestWindowHost(commandBar, width: 190, height: 100);
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsFalse(first.IsInOverflow);
            Assert.IsFalse(second.IsInOverflow);
            Assert.IsTrue(third.IsInOverflow);
            Assert.IsTrue(fourth.IsInOverflow);

            first.DynamicOverflowOrder = 1;
            second.DynamicOverflowOrder = 2;
            third.DynamicOverflowOrder = 0;
            fourth.DynamicOverflowOrder = 0;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(first.IsInOverflow);
            Assert.IsTrue(second.IsInOverflow);
            Assert.IsFalse(third.IsInOverflow);
            Assert.IsFalse(fourth.IsInOverflow);
        });
    }

    [TestMethod]
    public void CommandBarDynamicOverflowMovesWholeOrderGroupsAndAdjacentSeparators()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var leadingSeparator = new AppBarSeparator();
            var first = new AppBarButton { Label = "First", DynamicOverflowOrder = 1 };
            var trailingSeparator = new AppBarSeparator();
            var second = new AppBarButton { Label = "Second", DynamicOverflowOrder = 2 };
            var commandBar = new ModernWpf.Controls.CommandBar();
            commandBar.PrimaryCommands.Add(leadingSeparator);
            commandBar.PrimaryCommands.Add(first);
            commandBar.PrimaryCommands.Add(trailingSeparator);
            commandBar.PrimaryCommands.Add(second);

            using var host = new TestWindowHost(commandBar, width: 120, height: 100);
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(leadingSeparator.IsInOverflow);
            Assert.IsTrue(first.IsInOverflow);
            Assert.IsTrue(trailingSeparator.IsInOverflow);
            Assert.IsFalse(second.IsInOverflow);

            var groupCommandBar = new ModernWpf.Controls.CommandBar();
            var grouped = Enumerable.Range(0, 4)
                .Select(index => new AppBarButton
                {
                    Label = "Grouped " + index,
                    DynamicOverflowOrder = 1
                })
                .ToList();
            foreach (var button in grouped)
            {
                groupCommandBar.PrimaryCommands.Add(button);
            }

            host.Window.Content = groupCommandBar;
            host.Window.Width = 190;
            host.Window.Height = 100;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(grouped.All(button => button.IsInOverflow));
        });
    }

    [TestMethod]
    public void CommandBarDynamicOverflowChangingEventUsesCurrentWinUIActionAndTiming()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var commandBar = new ModernWpf.Controls.CommandBar();
            var commands = Enumerable.Range(0, 4)
                .Select(index => new AppBarButton { Label = "Command " + index })
                .ToList();
            foreach (var command in commands)
            {
                commandBar.PrimaryCommands.Add(command);
            }

            var actions = new List<CommandBarDynamicOverflowAction>();
            commandBar.DynamicOverflowItemsChanging += (sender, args) =>
            {
                Assert.AreSame(commandBar, sender);
                actions.Add(args.Action);

                if (args.Action == CommandBarDynamicOverflowAction.AddingToOverflow)
                {
                    Assert.IsTrue(commands.All(command => !command.IsInOverflow));
                }
                else
                {
                    Assert.IsTrue(commands.Any(command => command.IsInOverflow));
                }
            };

            using var host = new TestWindowHost(commandBar, width: 190, height: 100);
            host.UpdateLayout();

            CollectionAssert.AreEqual(
                new[] { CommandBarDynamicOverflowAction.AddingToOverflow },
                actions);
            Assert.IsTrue(commands.Any(command => command.IsInOverflow));

            host.Window.Width = 600;
            host.UpdateLayout();

            CollectionAssert.AreEqual(
                new[]
                {
                    CommandBarDynamicOverflowAction.AddingToOverflow,
                    CommandBarDynamicOverflowAction.RemovingFromOverflow
                },
                actions);
            Assert.IsTrue(commands.All(command => !command.IsInOverflow));
        });
    }

    [TestMethod]
    public void CommandBarOpenLifecycleUsesCurrentWinUIEventAndVirtualHookOrder()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var lifecycle = new List<string>();
            var commandBar = new TestCommandBar { LifecycleLog = lifecycle };
            commandBar.SecondaryCommands.Add(new AppBarButton { Label = "Settings" });
            commandBar.Opening += (sender, args) => lifecycle.Add("Opening");
            commandBar.Opened += (sender, args) => lifecycle.Add("Opened");
            commandBar.Closing += (sender, args) => lifecycle.Add("Closing");
            commandBar.Closed += (sender, args) => lifecycle.Add("Closed");

            using var host = new TestWindowHost(commandBar, width: 320, height: 160);
            commandBar.IsOpen = true;
            host.UpdateLayout();
            commandBar.IsOpen = false;
            host.UpdateLayout();
            for (int i = 0; i < 5 && !lifecycle.Contains("Closed"); i++)
            {
                WpfTestHost.DoEvents();
            }

            Assert.AreEqual(
                "OnOpening,Opening,OnOpened,Opened,OnClosing,Closing,OnClosed,Closed",
                string.Join(",", lifecycle));
        });
    }

    [TestMethod]
    public void CommandBarAutomationPeerUsesCurrentWinUIAppBarPatterns()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var commandBar = new ModernWpf.Controls.CommandBar();
            commandBar.SecondaryCommands.Add(new AppBarButton { Label = "Settings" });
            using var host = new TestWindowHost(commandBar, width: 320, height: 160);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(commandBar);
            Assert.IsInstanceOfType(peer, typeof(CommandBarAutomationPeer));
            Assert.AreEqual("ApplicationBar", peer.GetClassName());
            Assert.AreEqual("app bar", peer.GetLocalizedControlType());
            Assert.AreEqual(AutomationControlType.Custom, peer.GetAutomationControlType());

            var toggleProvider = (IToggleProvider)peer.GetPattern(PatternInterface.Toggle);
            var expandCollapseProvider = (IExpandCollapseProvider)peer.GetPattern(PatternInterface.ExpandCollapse);
            Assert.IsNotNull(toggleProvider);
            Assert.IsNotNull(expandCollapseProvider);
            Assert.AreEqual(ToggleState.Off, toggleProvider.ToggleState);
            Assert.AreEqual(ExpandCollapseState.Collapsed, expandCollapseProvider.ExpandCollapseState);
            Assert.IsNull(peer.GetPattern(PatternInterface.Window));

            expandCollapseProvider.Expand();
            host.UpdateLayout();
            Assert.IsTrue(commandBar.IsOpen);
            Assert.AreEqual(ToggleState.On, toggleProvider.ToggleState);
            Assert.AreEqual(ExpandCollapseState.Expanded, expandCollapseProvider.ExpandCollapseState);

            var windowProvider = (IWindowProvider)peer.GetPattern(PatternInterface.Window);
            Assert.IsNotNull(windowProvider);
            Assert.IsTrue(windowProvider.IsModal);
            Assert.IsTrue(windowProvider.IsTopmost);
            Assert.IsFalse(windowProvider.Maximizable);
            Assert.IsFalse(windowProvider.Minimizable);
            Assert.AreEqual(WindowInteractionState.Running, windowProvider.InteractionState);
            Assert.AreEqual(WindowVisualState.Normal, windowProvider.VisualState);

            toggleProvider.Toggle();
            host.UpdateLayout();
            Assert.IsFalse(commandBar.IsOpen);
            Assert.AreEqual(ToggleState.Off, toggleProvider.ToggleState);
            Assert.IsNull(peer.GetPattern(PatternInterface.Window));
        });
    }

    [TestMethod]
    public void CommandBarOverflowShadowUsesSourceWrapper()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var commandBar = new ModernWpf.Controls.CommandBar
            {
                IsDynamicOverflowEnabled = false
            };
            commandBar.SecondaryCommands.Add(new AppBarButton { Label = "Share" });

            using var host = new TestWindowHost(commandBar, width: 320, height: 160);
            host.UpdateLayout();

            var popup = FindTemplateChild<Popup>(commandBar, "OverflowPopup");
            var overflowContentRoot = FindTemplateChild<Grid>(commandBar, "OverflowContentRoot");
            var shadowWrapper = FindTemplateChild<ThemeShadowChrome>(commandBar, "SecondaryItemsControlShadowWrapper");
            var secondaryItemsControl = FindTemplateChild<CommandBarOverflowPresenter>(commandBar, "SecondaryItemsControl");

            Assert.AreSame(overflowContentRoot, popup.Child);
            Assert.AreSame(secondaryItemsControl, shadowWrapper.Child);
            Assert.AreEqual(32.0, shadowWrapper.Depth);
            Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Medium, shadowWrapper.WindowedPopupInsetMode);
            Assert.AreEqual(new Thickness(10, 2, 10, 18), shadowWrapper.PopupShadowPadding);
            Assert.AreEqual(secondaryItemsControl.CornerRadius, shadowWrapper.CornerRadius);
        });
    }

    [TestMethod]
    public void CommandBarOverflowPresenterFullWidthStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var presenter = new CommandBarOverflowPresenter
            {
                Content = new StackPanel()
            };

            using var host = new TestWindowHost(presenter, width: 320, height: 160);
            host.UpdateLayout();

            var layoutRoot = FindTemplateChild<Border>(presenter, "LayoutRoot");

            AssertStateSetter(layoutRoot, "DisplayModeStates", "FullWidthOpenDown", "LayoutRoot.Padding");
            AssertStateSetter(layoutRoot, "DisplayModeStates", "FullWidthOpenDown", "LayoutRoot.BorderThickness");
            AssertStateSetter(layoutRoot, "DisplayModeStates", "FullWidthOpenUp", "LayoutRoot.Padding");
            AssertStateSetter(layoutRoot, "DisplayModeStates", "FullWidthOpenUp", "LayoutRoot.BorderThickness");

            AssertVisualState(layoutRoot, "DisplayModeStates", "FullWidthOpenDown");
            Assert.AreEqual((Thickness)presenter.TryFindResource("CommandBarOverflowPresenterBorderDownPadding"), layoutRoot.Padding);
            Assert.AreEqual((Thickness)presenter.TryFindResource("CommandBarOverflowPresenterBorderDownThickness"), layoutRoot.BorderThickness);

            Assert.IsTrue(VisualStateManager.GoToState(presenter, "FullWidthOpenUp", false));

            Assert.AreEqual((Thickness)presenter.TryFindResource("CommandBarOverflowPresenterBorderUpPadding"), layoutRoot.Padding);
            Assert.AreEqual((Thickness)presenter.TryFindResource("CommandBarOverflowPresenterBorderUpThickness"), layoutRoot.BorderThickness);
        });
    }

    [TestMethod]
    public void CommandBarOverflowChromeConsumesLiveThemeResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var commandBar = new ModernWpf.Controls.CommandBar
            {
                Content = "Title",
                IsDynamicOverflowEnabled = false,
                OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Visible
            };
            commandBar.SecondaryCommands.Add(new AppBarButton { Label = "Share" });

            using var host = new TestWindowHost(commandBar, width: 360, height: 160);
            host.UpdateLayout();

            var commandBarStyle = FindImplicitStyle<ModernWpf.Controls.CommandBar>(commandBar);
            AssertDynamicResourceSetter(commandBarStyle, Control.BackgroundProperty, "CommandBarBackground");
            AssertDynamicResourceSetter(commandBarStyle, Control.ForegroundProperty, "CommandBarForeground");
            AssertDynamicResourceSetter(
                commandBarStyle,
                ModernWpf.Controls.CommandBar.CornerRadiusProperty,
                "ControlCornerRadius");

            var contentRoot = FindTemplateChild<Grid>(commandBar, "ContentRoot");
            var contentControl = FindTemplateChild<ContentControl>(commandBar, "ContentControl");
            var moreButton = FindTemplateChild<ToggleButton>(commandBar, "MoreButton");
            var overflowContentRoot = FindTemplateChild<Grid>(commandBar, "OverflowContentRoot");
            var highContrastBorder = FindTemplateChild<Rectangle>(commandBar, "HighContrastBorder");
            var openBorder = FindTemplateChild<Border>(commandBar, "OpenBorder");

            var replacementBackground = new SolidColorBrush(Colors.Magenta);
            var replacementForeground = new SolidColorBrush(Colors.Lime);
            var replacementHighContrastBorder = new SolidColorBrush(Colors.Cyan);
            var replacementOpenBorder = new SolidColorBrush(Colors.Yellow);
            var replacementOpenThickness = new Thickness(2, 3, 4, 5);

            commandBar.Resources["CommandBarBackground"] = replacementBackground;
            commandBar.Resources["CommandBarForeground"] = replacementForeground;
            commandBar.Resources["CommandBarOverflowMinWidth"] = 214d;
            commandBar.Resources["CommandBarOverflowMaxWidth"] = 333d;
            commandBar.Resources["CommandBarHighContrastBorder"] = replacementHighContrastBorder;
            commandBar.Resources["CommandBarBorderBrushOpen"] = replacementOpenBorder;
            commandBar.Resources["CommandBarBorderThicknessOpen"] = replacementOpenThickness;
            host.UpdateLayout();

            Assert.AreSame(replacementBackground, commandBar.Background);
            Assert.AreSame(replacementBackground, contentRoot.Background);
            Assert.AreSame(replacementForeground, commandBar.Foreground);
            Assert.AreSame(replacementForeground, contentControl.Foreground);
            Assert.AreSame(replacementForeground, moreButton.Foreground);
            Assert.AreEqual(214d, overflowContentRoot.MinWidth);
            Assert.AreEqual(333d, overflowContentRoot.MaxWidth);
            Assert.AreSame(replacementHighContrastBorder, highContrastBorder.Stroke);
            Assert.AreSame(replacementOpenBorder, openBorder.BorderBrush);
            Assert.AreEqual(replacementOpenThickness, openBorder.BorderThickness);
        });
    }

    [TestMethod]
    public void CommandBarMoreButtonIconDataCanBeOverriddenPerInstance()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var commandBar = new ModernWpf.Controls.CommandBar
            {
                OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Visible
            };

            using var host = new TestWindowHost(commandBar, width: 360, height: 160);

            var ellipsisIcon = FindTemplateChild<FontIconFallback>(commandBar, "EllipsisIcon");
            var defaultIconData = commandBar.TryFindResource("CommandBarMoreButtonIconData") as Geometry;

            Assert.IsNotNull(defaultIconData);
            Assert.AreSame(defaultIconData, ellipsisIcon.Data);

            var customIconData = Geometry.Parse("M 0,0 L 20,0 20,20 0,20 Z");
            commandBar.Resources["CommandBarMoreButtonIconData"] = customIconData;
            host.UpdateLayout();

            Assert.AreSame(customIconData, ellipsisIcon.Data);
        });
    }

    [TestMethod]
    public void CommandBarOverflowPresenterConsumesLiveThemeResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var presenter = new CommandBarOverflowPresenter
            {
                Content = new StackPanel()
            };

            using var host = new TestWindowHost(presenter, width: 320, height: 160);
            host.UpdateLayout();

            var presenterStyle = FindImplicitStyle<CommandBarOverflowPresenter>(presenter);
            AssertDynamicResourceSetter(presenterStyle, Control.BackgroundProperty, "CommandBarOverflowPresenterBackground");
            AssertDynamicResourceSetter(presenterStyle, Control.BorderBrushProperty, "CommandBarOverflowPresenterBorderBrush");
            AssertDynamicResourceSetter(presenterStyle, Control.PaddingProperty, "CommandBarOverflowPresenterBorderPadding");
            AssertDynamicResourceSetter(presenterStyle, FrameworkElement.MaxWidthProperty, "CommandBarOverflowMaxWidth");
            AssertDynamicResourceSetter(
                presenterStyle,
                CommandBarOverflowPresenter.CornerRadiusProperty,
                "OverlayCornerRadius");

            var layoutRoot = FindTemplateChild<Border>(presenter, "LayoutRoot");
            AssertStateSetterDynamicResource(
                layoutRoot,
                "DisplayModeStates",
                "FullWidthOpenDown",
                "LayoutRoot.Padding",
                "CommandBarOverflowPresenterBorderDownPadding");
            AssertStateSetterDynamicResource(
                layoutRoot,
                "DisplayModeStates",
                "FullWidthOpenDown",
                "LayoutRoot.BorderThickness",
                "CommandBarOverflowPresenterBorderDownThickness");
            AssertStateSetterDynamicResource(
                layoutRoot,
                "DisplayModeStates",
                "FullWidthOpenUp",
                "LayoutRoot.Padding",
                "CommandBarOverflowPresenterBorderUpPadding");
            AssertStateSetterDynamicResource(
                layoutRoot,
                "DisplayModeStates",
                "FullWidthOpenUp",
                "LayoutRoot.BorderThickness",
                "CommandBarOverflowPresenterBorderUpThickness");

            var replacementBackground = new SolidColorBrush(Colors.Magenta);
            var replacementBorderBrush = new SolidColorBrush(Colors.Cyan);
            var replacementPadding = new Thickness(4, 5, 6, 7);
            var replacementBorderThickness = new Thickness(2, 0, 2, 1);
            var replacementDownPadding = new Thickness(8, 9, 10, 11);
            var replacementDownThickness = new Thickness(0, 0, 0, 3);
            var replacementCornerRadius = new CornerRadius(9);

            presenter.Resources["CommandBarOverflowPresenterBackground"] = replacementBackground;
            presenter.Resources["CommandBarOverflowPresenterBorderBrush"] = replacementBorderBrush;
            presenter.Resources["CommandBarOverflowPresenterBorderPadding"] = replacementPadding;
            presenter.Resources["CommandBarOverflowPresenterBorderThickness"] = replacementBorderThickness;
            presenter.Resources["CommandBarOverflowPresenterBorderDownPadding"] = replacementDownPadding;
            presenter.Resources["CommandBarOverflowPresenterBorderDownThickness"] = replacementDownThickness;
            presenter.Resources["CommandBarOverflowMaxWidth"] = 312d;
            presenter.Resources["OverlayCornerRadius"] = replacementCornerRadius;
            host.UpdateLayout();

            Assert.AreSame(replacementBackground, presenter.Background);
            Assert.AreSame(replacementBackground, layoutRoot.Background);
            Assert.AreSame(replacementBorderBrush, presenter.BorderBrush);
            Assert.AreSame(replacementBorderBrush, layoutRoot.BorderBrush);
            Assert.AreEqual(replacementPadding, presenter.Padding);
            Assert.IsTrue(VisualStateManager.GoToState(presenter, "FullWidthOpenDown", false));
            Assert.AreEqual(replacementDownPadding, layoutRoot.Padding);
            Assert.AreEqual(replacementDownThickness, layoutRoot.BorderThickness);
            Assert.AreEqual(312d, presenter.MaxWidth);
            Assert.AreEqual(replacementCornerRadius, presenter.CornerRadius);
            Assert.AreEqual(replacementCornerRadius, layoutRoot.CornerRadius);
        });
    }

    [TestMethod]
    public void AppBarButtonDefaultsAndCommandTextMapping()
    {
        WpfTestHost.Run(() =>
        {
            var command = new RoutedUICommand("Open file", "OpenFile", typeof(CommandBarApiTests));
            command.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control, "Ctrl+O"));
            var icon = new FontIcon { Glyph = "\uE8E5" };
            var button = new AppBarButton
            {
                Command = command,
                Icon = icon,
                IsCompact = true,
                LabelPosition = CommandBarLabelPosition.Collapsed
            };

            Assert.AreEqual("Open file", button.Label);
            Assert.AreEqual("Ctrl+O", button.InputGestureText);
            Assert.AreEqual("Ctrl+O", button.KeyboardAcceleratorTextOverride);
            Assert.AreSame(icon, button.Icon);
            Assert.IsTrue(button.IsCompact);
            Assert.IsFalse(button.IsInOverflow);
            Assert.AreEqual(CommandBarLabelPosition.Collapsed, button.LabelPosition);
            Assert.IsNull(button.Flyout);
            Assert.IsNotNull(button.TemplateSettings);
        });
    }

    [TestMethod]
    public void AppBarButtonAutomationPeerMatchesWinUISourceShape()
    {
        WpfTestHost.Run(() =>
        {
            var button = new AppBarButton
            {
                Label = "Source label",
                Content = "Ignored content",
                KeyboardAcceleratorTextOverride = " Ctrl+A "
            };

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(button);

            Assert.IsInstanceOfType(peer, typeof(AppBarButtonAutomationPeer));
            Assert.AreEqual("AppBarButton", peer.GetClassName());
            Assert.AreEqual("app bar button", peer.GetLocalizedControlType());
            Assert.AreEqual("Source label", peer.GetName());
            Assert.AreEqual("Ctrl+A", peer.GetAcceleratorKey());
            Assert.IsNull(peer.GetChildren());
            Assert.IsNull(peer.GetPattern(PatternInterface.ExpandCollapse));

            AutomationProperties.SetName(button, "Explicit name");
            AutomationProperties.SetAcceleratorKey(button, "Alt+A");

            Assert.AreEqual("Explicit name", peer.GetName());
            Assert.AreEqual("Alt+A", peer.GetAcceleratorKey());
        });
    }

    [TestMethod]
    public void AppBarButtonFlyoutOpensFromClickAndAutomationLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var flyout = new Flyout
            {
                Content = new Border
                {
                    Width = 24,
                    Height = 24
                }
            };

            var button = new TestAppBarButton
            {
                Label = "Open",
                Flyout = flyout
            };

            using var host = new TestWindowHost(button, width: 180, height: 120);
            host.UpdateLayout();

            button.InvokeClick();
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreSame(button, flyout.Target);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(button);
            var provider = (IExpandCollapseProvider)peer.GetPattern(PatternInterface.ExpandCollapse);
            Assert.IsNotNull(provider);
            Assert.AreEqual(ExpandCollapseState.Expanded, provider.ExpandCollapseState);

            provider.Collapse();
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsFalse(flyout.IsOpen);
            Assert.AreEqual(ExpandCollapseState.Collapsed, provider.ExpandCollapseState);

            provider.Expand();
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreSame(button, flyout.Target);

            flyout.Hide();
        });
    }

    [TestMethod]
    public void AppBarButtonOverflowFlyoutUsesWinUISourceShowOptions()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var flyout = new Flyout
            {
                Content = new Border
                {
                    Width = 24,
                    Height = 24
                }
            };

            var button = new TestAppBarButton
            {
                Label = "More",
                Width = 80,
                Height = 32,
                Flyout = flyout
            };

            AppBarElementProperties.SetUseOverflowStyle(button, true);

            using var host = new TestWindowHost(button, width: 180, height: 120);
            host.UpdateLayout();

            Assert.IsTrue(button.IsInOverflow);
            Assert.AreEqual(80d, button.ActualWidth, 0.1);

            button.InvokeClick();
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreSame(button, flyout.Target);
            Assert.AreEqual(FlyoutPlacementMode.RightEdgeAlignedTop, flyout.GetEffectivePlacement());
            AssertOverflowFlyoutPlacementRectangle(button, flyout);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(button);
            var provider = (IExpandCollapseProvider)peer.GetPattern(PatternInterface.ExpandCollapse);
            Assert.IsNotNull(provider);

            provider.Collapse();
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            provider.Expand();
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.AreEqual(FlyoutPlacementMode.RightEdgeAlignedTop, flyout.GetEffectivePlacement());
            AssertOverflowFlyoutPlacementRectangle(button, flyout);

            flyout.Hide();
        });
    }

    [TestMethod]
    public void AppBarOverflowPointerEnterClosesPeerSubMenusLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var firstFlyout = new Flyout
            {
                Content = new Border
                {
                    Width = 24,
                    Height = 24
                }
            };
            var secondFlyout = new Flyout
            {
                Content = new Border
                {
                    Width = 24,
                    Height = 24
                }
            };
            var firstButton = new TestAppBarButton
            {
                Label = "First",
                Flyout = firstFlyout
            };
            var secondButton = new TestAppBarButton
            {
                Label = "Second",
                Flyout = secondFlyout
            };
            var toggleButton = new TestAppBarToggleButton
            {
                Label = "Toggle"
            };
            var commandBar = new ModernWpf.Controls.CommandBar();
            commandBar.SecondaryCommands.Add(firstButton);
            commandBar.SecondaryCommands.Add(secondButton);
            commandBar.SecondaryCommands.Add(toggleButton);

            using var host = new TestWindowHost(commandBar, width: 320, height: 160);
            host.UpdateLayout();

            commandBar.IsOpen = true;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(commandBar.IsOpen);

            AppBarElementProperties.SetUseOverflowStyle(firstButton, true);
            AppBarElementProperties.SetUseOverflowStyle(secondButton, true);
            AppBarElementProperties.SetUseOverflowStyle(toggleButton, true);

            firstButton.InvokeClick();
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(firstFlyout.IsOpen);

            secondButton.InvokeMouseEnter(CreateMouseEnterArgs(secondButton));
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsFalse(firstFlyout.IsOpen);

            secondButton.InvokeClick();
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(secondFlyout.IsOpen);

            secondButton.InvokeMouseEnter(CreateMouseEnterArgs(secondButton));
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(secondFlyout.IsOpen);

            toggleButton.InvokeMouseEnter(CreateMouseEnterArgs(toggleButton));
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsFalse(secondFlyout.IsOpen);
        });
    }

    [TestMethod]
    public void AppBarButtonClickClosesParentCommandBarUnlessItHasFlyoutLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var command = new RecordingCommand();
            var commandButton = new TestAppBarButton
            {
                Label = "Copy",
                Command = command
            };
            var flyout = new Flyout
            {
                Content = new Border
                {
                    Width = 24,
                    Height = 24
                }
            };
            var flyoutButton = new TestAppBarButton
            {
                Label = "More",
                Flyout = flyout
            };
            var commandBar = new ModernWpf.Controls.CommandBar();
            commandBar.SecondaryCommands.Add(commandButton);
            commandBar.SecondaryCommands.Add(flyoutButton);

            using var host = new TestWindowHost(commandBar, width: 320, height: 160);
            host.UpdateLayout();

            commandBar.IsOpen = true;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(commandBar.IsOpen);

            commandButton.InvokeClick();
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsFalse(commandBar.IsOpen);
            Assert.AreEqual(1, command.ExecuteCount);

            commandBar.IsOpen = true;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            flyoutButton.InvokeClick();
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(commandBar.IsOpen);
            Assert.IsTrue(flyout.IsOpen);
            Assert.AreSame(flyoutButton, flyout.Target);

            flyout.Hide();
        });
    }

    [TestMethod]
    public void AppBarToggleButtonClickClosesParentCommandBarLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var command = new RecordingCommand();
            var toggleButton = new TestAppBarToggleButton
            {
                Label = "Pin",
                Command = command
            };
            var commandBar = new ModernWpf.Controls.CommandBar();
            commandBar.SecondaryCommands.Add(toggleButton);

            using var host = new TestWindowHost(commandBar, width: 320, height: 160);
            host.UpdateLayout();

            commandBar.IsOpen = true;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(commandBar.IsOpen);

            toggleButton.InvokeClick();
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsFalse(commandBar.IsOpen);
            Assert.AreEqual(true, toggleButton.IsChecked);
            Assert.AreEqual(1, command.ExecuteCount);
        });
    }

    [TestMethod]
    public void CommandBarSecondaryCommandsUseTouchInputModeWhenOpenedByTouchLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var primaryButton = new AppBarButton { Label = "Primary" };
            var primaryToggleButton = new AppBarToggleButton { Label = "Primary toggle" };
            var secondaryButton = new AppBarButton { Label = "Secondary" };
            var secondaryToggleButton = new AppBarToggleButton { Label = "Secondary toggle" };
            var commandBar = new ModernWpf.Controls.CommandBar
            {
                IsDynamicOverflowEnabled = false
            };
            commandBar.PrimaryCommands.Add(primaryButton);
            commandBar.PrimaryCommands.Add(primaryToggleButton);
            commandBar.SecondaryCommands.Add(secondaryButton);
            commandBar.SecondaryCommands.Add(secondaryToggleButton);

            using var host = new TestWindowHost(commandBar, width: 320, height: 180);
            host.UpdateLayout();

            commandBar.SetLastInputModeForTesting(AppBarButtonInputMode.Touch);
            commandBar.IsOpen = true;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            var primaryButtonRoot = FindTemplateChild<Border>(primaryButton, "Root");
            var primaryToggleRoot = FindTemplateChild<Border>(primaryToggleButton, "Root");
            var secondaryButtonRoot = FindTemplateChild<Border>(secondaryButton, "Root");
            var secondaryToggleRoot = FindTemplateChild<Border>(secondaryToggleButton, "Root");

            AssertVisualState(primaryButtonRoot, "InputModeStates", "InputModeDefault");
            AssertVisualState(primaryToggleRoot, "InputModeStates", "InputModeDefault");
            AssertVisualState(secondaryButtonRoot, "InputModeStates", "TouchInputMode");
            AssertVisualState(secondaryToggleRoot, "InputModeStates", "TouchInputMode");

            commandBar.SecondaryCommands.Remove(secondaryToggleButton);
            commandBar.PrimaryCommands.Add(secondaryToggleButton);
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            secondaryButtonRoot = FindTemplateChild<Border>(secondaryButton, "Root");
            secondaryToggleRoot = FindTemplateChild<Border>(secondaryToggleButton, "Root");

            Assert.IsFalse(secondaryToggleButton.IsInOverflow);
            AssertVisualState(secondaryButtonRoot, "InputModeStates", "TouchInputMode");
            AssertVisualState(secondaryToggleRoot, "InputModeStates", "InputModeDefault");

            commandBar.PrimaryCommands.Remove(primaryButton);
            commandBar.PrimaryCommands.Remove(primaryToggleButton);
            commandBar.SecondaryCommands.Add(primaryButton);
            commandBar.SecondaryCommands.Add(primaryToggleButton);
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            primaryButtonRoot = FindTemplateChild<Border>(primaryButton, "Root");
            primaryToggleRoot = FindTemplateChild<Border>(primaryToggleButton, "Root");

            AssertVisualState(primaryButtonRoot, "InputModeStates", "TouchInputMode");
            AssertVisualState(primaryToggleRoot, "InputModeStates", "TouchInputMode");

            commandBar.IsOpen = false;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            primaryButtonRoot = FindTemplateChild<Border>(primaryButton, "Root");
            primaryToggleRoot = FindTemplateChild<Border>(primaryToggleButton, "Root");
            secondaryButtonRoot = FindTemplateChild<Border>(secondaryButton, "Root");

            AssertVisualState(primaryButtonRoot, "InputModeStates", "InputModeDefault");
            AssertVisualState(primaryToggleRoot, "InputModeStates", "InputModeDefault");
            AssertVisualState(secondaryButtonRoot, "InputModeStates", "InputModeDefault");
        });
    }

    [TestMethod]
    public void CommandBarAvailableCommandsStatesUseVisibleCommandsLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var primaryButton = new AppBarButton { Label = "Copy" };
            var secondaryButton = new AppBarToggleButton { Label = "Pin" };
            var secondarySeparator = new AppBarSeparator();
            var commandBar = new ModernWpf.Controls.CommandBar
            {
                IsDynamicOverflowEnabled = false
            };
            commandBar.PrimaryCommands.Add(primaryButton);
            commandBar.SecondaryCommands.Add(secondarySeparator);
            commandBar.SecondaryCommands.Add(secondaryButton);

            using var host = new TestWindowHost(commandBar, width: 320, height: 160);
            host.UpdateLayout();

            var layoutRoot = FindTemplateChild<Grid>(commandBar, "LayoutRoot");

            AssertVisualState(layoutRoot, "AvailableCommandsStates", "BothCommands");

            secondaryButton.Visibility = Visibility.Collapsed;
            host.UpdateLayout();

            AssertVisualState(layoutRoot, "AvailableCommandsStates", "BothCommands");

            secondarySeparator.Visibility = Visibility.Collapsed;
            host.UpdateLayout();

            AssertVisualState(layoutRoot, "AvailableCommandsStates", "PrimaryCommandsOnly");

            primaryButton.Visibility = Visibility.Collapsed;
            secondaryButton.Visibility = Visibility.Visible;
            host.UpdateLayout();

            AssertVisualState(layoutRoot, "AvailableCommandsStates", "SecondaryCommandsOnly");

            secondaryButton.Visibility = Visibility.Collapsed;
            host.UpdateLayout();

            AssertVisualState(layoutRoot, "AvailableCommandsStates", "PrimaryCommandsOnly");

            primaryButton.Visibility = Visibility.Visible;
            secondarySeparator.Visibility = Visibility.Visible;
            host.UpdateLayout();

            AssertVisualState(layoutRoot, "AvailableCommandsStates", "BothCommands");
        });
    }

    [TestMethod]
    public void AppBarToggleButtonAutomationPeerMatchesWinUISourceShape()
    {
        WpfTestHost.Run(() =>
        {
            var command = new RecordingCommand();
            var toggleButton = new AppBarToggleButton
            {
                Label = "Pin",
                Content = "Ignored content",
                KeyboardAcceleratorTextOverride = " Ctrl+P ",
                Command = command
            };

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(toggleButton);

            Assert.IsInstanceOfType(peer, typeof(AppBarToggleButtonAutomationPeer));
            Assert.AreEqual("AppBarToggleButton", peer.GetClassName());
            Assert.AreEqual("app bar toggle button", peer.GetLocalizedControlType());
            Assert.AreEqual("Pin", peer.GetName());
            Assert.AreEqual("Ctrl+P", peer.GetAcceleratorKey());
            Assert.IsNull(peer.GetChildren());

            AutomationProperties.SetName(toggleButton, "Explicit toggle");
            AutomationProperties.SetAcceleratorKey(toggleButton, "Alt+P");

            Assert.AreEqual("Explicit toggle", peer.GetName());
            Assert.AreEqual("Alt+P", peer.GetAcceleratorKey());

            var provider = (IToggleProvider)peer.GetPattern(PatternInterface.Toggle);
            Assert.IsNotNull(provider);
            Assert.AreEqual(ToggleState.Off, provider.ToggleState);

            provider.Toggle();

            Assert.AreEqual(true, toggleButton.IsChecked);
            Assert.AreEqual(ToggleState.On, provider.ToggleState);
            Assert.AreEqual(1, command.ExecuteCount);

            provider.Toggle();

            Assert.AreEqual(false, toggleButton.IsChecked);
            Assert.AreEqual(ToggleState.Off, provider.ToggleState);
            Assert.AreEqual(2, command.ExecuteCount);
        });
    }

    [TestMethod]
    public void NonTabStopAppBarButtonsInCommandBarRemainAutomationKeyboardFocusableLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var primaryButton = new AppBarButton
            {
                Label = "Primary",
                IsTabStop = false
            };

            var primaryToggleButton = new AppBarToggleButton
            {
                Label = "Primary toggle",
                IsTabStop = false
            };

            var secondaryButton = new AppBarButton
            {
                Label = "Secondary",
                IsTabStop = false
            };

            var secondaryToggleButton = new AppBarToggleButton
            {
                Label = "Secondary toggle",
                IsTabStop = false
            };

            var commandBar = new ModernWpf.Controls.CommandBar
            {
                IsOpen = true
            };
            commandBar.PrimaryCommands.Add(primaryButton);
            commandBar.PrimaryCommands.Add(primaryToggleButton);
            commandBar.SecondaryCommands.Add(secondaryButton);
            commandBar.SecondaryCommands.Add(secondaryToggleButton);

            using var host = new TestWindowHost(commandBar, width: 420, height: 160);
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(FrameworkElementAutomationPeer.CreatePeerForElement(primaryButton).IsKeyboardFocusable());
            Assert.IsTrue(FrameworkElementAutomationPeer.CreatePeerForElement(primaryToggleButton).IsKeyboardFocusable());
            Assert.IsTrue(FrameworkElementAutomationPeer.CreatePeerForElement(secondaryButton).IsKeyboardFocusable());
            Assert.IsTrue(FrameworkElementAutomationPeer.CreatePeerForElement(secondaryToggleButton).IsKeyboardFocusable());

            primaryButton.IsEnabled = false;
            primaryToggleButton.Visibility = Visibility.Collapsed;
            secondaryButton.IsEnabled = false;
            secondaryToggleButton.Visibility = Visibility.Collapsed;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsFalse(FrameworkElementAutomationPeer.FromElement(primaryButton).IsKeyboardFocusable());
            Assert.IsFalse(FrameworkElementAutomationPeer.FromElement(primaryToggleButton).IsKeyboardFocusable());
            Assert.IsFalse(FrameworkElementAutomationPeer.FromElement(secondaryButton).IsKeyboardFocusable());
            Assert.IsFalse(FrameworkElementAutomationPeer.FromElement(secondaryToggleButton).IsKeyboardFocusable());
        });
    }

    [TestMethod]
    public void AppBarButtonsDenyPointerFocusButRemainFocusable()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var focusTarget = new Button { Content = "Focus target" };
            var button = new TestAppBarButton { Label = "Button" };
            var toggleButton = new TestAppBarToggleButton { Label = "Toggle" };
            var commandBar = new ModernWpf.Controls.CommandBar();
            commandBar.PrimaryCommands.Add(button);
            commandBar.PrimaryCommands.Add(toggleButton);

            var root = new StackPanel();
            root.Children.Add(focusTarget);
            root.Children.Add(commandBar);

            using var host = new TestWindowHost(root, width: 400, height: 180);
            host.UpdateLayout();

            Assert.IsTrue(focusTarget.Focus());
            WpfTestHost.DoEvents();
            Assert.AreSame(focusTarget, Keyboard.FocusedElement);

            bool buttonGotFocus = false;
            bool toggleButtonGotFocus = false;
            button.GotKeyboardFocus += (_, _) => buttonGotFocus = true;
            toggleButton.GotKeyboardFocus += (_, _) => toggleButtonGotFocus = true;

            button.InvokeMouseLeftButtonDown(CreateMouseLeftButtonDownArgs(button));
            WpfTestHost.DoEvents();

            Assert.IsFalse(buttonGotFocus);
            Assert.AreSame(focusTarget, Keyboard.FocusedElement);

            toggleButton.InvokeMouseLeftButtonDown(CreateMouseLeftButtonDownArgs(toggleButton));
            WpfTestHost.DoEvents();

            Assert.IsFalse(toggleButtonGotFocus);
            Assert.AreSame(focusTarget, Keyboard.FocusedElement);

            Assert.IsTrue(button.Focus());
            WpfTestHost.DoEvents();
            Assert.AreSame(button, Keyboard.FocusedElement);

            Assert.IsTrue(toggleButton.Focus());
            WpfTestHost.DoEvents();
            Assert.AreSame(toggleButton, Keyboard.FocusedElement);
        });
    }

    [TestMethod]
    public void AppBarButtonTemplateUsesWinUIInnerChrome()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var icon = new SymbolIcon(Symbol.Accept);
            var button = new AppBarButton
            {
                Icon = icon,
                Label = "Accept",
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            using var host = new TestWindowHost(button, width: 160, height: 120);
            host.UpdateLayout();

            var innerBorder = FindTemplateChild<BorderEx>(button, "AppBarButtonInnerBorder");
            var content = FindTemplateChild<ContentPresenterEx>(button, "Content");

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, button.BackgroundSizing);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, innerBorder.BackgroundSizing);
            Assert.AreEqual(button.CornerRadius, innerBorder.CornerRadius);
            Assert.AreSame(icon, content.Content);
            Assert.AreEqual(button.Foreground, content.Foreground);
        });
    }

    [TestMethod]
    public void AppBarButtonAndToggleTemplatesRenderExplicitContentWithIconFallback()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var buttonContent = new Border { Width = 20, Height = 20 };
            var buttonIcon = new SymbolIcon(Symbol.Accept);
            var button = new AppBarButton
            {
                Content = buttonContent,
                Icon = buttonIcon,
                Label = "Content"
            };

            var toggleContent = new Border { Width = 20, Height = 20 };
            var toggleIcon = new SymbolIcon(Symbol.Pin);
            var toggleButton = new AppBarToggleButton
            {
                Content = toggleContent,
                Icon = toggleIcon,
                Label = "Content"
            };

            var iconOnlyButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Icon"
            };
            var iconOnlyToggleButton = new AppBarToggleButton
            {
                Icon = new SymbolIcon(Symbol.Pin),
                Label = "Icon"
            };

            var root = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { button, toggleButton, iconOnlyButton, iconOnlyToggleButton }
            };

            using var host = new TestWindowHost(root, width: 360, height: 140);
            host.UpdateLayout();

            Assert.AreSame(buttonContent, FindTemplateChild<ContentPresenterEx>(button, "Content").Content);
            Assert.AreSame(toggleContent, FindTemplateChild<ContentPresenterEx>(toggleButton, "Content").Content);
            Assert.AreSame(iconOnlyButton.Icon, FindTemplateChild<ContentPresenterEx>(iconOnlyButton, "Content").Content);
            Assert.AreSame(iconOnlyToggleButton.Icon, FindTemplateChild<ContentPresenterEx>(iconOnlyToggleButton, "Content").Content);
            Assert.IsTrue(buttonContent.RenderSize.Width > 0);
            Assert.IsTrue(toggleContent.RenderSize.Width > 0);
        });
    }

    [TestMethod]
    public void AppBarElementsConsumeLiveCoreThemeResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept",
                InputGestureText = "Ctrl+A",
                Flyout = new MenuFlyout()
            };
            var toggleButton = new AppBarToggleButton
            {
                Icon = new SymbolIcon(Symbol.Pin),
                Label = "Pin",
                InputGestureText = "Ctrl+P"
            };
            var separator = new AppBarSeparator();
            var root = new StackPanel
            {
                Children = { button, toggleButton, separator }
            };

            using var host = new TestWindowHost(root, width: 360, height: 180);
            host.UpdateLayout();

            var buttonStyle = FindImplicitStyle<AppBarButton>(button);
            AssertDynamicResourceSetter(buttonStyle, Control.BackgroundProperty, "AppBarButtonBackground");
            AssertDynamicResourceSetter(buttonStyle, Control.ForegroundProperty, "AppBarButtonForeground");
            AssertDynamicResourceSetter(buttonStyle, Control.BorderBrushProperty, "AppBarButtonBorderBrush");
            AssertDynamicResourceSetter(buttonStyle, AppBarButton.CornerRadiusProperty, "ControlCornerRadius");

            var buttonInnerBorder = FindTemplateChild<BorderEx>(button, "AppBarButtonInnerBorder");
            var buttonContent = FindTemplateChild<ContentPresenterEx>(button, "Content");
            var buttonTextLabel = FindTemplateChild<TextBlock>(button, "TextLabel");
            var buttonRoot = FindTemplateChild<Border>(button, "Root");

            AssertStateSetterDynamicResource(
                buttonRoot,
                "CommonStates",
                "PointerOver",
                "AppBarButtonInnerBorder.Background",
                "AppBarButtonBackgroundPointerOver");
            AssertStateSetterDynamicResource(
                buttonRoot,
                "CommonStates",
                "PointerOver",
                "Content.Foreground",
                "AppBarButtonForegroundPointerOver");
            AssertStateSetterDynamicResource(
                buttonRoot,
                "CommonStates",
                "PointerOver",
                "KeyboardAcceleratorTextLabel.Foreground",
                "AppBarButtonKeyboardAcceleratorTextForegroundPointerOver");
            AssertStateSetterDynamicResource(
                buttonRoot,
                "CommonStates",
                "PointerOver",
                "SubItemChevron.Foreground",
                "AppBarButtonSubItemChevronForegroundPointerOver");

            var buttonBackground = new SolidColorBrush(Colors.Magenta);
            var buttonForeground = new SolidColorBrush(Colors.Lime);
            var buttonBorderBrush = new SolidColorBrush(Colors.Cyan);
            var buttonCornerRadius = new CornerRadius(7);

            button.Resources["AppBarButtonBackground"] = buttonBackground;
            button.Resources["AppBarButtonForeground"] = buttonForeground;
            button.Resources["AppBarButtonBorderBrush"] = buttonBorderBrush;
            button.Resources["ControlCornerRadius"] = buttonCornerRadius;
            host.UpdateLayout();

            Assert.AreSame(buttonBackground, button.Background);
            Assert.AreSame(buttonBackground, buttonInnerBorder.Background);
            Assert.AreSame(buttonForeground, button.Foreground);
            Assert.AreSame(buttonForeground, buttonContent.Foreground);
            Assert.AreSame(buttonForeground, buttonTextLabel.Foreground);
            Assert.AreSame(buttonBorderBrush, button.BorderBrush);
            Assert.AreSame(buttonBorderBrush, buttonInnerBorder.BorderBrush);
            Assert.AreEqual(buttonCornerRadius, button.CornerRadius);
            Assert.AreEqual(buttonCornerRadius, buttonInnerBorder.CornerRadius);

            var toggleStyle = FindImplicitStyle<AppBarToggleButton>(toggleButton);
            AssertDynamicResourceSetter(toggleStyle, Control.BackgroundProperty, "AppBarToggleButtonBackground");
            AssertDynamicResourceSetter(toggleStyle, Control.ForegroundProperty, "AppBarToggleButtonForeground");
            AssertDynamicResourceSetter(toggleStyle, Control.BorderBrushProperty, "AppBarToggleButtonBorderBrush");
            AssertDynamicResourceSetter(toggleStyle, AppBarToggleButton.CornerRadiusProperty, "ControlCornerRadius");

            var toggleInnerBorder = FindTemplateChild<BorderEx>(toggleButton, "AppBarToggleButtonInnerBorder");
            var toggleContent = FindTemplateChild<ContentPresenterEx>(toggleButton, "Content");
            var toggleTextLabel = FindTemplateChild<TextBlock>(toggleButton, "TextLabel");
            var toggleRoot = FindTemplateChild<Border>(toggleButton, "Root");

            AssertStateSetterDynamicResource(
                toggleRoot,
                "CommonStates",
                "Checked",
                "AppBarToggleButtonInnerBorder.Background",
                "AppBarToggleButtonBackgroundChecked");
            AssertStateSetterDynamicResource(
                toggleRoot,
                "CommonStates",
                "Checked",
                "Content.Foreground",
                "AppBarToggleButtonForegroundChecked");
            AssertStateSetterDynamicResource(
                toggleRoot,
                "CommonStates",
                "Checked",
                "KeyboardAcceleratorTextLabel.Foreground",
                "AppBarToggleButtonKeyboardAcceleratorTextForegroundChecked");

            var toggleBackground = new SolidColorBrush(Colors.DarkCyan);
            var toggleForeground = new SolidColorBrush(Colors.DarkOrange);
            var toggleBorderBrush = new SolidColorBrush(Colors.DarkViolet);
            var toggleCornerRadius = new CornerRadius(5);

            toggleButton.Resources["AppBarToggleButtonBackground"] = toggleBackground;
            toggleButton.Resources["AppBarToggleButtonForeground"] = toggleForeground;
            toggleButton.Resources["AppBarToggleButtonBorderBrush"] = toggleBorderBrush;
            toggleButton.Resources["ControlCornerRadius"] = toggleCornerRadius;
            host.UpdateLayout();

            Assert.AreSame(toggleBackground, toggleButton.Background);
            Assert.AreSame(toggleBackground, toggleInnerBorder.Background);
            Assert.AreSame(toggleForeground, toggleButton.Foreground);
            Assert.AreSame(toggleForeground, toggleContent.Foreground);
            Assert.AreSame(toggleForeground, toggleTextLabel.Foreground);
            Assert.AreSame(toggleBorderBrush, toggleButton.BorderBrush);
            Assert.AreSame(toggleBorderBrush, toggleInnerBorder.BorderBrush);
            Assert.AreEqual(toggleCornerRadius, toggleButton.CornerRadius);
            Assert.AreEqual(toggleCornerRadius, toggleInnerBorder.CornerRadius);

            var separatorStyle = FindImplicitStyle<AppBarSeparator>(separator);
            AssertDynamicResourceSetter(separatorStyle, Control.ForegroundProperty, "AppBarSeparatorForeground");

            var separatorRectangle = FindTemplateChild<Rectangle>(separator, "SeparatorRectangle");
            var separatorForeground = new SolidColorBrush(Colors.Brown);
            separator.Resources["AppBarSeparatorForeground"] = separatorForeground;
            host.UpdateLayout();

            Assert.AreSame(separatorForeground, separator.Foreground);
            Assert.AreSame(separatorForeground, separatorRectangle.Fill);
        });
    }

    [TestMethod]
    public void AppBarButtonTemplateUsesVisualStateSettersForWinUIStateParity()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var flyout = new MenuFlyout();
            var button = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept",
                InputGestureText = "Ctrl+A",
                Flyout = flyout
            };

            using var host = new TestWindowHost(button, width: 180, height: 120);
            host.UpdateLayout();

            var root = FindTemplateChild<Border>(button, "Root");
            var subItemChevronPanel = FindTemplateChild<Grid>(button, "SubItemChevronPanel");
            var subItemChevron = FindTemplateChild<FontIconFallback>(button, "SubItemChevron");
            var overflowSubItemChevron = FindTemplateChild<FontIconFallback>(button, "OverflowSubItemChevron");
            var overflowTextLabel = FindTemplateChild<TextBlock>(button, "OverflowTextLabel");

            AssertStateSetter(root, "ApplicationViewStates", "Compact", "AppBarButtonInnerBorder.Margin");
            AssertStateSetter(root, "ApplicationViewStates", "LabelOnRight", "TextLabel.(Grid.Row)");
            AssertStateSetter(root, "ApplicationViewStates", "LabelOnRight", "SubItemChevron.Margin");
            AssertStateSetterAbsent(root, "ApplicationViewStates", "LabelOnRight", null, "Width");
            AssertStateSetter(root, "ApplicationViewStates", "Overflow", "OverflowTextLabel.Visibility");
            AssertStateSetter(root, "ApplicationViewStates", "OverflowWithMenuIcons", "ContentViewbox.Width");
            AssertStateSetter(root, "ApplicationViewStates", "OverflowWithToggleButtonsAndMenuIcons", "OverflowTextLabel.Margin");

            AssertStateSetter(root, "CommonStates", "PointerOver", "AppBarButtonInnerBorder.Background");
            AssertStateSetter(root, "CommonStates", "Pressed", "Content.Foreground");
            AssertStateSetter(root, "CommonStates", "Disabled", "KeyboardAcceleratorTextLabel.Foreground");
            AssertStateSetter(root, "CommonStates", "OverflowNormal", "SubItemChevron.Visibility");
            AssertStateSetter(root, "CommonStates", "OverflowNormal", "OverflowSubItemChevron.Visibility");
            AssertStateSetter(root, "CommonStates", "OverflowPointerOver", "SubItemChevron.Foreground");
            AssertStateSetter(root, "CommonStates", "OverflowPointerOver", "SubItemChevron.Visibility");
            AssertStateSetter(root, "CommonStates", "OverflowPointerOver", "OverflowSubItemChevron.Visibility");
            AssertStateSetter(root, "CommonStates", "OverflowPressed", "SubItemChevron.Foreground");
            AssertStateSetter(root, "CommonStates", "OverflowPressed", "SubItemChevron.Visibility");
            AssertStateSetter(root, "CommonStates", "OverflowPressed", "OverflowSubItemChevron.Visibility");
            AssertStateSetter(root, "CommonStates", "OverflowSubMenuOpened", "AppBarButtonInnerBorder.Background");
            AssertStateSetter(root, "CommonStates", "OverflowSubMenuOpened", "SubItemChevron.Foreground");
            AssertStateSetter(root, "CommonStates", "OverflowSubMenuOpened", "SubItemChevron.Visibility");
            AssertStateSetter(root, "CommonStates", "OverflowSubMenuOpened", "OverflowSubItemChevron.Visibility");

            AssertStateSetter(root, "InputModeStates", "TouchInputMode", "OverflowTextLabel.Padding");
            AssertStateSetter(root, "InputModeStates", "GameControllerInputMode", "OverflowTextLabel.Padding");
            Assert.IsTrue(VisualStateManager.GoToState(button, "TouchInputMode", false));
            Assert.AreEqual((Thickness)button.TryFindResource("AppBarButtonOverflowTextTouchMargin"), overflowTextLabel.Padding);
            Assert.IsTrue(VisualStateManager.GoToState(button, "InputModeDefault", false));
            Assert.AreEqual((Thickness)button.TryFindResource("AppBarButtonOverflowTextLabelPadding"), overflowTextLabel.Padding);

            AssertStateSetter(root, "KeyboardAcceleratorTextVisibility", "KeyboardAcceleratorTextVisible", "KeyboardAcceleratorTextLabel.Visibility");
            AssertStateSetter(root, "FlyoutStates", "HasFlyout", "SubItemChevronPanel.Visibility");
            AssertVisualState(root, "FlyoutStates", "HasFlyout");
            Assert.AreEqual((Visibility)button.TryFindResource("AppBarButtonHasFlyoutChevronVisibility"), subItemChevronPanel.Visibility);
            Assert.AreEqual(Visibility.Visible, subItemChevron.Visibility);
            Assert.AreEqual(Visibility.Collapsed, overflowSubItemChevron.Visibility);

            Assert.IsTrue(VisualStateManager.GoToState(button, "LabelOnRight", false));
            Assert.AreEqual((Thickness)button.TryFindResource("AppBarButtonSubItemChevronLabelOnRightMargin"), subItemChevron.Margin);

            AppBarElementProperties.SetUseOverflowStyle(button, true);
            button.SetOverflowStyleParams(hasIcons: true, hasToggleButtons: false, hasKeyboardAcceleratorText: true);
            Assert.IsTrue(button.IsInOverflow);
            AssertVisualState(root, "CommonStates", "OverflowNormal");
            AssertVisualState(root, "KeyboardAcceleratorTextVisibility", "KeyboardAcceleratorTextVisible");
            Assert.AreEqual(Visibility.Collapsed, subItemChevron.Visibility);
            Assert.AreEqual(Visibility.Visible, overflowSubItemChevron.Visibility);

            flyout.OnOpened();
            AssertVisualState(root, "CommonStates", "OverflowSubMenuOpened");
            Assert.AreEqual(Visibility.Collapsed, subItemChevron.Visibility);
            Assert.AreEqual(Visibility.Visible, overflowSubItemChevron.Visibility);

            flyout.OnClosed();
            AssertVisualState(root, "CommonStates", "OverflowNormal");
        });
    }

    [TestMethod]
    public void AppBarButtonsUseWinUIDefaultLabelPositionPropagation()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept"
            };
            var toggleButton = new AppBarToggleButton
            {
                Icon = new SymbolIcon(Symbol.Pin),
                Label = "Pin"
            };
            var commandBar = new ModernWpf.Controls.CommandBar
            {
                DefaultLabelPosition = CommandBarDefaultLabelPosition.Right,
                IsDynamicOverflowEnabled = false
            };
            commandBar.PrimaryCommands.Add(button);
            commandBar.PrimaryCommands.Add(toggleButton);

            using var host = new TestWindowHost(commandBar, width: 320, height: 120);
            host.UpdateLayout();

            var buttonRoot = FindTemplateChild<Border>(button, "Root");
            var toggleRoot = FindTemplateChild<Border>(toggleButton, "Root");

            AssertVisualState(buttonRoot, "ApplicationViewStates", "LabelOnRight");
            AssertVisualState(toggleRoot, "ApplicationViewStates", "LabelOnRight");

            commandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Bottom;
            host.UpdateLayout();

            AssertVisualState(buttonRoot, "ApplicationViewStates", "FullSize");
            AssertVisualState(toggleRoot, "ApplicationViewStates", "FullSize");

            button.LabelPosition = CommandBarLabelPosition.Collapsed;
            toggleButton.LabelPosition = CommandBarLabelPosition.Collapsed;
            host.UpdateLayout();

            AssertVisualState(buttonRoot, "ApplicationViewStates", "LabelCollapsed");
            AssertVisualState(toggleRoot, "ApplicationViewStates", "LabelCollapsed");
        });
    }

    [TestMethod]
    public void AppBarLabelOnRightWidthAdjustmentRespectsLocalWidthLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var autoButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept"
            };
            var fixedButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "Add",
                Width = 120
            };
            var autoToggleButton = new AppBarToggleButton
            {
                Icon = new SymbolIcon(Symbol.Pin),
                Label = "Pin"
            };
            var fixedToggleButton = new AppBarToggleButton
            {
                Icon = new SymbolIcon(Symbol.Setting),
                Label = "Settings",
                Width = 132
            };
            var commandBar = new ModernWpf.Controls.CommandBar
            {
                DefaultLabelPosition = CommandBarDefaultLabelPosition.Right,
                IsDynamicOverflowEnabled = false
            };
            commandBar.PrimaryCommands.Add(autoButton);
            commandBar.PrimaryCommands.Add(fixedButton);
            commandBar.PrimaryCommands.Add(autoToggleButton);
            commandBar.PrimaryCommands.Add(fixedToggleButton);

            using var host = new TestWindowHost(commandBar, width: 640, height: 120);
            host.UpdateLayout();

            Assert.IsTrue(double.IsNaN(autoButton.Width));
            Assert.AreEqual(120d, fixedButton.Width);
            Assert.IsTrue(double.IsNaN(autoToggleButton.Width));
            Assert.AreEqual(132d, fixedToggleButton.Width);

            commandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Bottom;
            host.UpdateLayout();

            Assert.AreEqual(68d, autoButton.Width);
            Assert.AreEqual(120d, fixedButton.Width);
            Assert.AreEqual(68d, autoToggleButton.Width);
            Assert.AreEqual(132d, fixedToggleButton.Width);
        });
    }

    [TestMethod]
    public void CommandBarAutoOverflowButtonUsesBottomLabelQueries()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept"
            };
            var commandBar = new ModernWpf.Controls.CommandBar
            {
                DefaultLabelPosition = CommandBarDefaultLabelPosition.Bottom,
                IsDynamicOverflowEnabled = false
            };
            commandBar.PrimaryCommands.Add(button);

            using var host = new TestWindowHost(commandBar, width: 240, height: 120);
            host.UpdateLayout();

            var moreButton = FindTemplateChild<ToggleButton>(commandBar, "MoreButton");

            Assert.AreEqual(Visibility.Visible, commandBar.CommandBarTemplateSettings.EffectiveOverflowButtonVisibility);
            Assert.AreEqual(Visibility.Visible, moreButton.Visibility);

            button.LabelPosition = CommandBarLabelPosition.Collapsed;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, commandBar.CommandBarTemplateSettings.EffectiveOverflowButtonVisibility);
            Assert.AreEqual(Visibility.Collapsed, moreButton.Visibility);
        });
    }

    [TestMethod]
    public void CommandBarAutoOverflowButtonUsesPhysicalPixelCompactHeightThreshold()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var commandBar = new ModernWpf.Controls.CommandBar
            {
                IsDynamicOverflowEnabled = false,
                UseLayoutRounding = false
            };

            using var host = new TestWindowHost(commandBar, width: 240, height: 120);
            host.UpdateLayout();

            var compactHeight = (double)commandBar.TryFindResource("AppBarThemeCompactHeight");
            var rasterizationScale = VisualTreeHelper.GetDpi(commandBar).DpiScaleY;
            var halfPhysicalPixel = 0.5 / rasterizationScale;

            commandBar.Height = compactHeight + halfPhysicalPixel * 0.75;
            host.UpdateLayout();

            Assert.AreEqual(
                Visibility.Collapsed,
                commandBar.CommandBarTemplateSettings.EffectiveOverflowButtonVisibility);

            commandBar.Height = compactHeight + halfPhysicalPixel * 1.25;
            host.UpdateLayout();

            Assert.AreEqual(
                Visibility.Visible,
                commandBar.CommandBarTemplateSettings.EffectiveOverflowButtonVisibility);
        });
    }

    [TestMethod]
    public void CommandBarCompactHeightThresholdUsesFractionalRasterizationScale()
    {
        const double rasterizationScale = 1.25;
        double halfPhysicalPixel = 0.5 / rasterizationScale;

        Assert.IsFalse(ModernWpf.Controls.CommandBar.IsCompactHeightDifferenceSignificant(
            halfPhysicalPixel * 0.75,
            rasterizationScale));
        Assert.IsTrue(ModernWpf.Controls.CommandBar.IsCompactHeightDifferenceSignificant(
            halfPhysicalPixel,
            rasterizationScale));
        Assert.IsTrue(ModernWpf.Controls.CommandBar.IsCompactHeightDifferenceSignificant(
            -halfPhysicalPixel,
            rasterizationScale));
    }

    [TestMethod]
    public void CommandBarAutoOverflowButtonTreatsEmptyAppBarLabelsAsPresent()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new AppBarButton { Icon = new SymbolIcon(Symbol.Accept) };
            VerifyEmptyLabelPrimaryCommandShowsAutoOverflowButton(button);

            var toggleButton = new AppBarToggleButton { Icon = new SymbolIcon(Symbol.Accept) };
            VerifyEmptyLabelPrimaryCommandShowsAutoOverflowButton(toggleButton);
        });
    }

    [TestMethod]
    public void AppBarToggleButtonDefaultsAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var icon = new SymbolIcon(Symbol.Accept);
            var toggleButton = new AppBarToggleButton
            {
                Icon = icon,
                Label = "Pin",
                IsChecked = true,
                IsCompact = true,
                LabelPosition = CommandBarLabelPosition.Collapsed
            };

            Assert.AreSame(icon, toggleButton.Icon);
            Assert.AreEqual("Pin", toggleButton.Label);
            Assert.AreEqual(true, toggleButton.IsChecked);
            Assert.IsTrue(toggleButton.IsCompact);
            Assert.IsFalse(toggleButton.IsInOverflow);
            Assert.AreEqual(CommandBarLabelPosition.Collapsed, toggleButton.LabelPosition);
            Assert.IsNotNull(toggleButton.TemplateSettings);
        });
    }

    [TestMethod]
    public void AppBarOverflowPanelAppliesWinUISourceOverflowParams()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Open",
                KeyboardAcceleratorTextOverride = "Ctrl+O",
                Flyout = new MenuFlyout()
            };
            var toggleButton = new AppBarToggleButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Pin",
                KeyboardAcceleratorTextOverride = "Ctrl+Shift+P",
                IsChecked = true
            };
            var panel = new CommandBarFlyoutOverflowPanel();
            panel.Children.Add(button);
            panel.Children.Add(toggleButton);

            using var host = new TestWindowHost(panel, width: 320, height: 160);
            host.UpdateLayout();

            var buttonRoot = FindTemplateChild<Border>(button, "Root");
            var toggleRoot = FindTemplateChild<Border>(toggleButton, "Root");
            var buttonKeyboardText = FindTemplateChild<TextBlock>(button, "KeyboardAcceleratorTextLabel");
            var toggleKeyboardText = FindTemplateChild<TextBlock>(toggleButton, "KeyboardAcceleratorTextLabel");

            Assert.IsTrue(button.IsInOverflow);
            Assert.IsTrue(toggleButton.IsInOverflow);
            AssertVisualState(buttonRoot, "ApplicationViewStates", "OverflowWithToggleButtonsAndMenuIcons");
            AssertVisualState(toggleRoot, "ApplicationViewStates", "OverflowWithMenuIcons");
            AssertVisualState(buttonRoot, "KeyboardAcceleratorTextVisibility", "KeyboardAcceleratorTextVisible");
            AssertVisualState(toggleRoot, "KeyboardAcceleratorTextVisibility", "KeyboardAcceleratorTextVisible");
            AssertVisualState(toggleRoot, "CommonStates", "OverflowChecked");

            Assert.AreEqual("Ctrl+O", buttonKeyboardText.Text);
            Assert.AreEqual("Ctrl+Shift+P", toggleKeyboardText.Text);
            Assert.IsTrue(button.TemplateSettings.KeyboardAcceleratorTextMinWidth > 0);
            Assert.AreEqual(
                button.TemplateSettings.KeyboardAcceleratorTextMinWidth,
                toggleButton.TemplateSettings.KeyboardAcceleratorTextMinWidth);
        });
    }

    [TestMethod]
    public void AppBarOverflowSizingUsesWinUISourceOverflowStyles()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new AppBarButton();
            var toggleButton = new AppBarToggleButton();
            var root = new StackPanel();
            root.Children.Add(button);
            root.Children.Add(toggleButton);

            using var host = new TestWindowHost(root, width: 240, height: 160);
            host.UpdateLayout();

            Assert.AreEqual(68d, button.Width);
            Assert.AreEqual(HorizontalAlignment.Left, button.HorizontalAlignment);
            Assert.IsFalse(button.IsInOverflow);

            Assert.AreEqual(68d, toggleButton.Width);
            Assert.AreEqual(HorizontalAlignment.Left, toggleButton.HorizontalAlignment);
            Assert.IsFalse(toggleButton.IsInOverflow);

            AssertStyleDoesNotTriggerOnToolBarIsOverflowItem(FindImplicitStyle<AppBarButton>(button));
            AssertStyleDoesNotTriggerOnToolBarIsOverflowItem(FindImplicitStyle<AppBarToggleButton>(toggleButton));

            var resources = CreateCommandBarResources();
            AssertOverflowStyleMatchesWinUISource(
                (Style)resources["AppBarButtonOverflowStyle"],
                typeof(AppBarButton));
            AssertOverflowStyleMatchesWinUISource(
                (Style)resources["AppBarToggleButtonOverflowStyle"],
                typeof(AppBarToggleButton));
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2CommandBarHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceValue(themeName, "CommandBarOverflowMinWidth", 160d);
                AssertThemeResourceValue(themeName, "CommandBarOverflowTouchMinWidth", 240d);
                AssertThemeResourceValue(themeName, "CommandBarOverflowMaxWidth", 480d);
                AssertThemeResourceValue(themeName, "CommandBarOverflowMaxHeight", 198d);
                AssertThemeResourceReference(themeName, "CommandBarBackground", "ControlFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "CommandBarBackgroundOpen", "AcrylicInAppFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "CommandBarBorderBrushOpen", "CardStrokeColorDefaultSolidBrush");
                AssertThemeResourceReference(themeName, "CommandBarForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "CommandBarHighContrastBorder", "ControlFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "CommandBarEllipsisIconForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "CommandBarOverflowPresenterBackground", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "CommandBarOverflowPresenterBorderBrush", "SystemControlTransientBorderBrush");
                AssertThemeResourceReference(themeName, "CommandBarLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");
                AssertThemeResourceValue(themeName, "CommandBarBorderThicknessOpen", new Thickness(1));

                AssertCommonAppBarMetrics(themeName);
                AssertThemeResourceReferences(themeName,
                    ("AppBarButtonBackground", "SubtleFillColorTransparentBrush"),
                    ("AppBarButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush"),
                    ("AppBarButtonBackgroundPressed", "SubtleFillColorTertiaryBrush"),
                    ("AppBarButtonBackgroundDisabled", "SubtleFillColorDisabledBrush"),
                    ("AppBarButtonForeground", "TextFillColorPrimaryBrush"),
                    ("AppBarButtonForegroundPressed", "TextFillColorSecondaryBrush"),
                    ("AppBarButtonBorderBrush", "ControlFillColorTransparentBrush"),
                    ("AppBarButtonKeyboardAcceleratorTextForeground", "TextFillColorSecondaryBrush"),
                    ("AppBarButtonKeyboardAcceleratorTextForegroundPressed", "TextFillColorTertiaryBrush"),
                    ("AppBarButtonBackgroundSubMenuOpened", "SubtleFillColorSecondaryBrush"),
                    ("AppBarButtonSubItemChevronForegroundDisabled", "TextFillColorDisabledBrush"),
                    ("AppBarToggleButtonBackground", "SubtleFillColorTransparentBrush"),
                    ("AppBarToggleButtonBackgroundChecked", "AccentFillColorDefaultBrush"),
                    ("AppBarToggleButtonBackgroundCheckedPointerOver", "AccentFillColorSecondaryBrush"),
                    ("AppBarToggleButtonBackgroundHighLightOverlayPointerOver", "SubtleFillColorSecondaryBrush"),
                    ("AppBarToggleButtonForeground", "TextFillColorPrimaryBrush"),
                    ("AppBarToggleButtonForegroundChecked", "TextOnAccentFillColorPrimaryBrush"),
                    ("AppBarToggleButtonForegroundCheckedPressed", "TextOnAccentFillColorSecondaryBrush"),
                    ("AppBarToggleButtonForegroundCheckedDisabled", "TextOnAccentAAFillColorDisabled"),
                    ("AppBarToggleButtonBorderBrushChecked", "AccentControlElevationBorderBrush"),
                    ("AppBarToggleButtonCheckGlyphForegroundChecked", "TextOnAccentFillColorPrimaryBrush"),
                    ("AppBarToggleButtonOverflowLabelForegroundCheckedPointerOver", "TextFillColorPrimaryBrush"),
                    ("AppBarToggleButtonKeyboardAcceleratorTextForegroundCheckedPressed", "TextFillColorTertiaryBrush"),
                    ("AppBarSeparatorForeground", "DividerStrokeColorDefaultBrush"));
            }

            AssertThemeResourceReference("HighContrast", "CommandBarBackground", "SystemControlBackgroundChromeMediumBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarBackgroundOpen", "SystemControlBackgroundChromeMediumBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarBorderBrushOpen", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarForeground", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarHighContrastBorder", "SystemControlForegroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarEllipsisIconForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarOverflowPresenterBackground", "SystemControlBackgroundChromeMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarOverflowPresenterBorderBrush", "SystemControlTransientBorderBrush");
            AssertThemeResourceReference("HighContrast", "CommandBarLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");
            AssertThemeResourceValue("HighContrast", "CommandBarBorderThicknessOpen", new Thickness(0));
            AssertThemeResourceValue("HighContrast", "CommandBarOverflowPresenterBorderThickness", new Thickness(1));
            AssertCommonAppBarMetrics("HighContrast");
            AssertThemeResourceReferences("HighContrast",
                ("AppBarButtonBackground", "SystemControlTransparentBrush"),
                ("AppBarButtonBackgroundPointerOver", "SystemControlHighlightListLowBrush"),
                ("AppBarButtonBackgroundPressed", "SystemControlHighlightListMediumBrush"),
                ("AppBarButtonBackgroundDisabled", "SystemControlTransparentBrush"),
                ("AppBarButtonForeground", "SystemControlForegroundBaseHighBrush"),
                ("AppBarButtonForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarButtonForegroundPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarButtonForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("AppBarButtonBorderBrush", "SystemControlForegroundTransparentBrush"),
                ("AppBarButtonBorderBrushPointerOver", "SystemControlHighlightTransparentBrush"),
                ("AppBarButtonBorderBrushPressed", "SystemControlHighlightTransparentBrush"),
                ("AppBarButtonBorderBrushDisabled", "SystemControlDisabledTransparentBrush"),
                ("AppBarButtonKeyboardAcceleratorTextForeground", "SystemControlForegroundBaseMediumBrush"),
                ("AppBarButtonKeyboardAcceleratorTextForegroundPointerOver", "SystemControlHighlightAltBaseMediumBrush"),
                ("AppBarButtonKeyboardAcceleratorTextForegroundPressed", "SystemControlHighlightAltBaseMediumBrush"),
                ("AppBarButtonKeyboardAcceleratorTextForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("AppBarButtonBackgroundSubMenuOpened", "SystemControlHighlightListAccentLowBrush"),
                ("AppBarButtonForegroundSubMenuOpened", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarButtonKeyboardAcceleratorTextForegroundSubMenuOpened", "SystemControlHighlightAltBaseMediumBrush"),
                ("AppBarButtonBorderBrushSubMenuOpened", "SystemControlTransparentBrush"),
                ("AppBarButtonSubItemChevronForeground", "SystemControlForegroundBaseMediumHighBrush"),
                ("AppBarButtonSubItemChevronForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarButtonSubItemChevronForegroundPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarButtonSubItemChevronForegroundSubMenuOpened", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarButtonSubItemChevronForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("AppBarToggleButtonBackground", "SystemControlTransparentBrush"),
                ("AppBarToggleButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush"),
                ("AppBarToggleButtonBackgroundPressed", "SubtleFillColorTertiaryBrush"),
                ("AppBarToggleButtonBackgroundDisabled", "SystemControlTransparentBrush"),
                ("AppBarToggleButtonBackgroundChecked", "SystemControlHighlightListAccentLowBrush"),
                ("AppBarToggleButtonBackgroundCheckedPointerOver", "SystemControlHighlightListAccentMediumBrush"),
                ("AppBarToggleButtonBackgroundCheckedPressed", "SystemControlHighlightListAccentHighBrush"),
                ("AppBarToggleButtonBackgroundCheckedDisabled", "SystemControlDisabledAccentBrush"),
                ("AppBarToggleButtonBackgroundHighLightOverlay", "SystemControlTransparentBrush"),
                ("AppBarToggleButtonBackgroundHighLightOverlayPointerOver", "SystemControlHighlightListLowBrush"),
                ("AppBarToggleButtonBackgroundHighLightOverlayPressed", "SystemControlHighlightListMediumBrush"),
                ("AppBarToggleButtonBackgroundHighLightOverlayCheckedPointerOver", "SystemControlHighlightListLowBrush"),
                ("AppBarToggleButtonBackgroundHighLightOverlayCheckedPressed", "SystemControlHighlightListMediumBrush"),
                ("AppBarToggleButtonForeground", "SystemControlForegroundBaseHighBrush"),
                ("AppBarToggleButtonForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonForegroundPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("AppBarToggleButtonForegroundChecked", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonForegroundCheckedPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonForegroundCheckedPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonForegroundCheckedDisabled", "SystemControlBackgroundBaseMediumLowBrush"),
                ("AppBarToggleButtonBorderBrush", "SystemControlForegroundTransparentBrush"),
                ("AppBarToggleButtonBorderBrushPointerOver", "SystemControlHighlightTransparentBrush"),
                ("AppBarToggleButtonBorderBrushPressed", "SystemControlHighlightTransparentBrush"),
                ("AppBarToggleButtonBorderBrushDisabled", "SystemControlDisabledTransparentBrush"),
                ("AppBarToggleButtonBorderBrushChecked", "SystemControlHighlightTransparentBrush"),
                ("AppBarToggleButtonBorderBrushCheckedPointerOver", "SystemControlHighlightTransparentBrush"),
                ("AppBarToggleButtonBorderBrushCheckedPressed", "SystemControlHighlightTransparentBrush"),
                ("AppBarToggleButtonBorderBrushCheckedDisabled", "SystemControlHighlightTransparentBrush"),
                ("AppBarToggleButtonCheckGlyphForeground", "SystemControlForegroundBaseHighBrush"),
                ("AppBarToggleButtonCheckGlyphForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonCheckGlyphForegroundPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonCheckGlyphForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("AppBarToggleButtonCheckGlyphForegroundChecked", "SystemControlForegroundBaseHighBrush"),
                ("AppBarToggleButtonCheckGlyphForegroundCheckedPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonCheckGlyphForegroundCheckedPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonCheckGlyphForegroundCheckedDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("AppBarToggleButtonOverflowLabelForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonOverflowLabelForegroundPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonOverflowLabelForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("AppBarToggleButtonOverflowLabelForegroundCheckedPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonOverflowLabelForegroundCheckedPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("AppBarToggleButtonOverflowLabelForegroundCheckedDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("AppBarToggleButtonKeyboardAcceleratorTextForeground", "SystemControlForegroundBaseMediumBrush"),
                ("AppBarToggleButtonKeyboardAcceleratorTextForegroundPointerOver", "SystemControlHighlightAltBaseMediumBrush"),
                ("AppBarToggleButtonKeyboardAcceleratorTextForegroundPressed", "SystemControlHighlightAltBaseMediumBrush"),
                ("AppBarToggleButtonKeyboardAcceleratorTextForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("AppBarToggleButtonKeyboardAcceleratorTextForegroundChecked", "SystemControlForegroundBaseMediumBrush"),
                ("AppBarToggleButtonKeyboardAcceleratorTextForegroundCheckedPointerOver", "SystemControlHighlightAltBaseMediumBrush"),
                ("AppBarToggleButtonKeyboardAcceleratorTextForegroundCheckedPressed", "SystemControlHighlightAltBaseMediumBrush"),
                ("AppBarToggleButtonKeyboardAcceleratorTextForegroundCheckedDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("AppBarSeparatorForeground", "SystemControlForegroundBaseMediumLowBrush"));
        });
    }

    [TestMethod]
    public void AppBarToggleButtonTemplateUsesWinUIInnerChrome()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var icon = new SymbolIcon(Symbol.Accept);
            var toggleButton = new AppBarToggleButton
            {
                Icon = icon,
                Label = "Pin",
                IsChecked = true
            };

            using var host = new TestWindowHost(toggleButton, width: 160, height: 120);
            host.UpdateLayout();

            var innerBorder = FindTemplateChild<BorderEx>(toggleButton, "AppBarToggleButtonInnerBorder");
            var content = FindTemplateChild<ContentPresenterEx>(toggleButton, "Content");

            Assert.AreEqual(BackgroundSizing.InnerBorderEdge, toggleButton.BackgroundSizing);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, innerBorder.BackgroundSizing);
            Assert.AreEqual(toggleButton.CornerRadius, innerBorder.CornerRadius);
            Assert.AreSame(icon, content.Content);
        });
    }

    [TestMethod]
    public void AppBarToggleButtonTemplateUsesVisualStateSettersForWinUIStateParity()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleButton = new AppBarToggleButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Pin",
                InputGestureText = "Ctrl+P",
                IsChecked = true
            };

            using var host = new TestWindowHost(toggleButton, width: 180, height: 120);
            host.UpdateLayout();

            var root = FindTemplateChild<Border>(toggleButton, "Root");
            var overflowCheckGlyph = FindTemplateChild<FontIconFallback>(toggleButton, "OverflowCheckGlyph");
            var overflowTextLabel = FindTemplateChild<TextBlock>(toggleButton, "OverflowTextLabel");

            AssertStateSetter(root, "ApplicationViewStates", "Compact", "AppBarToggleButtonInnerBorder.Margin");
            AssertStateSetter(root, "ApplicationViewStates", "LabelOnRight", "TextLabel.(Grid.Column)");
            AssertStateSetterAbsent(root, "ApplicationViewStates", "LabelOnRight", null, "Width");
            AssertStateSetter(root, "ApplicationViewStates", "Overflow", "OverflowCheckGlyph.Visibility");
            AssertStateSetter(root, "ApplicationViewStates", "OverflowWithMenuIcons", "ContentViewbox.MaxWidth");

            AssertStateSetter(root, "CommonStates", "PointerOver", "AppBarToggleButtonInnerBorder.Background");
            AssertStateSetter(root, "CommonStates", "Checked", "AppBarToggleButtonInnerBorder.BackgroundSizing");
            AssertStateSetter(root, "CommonStates", "CheckedPointerOver", "OverflowCheckGlyph.Opacity");
            AssertStateSetter(root, "CommonStates", "CheckedDisabled", "KeyboardAcceleratorTextLabel.Foreground");
            AssertStateSetter(root, "CommonStates", "OverflowPointerOver", "OverflowCheckGlyph.Foreground");
            AssertStateSetter(root, "CommonStates", "OverflowCheckedPointerOver", "AppBarToggleButtonInnerBorder.Background");
            AssertStateSetter(root, "CommonStates", "OverflowCheckedPressed", "Content.Foreground");

            AssertStateSetter(root, "InputModeStates", "TouchInputMode", "OverflowTextLabel.Padding");
            AssertStateSetter(root, "InputModeStates", "TouchInputMode", "OverflowCheckGlyph.Margin");
            AssertStateSetter(root, "InputModeStates", "GameControllerInputMode", "OverflowTextLabel.Padding");
            AssertStateSetter(root, "InputModeStates", "GameControllerInputMode", "OverflowCheckGlyph.Margin");
            Assert.IsTrue(VisualStateManager.GoToState(toggleButton, "GameControllerInputMode", false));
            Assert.AreEqual((Thickness)toggleButton.TryFindResource("AppBarToggleButtonOverflowTextTouchMargin"), overflowTextLabel.Padding);
            Assert.AreEqual((Thickness)toggleButton.TryFindResource("AppBarToggleButtonOverflowCheckTouchMargin"), overflowCheckGlyph.Margin);
            Assert.IsTrue(VisualStateManager.GoToState(toggleButton, "InputModeDefault", false));
            Assert.AreEqual((Thickness)toggleButton.TryFindResource("AppBarToggleButtonOverflowTextLabelPadding"), overflowTextLabel.Padding);
            Assert.AreEqual((Thickness)toggleButton.TryFindResource("AppBarToggleButtonOverflowCheckMargin"), overflowCheckGlyph.Margin);

            AssertStateSetter(root, "KeyboardAcceleratorTextVisibility", "KeyboardAcceleratorTextVisible", "KeyboardAcceleratorTextLabel.Visibility");
        });
    }

    [TestMethod]
    public void AppBarElementContainerTemplateUsesWinUIContentPresenter()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var container = new AppBarElementContainer
            {
                Content = "Custom content",
                ContentTransitions = transitions,
                Padding = new Thickness(3, 4, 5, 6),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Bottom
            };

            using var host = new TestWindowHost(container, width: 180, height: 120);
            host.UpdateLayout();

            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(container)
                ?? throw new AssertFailedException("Expected AppBarElementContainer template to use ContentPresenterEx.");

            Assert.IsFalse(container.IsInOverflow);
            Assert.AreEqual("Custom content", presenter.Content);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreEqual(container.Padding, presenter.Margin);
            Assert.AreEqual(HorizontalAlignment.Center, presenter.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalAlignment);

            AppBarElementProperties.SetUseOverflowStyle(container, true);
            Assert.IsTrue(container.IsInOverflow);

            AppBarElementProperties.SetUseOverflowStyle(container, false);
            Assert.IsFalse(container.IsInOverflow);
        });
    }

    [TestMethod]
    public void SplitButtonCommandBarStyleUsesWinUIPrimaryPresenter()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = CreateCommandBarResources();
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var splitButton = new ModernWpf.Controls.SplitButton
            {
                Style = (Style)resources["SplitButtonCommandBarStyle"],
                Content = "Accept",
                ContentTransitions = transitions
            };

            var root = CreateTemplateHost(splitButton, resources);
            using var host = new TestWindowHost(root, width: 180, height: 120);
            host.UpdateLayout();

            var primaryButton = FindTemplateChild<Button>(splitButton, "PrimaryButton");
            var rootGrid = FindTemplateChild<Border>(splitButton, "RootGrid");
            var primaryBackgroundGrid = FindTemplateChild<Border>(splitButton, "PrimaryBackgroundGrid");
            var secondaryBackgroundGrid = FindTemplateChild<Border>(splitButton, "SecondaryBackgroundGrid");
            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(primaryButton)
                ?? throw new AssertFailedException("Expected SplitButtonCommandBarStyle primary button to use ContentPresenterEx.");

            Assert.AreEqual("Accept", presenter.Content);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreEqual(splitButton.Foreground, presenter.Foreground);
            Assert.AreEqual(0, splitButton.Template.Triggers.Count);

            AssertStateSetter(rootGrid, "CommonStates", "Disabled", "Border.BorderBrush");
            AssertStateSetter(rootGrid, "CommonStates", "FlyoutOpen", "PrimaryBackgroundGrid.Background");
            AssertStateSetter(rootGrid, "CommonStates", "FlyoutOpen", "Border.BorderBrush");
            AssertStateSetter(rootGrid, "CommonStates", "TouchPressed", "SecondaryButton.Foreground");
            AssertStateSetter(rootGrid, "CommonStates", "PrimaryPointerOver", "SecondaryBackgroundGrid.Background");
            AssertStateSetter(rootGrid, "CommonStates", "PrimaryPressed", "PrimaryButton.Foreground");
            AssertStateSetter(rootGrid, "CommonStates", "SecondaryPointerOver", "PrimaryBackgroundGrid.Background");
            AssertStateSetter(rootGrid, "CommonStates", "SecondaryPressed", "SecondaryButton.Foreground");
            AssertStateSetter(rootGrid, "CommonStates", "Checked", "Border.BorderBrush");
            AssertStateSetter(rootGrid, "CommonStates", "CheckedFlyoutOpen", "SecondaryBackgroundGrid.Background");
            AssertStateSetter(rootGrid, "CommonStates", "CheckedTouchPressed", "PrimaryButton.Foreground");
            AssertStateSetter(rootGrid, "CommonStates", "CheckedPrimaryPointerOver", "SecondaryButton.Foreground");
            AssertStateSetter(rootGrid, "CommonStates", "CheckedPrimaryPressed", "PrimaryBackgroundGrid.Background");
            AssertStateSetter(rootGrid, "CommonStates", "CheckedSecondaryPointerOver", "SecondaryBackgroundGrid.Background");
            AssertStateSetter(rootGrid, "CommonStates", "CheckedSecondaryPressed", "SecondaryButton.Foreground");
            AssertStateSetter(rootGrid, "SecondaryButtonPlacementStates", "SecondaryButtonSpan", "SecondaryButton.(Grid.Column)");
            AssertStateSetter(rootGrid, "SecondaryButtonPlacementStates", "SecondaryButtonSpan", "SecondaryButton.(Grid.ColumnSpan)");

            primaryButton.ApplyTemplate();
            var primaryButtonRoot = primaryButton.Template?.FindName("RootGrid", primaryButton) as FrameworkElement
                ?? throw new AssertFailedException("Expected SplitButtonCommandBarStyle primary button template root.");
            AssertStateSetter(primaryButtonRoot, "CommonStates", "Disabled", "ContentPresenter.Foreground");
            AssertStateSetter(primaryButtonRoot, "CommonStates", "Disabled", "RootGrid.Background");

            Assert.IsTrue(VisualStateManager.GoToState(splitButton, "PrimaryPointerOver", false));
            Assert.AreSame(splitButton.TryFindResource("AppBarButtonBackgroundPointerOver"), primaryBackgroundGrid.Background);
            Assert.AreSame(splitButton.TryFindResource("SplitButtonInAppBarUnfocusedPointerOver"), secondaryBackgroundGrid.Background);
        });
    }

    [TestMethod]
    public void AppBarSeparatorMapsOverflowState()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var separator = new AppBarSeparator();
            using var host = new TestWindowHost(separator, width: 120, height: 80);
            host.UpdateLayout();

            var rootGrid = FindTemplateChild<Grid>(separator, "RootGrid");
            var separatorRectangle = FindTemplateChild<Rectangle>(separator, "SeparatorRectangle");

            Assert.IsFalse(separator.Focusable);
            Assert.IsFalse(separator.IsCompact);
            Assert.IsFalse(separator.IsInOverflow);

            separator.IsCompact = true;
            host.UpdateLayout();

            Assert.AreEqual((double)rootGrid.TryFindResource("AppBarThemeCompactHeight"), rootGrid.Height);
            Assert.AreEqual(VerticalAlignment.Top, rootGrid.VerticalAlignment);

            AppBarElementProperties.SetUseOverflowStyle(separator, true);
            host.UpdateLayout();

            Assert.IsTrue(separator.IsCompact);
            Assert.IsTrue(separator.IsInOverflow);
            Assert.IsTrue(double.IsNaN(separatorRectangle.Width));
            Assert.AreEqual(HorizontalAlignment.Stretch, separatorRectangle.HorizontalAlignment);
            Assert.AreEqual(1.0, separatorRectangle.Height);
            Assert.AreEqual(new Thickness(0, 4, 0, 4), separatorRectangle.Margin);
        });
    }

    private static ResourceDictionary CreateCommandBarResources()
    {
        return new ResourceDictionary
        {
            Source = new Uri("/ModernWpf.Controls;component/CommandBar/CommandBar.xaml", UriKind.Relative)
        };
    }

    private static FrameworkElement CreateTemplateHost(UIElement child, ResourceDictionary resources)
    {
        var root = new Grid();
        root.Resources.MergedDictionaries.Add(resources);
        root.Children.Add(child);
        return root;
    }

    private static void VerifyEmptyLabelPrimaryCommandShowsAutoOverflowButton(Control command)
    {
        var appBarElement = (ICommandBarElement)command;
        var commandBar = new ModernWpf.Controls.CommandBar
        {
            DefaultLabelPosition = CommandBarDefaultLabelPosition.Bottom,
            IsDynamicOverflowEnabled = false
        };
        commandBar.PrimaryCommands.Add(appBarElement);

        using var host = new TestWindowHost(commandBar, width: 240, height: 120);
        host.UpdateLayout();

        var moreButton = FindTemplateChild<ToggleButton>(commandBar, "MoreButton");

        Assert.AreEqual(Visibility.Visible, commandBar.CommandBarTemplateSettings.EffectiveOverflowButtonVisibility);
        Assert.AreEqual(Visibility.Visible, moreButton.Visibility);

        switch (command)
        {
            case AppBarButton button:
                button.LabelPosition = CommandBarLabelPosition.Collapsed;
                break;
            case AppBarToggleButton toggleButton:
                toggleButton.LabelPosition = CommandBarLabelPosition.Collapsed;
                break;
        }

        host.UpdateLayout();

        Assert.AreEqual(Visibility.Collapsed, commandBar.CommandBarTemplateSettings.EffectiveOverflowButtonVisibility);
        Assert.AreEqual(Visibility.Collapsed, moreButton.Visibility);
    }

    private static MouseButtonEventArgs CreateMouseLeftButtonDownArgs(UIElement source)
    {
        return new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonDownEvent,
            Source = source
        };
    }

    private static MouseEventArgs CreateMouseEnterArgs(UIElement source)
    {
        return new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
        {
            RoutedEvent = UIElement.MouseEnterEvent,
            Source = source
        };
    }

    private static void AssertOverflowFlyoutPlacementRectangle(AppBarButton button, Flyout flyout)
    {
        var placementRectangle = flyout.InternalPopup.PlacementRectangle;

        bool hasTargetPointRect =
            Math.Abs(placementRectangle.X - button.ActualWidth) <= 0.1 &&
            placementRectangle.Width == 0 &&
            placementRectangle.Height == 0;
        bool hasExclusionRect =
            Math.Abs(placementRectangle.Right - button.ActualWidth) <= 0.1 &&
            placementRectangle.Left <= 0.1 &&
            placementRectangle.Top <= 0 &&
            placementRectangle.Bottom >= button.ActualHeight;

        Assert.IsTrue(
            hasTargetPointRect || hasExclusionRect,
            $"Expected an overflow target-point or exclusion rectangle for width {button.ActualWidth}; actual {placementRectangle}.");
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        control.ApplyTemplate();

        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template child '{name}' to be {typeof(T).Name}.");
    }

    private static Style FindImplicitStyle<T>(FrameworkElement element)
        where T : FrameworkElement
    {
        return element.TryFindResource(typeof(T)) as Style
            ?? throw new AssertFailedException($"Expected implicit style for {typeof(T).Name}.");
    }

    private static void AssertStyleDoesNotTriggerOnToolBarIsOverflowItem(Style style)
    {
        for (Style? current = style; current is not null; current = current.BasedOn)
        {
            foreach (TriggerBase triggerBase in current.Triggers)
            {
                if (triggerBase is Trigger trigger &&
                    trigger.Property == ToolBar.IsOverflowItemProperty)
                {
                    Assert.Fail("AppBar default styles must not use WPF ToolBar.IsOverflowItem for overflow sizing.");
                }
            }
        }
    }

    private static void AssertOverflowStyleMatchesWinUISource(Style style, Type targetType)
    {
        Assert.AreEqual(targetType, style.TargetType);
        AssertStyleSetter(style, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
        AssertStyleSetter(style, FrameworkElement.WidthProperty, double.NaN);
    }

    private static void AssertStyleSetter(Style style, DependencyProperty property, object expectedValue)
    {
        for (Style? current = style; current is not null; current = current.BasedOn)
        {
            foreach (SetterBase setterBase in current.Setters)
            {
                if (setterBase is Setter setter &&
                    setter.Property == property &&
                    ValuesAreEqual(setter.Value, expectedValue))
                {
                    return;
                }
            }
        }

        Assert.Fail($"Expected style for {style.TargetType.Name} to set {property.Name} to {expectedValue}.");
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        for (Style? current = style; current is not null; current = current.BasedOn)
        {
            foreach (SetterBase setterBase in current.Setters)
            {
                if (setterBase is Setter setter &&
                    setter.Property == property &&
                    setter.Value is DynamicResourceExtension dynamicResource &&
                    Equals(dynamicResource.ResourceKey, expectedResourceKey))
                {
                    return;
                }
            }
        }

        Assert.Fail($"Expected style for {style.TargetType.Name} to set {property.Name} from dynamic resource {expectedResourceKey}.");
    }

    private static bool ValuesAreEqual(object actual, object expected)
    {
        return (actual is double actualDouble &&
            expected is double expectedDouble &&
            double.IsNaN(actualDouble) &&
            double.IsNaN(expectedDouble)) ||
            Equals(actual, expected);
    }

    private static void AssertThemeResourceReference(string themeName, object resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceReferences(
        string themeName,
        params (object ResourceKey, object ExpectedResourceKey)[] expectedResources)
    {
        foreach (var expectedResource in expectedResources)
        {
            AssertThemeResourceReference(themeName, expectedResource.ResourceKey, expectedResource.ExpectedResourceKey);
        }
    }

    private static void AssertThemeResourceValue<T>(string themeName, object resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static void AssertCommonAppBarMetrics(string themeName)
    {
        AssertThemeResourceValue(themeName, "AppBarExpandButtonThemeHeight", 24d);
        AssertThemeResourceValue(themeName, "AppBarExpandButtonThemeWidth", 48d);
        AssertThemeResourceValue(themeName, "AppBarExpandButtonCircleDiameter", 3d);
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string? expectedTarget,
        string? expectedProperty = null)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        Assert.IsNotNull(group, $"Expected visual state group '{groupName}'.");

        var state = FindVisualState(group!, stateName);
        Assert.IsNotNull(state, $"Expected visual state '{groupName}.{stateName}'.");
        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        foreach (VisualStateSetter setter in stateEx.Setters)
        {
            bool targetMatches = expectedTarget is null ?
                string.IsNullOrEmpty(setter.Target) :
                setter.Target == expectedTarget;
            bool propertyMatches = expectedProperty is null ?
                string.IsNullOrEmpty(setter.Property) :
                setter.Property == expectedProperty;

            if (targetMatches && propertyMatches)
            {
                return;
            }
        }

        Assert.Fail(
            $"Expected visual state '{groupName}.{stateName}' to contain setter '{expectedTarget ?? expectedProperty}'.");
    }

    private static void AssertStateSetterDynamicResource(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string target,
        object expectedResourceKey)
    {
        var setter = FindStateSetter(stateGroupsRoot, groupName, stateName, target);

        AssertResourceReferenceExpression(
            setter.ReadLocalValue(VisualStateSetter.ValueProperty),
            expectedResourceKey);
    }

    private static VisualStateSetter FindStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string target)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        Assert.IsNotNull(group, $"Expected visual state group '{groupName}'.");

        var state = FindVisualState(group!, stateName);
        Assert.IsNotNull(state, $"Expected visual state '{groupName}.{stateName}'.");
        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        return stateEx.Setters.SingleOrDefault(setter => setter.Target == target)
            ?? throw new AssertFailedException(
                $"Expected visual state '{groupName}.{stateName}' to contain setter '{target}'.");
    }

    private static void AssertResourceReferenceExpression(object value, object expectedResourceKey)
    {
        Assert.IsNotNull(value, "Expected dynamic resource local value.");
        Assert.AreEqual("System.Windows.ResourceReferenceExpression", value.GetType().FullName);
        var resourceKeyProperty = value.GetType().GetProperty(
            "ResourceKey",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(resourceKeyProperty, "Expected ResourceReferenceExpression.ResourceKey.");
        Assert.AreEqual(expectedResourceKey, resourceKeyProperty!.GetValue(value));
    }

    private static void AssertStateSetterAbsent(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string? expectedTarget,
        string? expectedProperty = null)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        Assert.IsNotNull(group, $"Expected visual state group '{groupName}'.");

        var state = FindVisualState(group!, stateName);
        Assert.IsNotNull(state, $"Expected visual state '{groupName}.{stateName}'.");
        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        foreach (VisualStateSetter setter in stateEx.Setters)
        {
            bool targetMatches = expectedTarget is null ?
                string.IsNullOrEmpty(setter.Target) :
                setter.Target == expectedTarget;
            bool propertyMatches = expectedProperty is null ?
                string.IsNullOrEmpty(setter.Property) :
                setter.Property == expectedProperty;

            if (targetMatches && propertyMatches)
            {
                Assert.Fail(
                    $"Expected visual state '{groupName}.{stateName}' not to contain setter '{expectedTarget ?? expectedProperty}'.");
            }
        }
    }

    private static void AssertVisualState(FrameworkElement stateGroupsRoot, string groupName, string expectedStateName)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        Assert.IsNotNull(group, $"Expected visual state group '{groupName}'.");
        Assert.AreEqual(expectedStateName, group!.CurrentState?.Name);
    }

    private static VisualStateGroup? FindVisualStateGroup(FrameworkElement stateGroupsRoot, string groupName)
    {
        foreach (VisualStateGroup group in VisualStateManager.GetVisualStateGroups(stateGroupsRoot))
        {
            if (group.Name == groupName)
            {
                return group;
            }
        }

        return null;
    }

    private static VisualState? FindVisualState(VisualStateGroup group, string stateName)
    {
        foreach (VisualState state in group.States)
        {
            if (state.Name == stateName)
            {
                return state;
            }
        }

        return null;
    }

    private static KeyEventArgs CreateKeyEventArgs(Visual source, Key key)
    {
        return new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(source),
            Environment.TickCount,
            key)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };
    }

    private sealed class TestCommandBar : ModernWpf.Controls.CommandBar
    {
        public IList<string>? LifecycleLog { get; set; }

        public void InvokeKeyDown(KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        protected override void OnOpening(object e)
        {
            LifecycleLog?.Add("OnOpening");
            base.OnOpening(e);
        }

        protected override void OnOpened(object e)
        {
            LifecycleLog?.Add("OnOpened");
            base.OnOpened(e);
        }

        protected override void OnClosing(object e)
        {
            LifecycleLog?.Add("OnClosing");
            base.OnClosing(e);
        }

        protected override void OnClosed(object e)
        {
            LifecycleLog?.Add("OnClosed");
            base.OnClosed(e);
        }
    }

    private sealed class TestAppBarButton : AppBarButton
    {
        public void InvokeMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);
        }

        public void InvokeMouseEnter(MouseEventArgs e)
        {
            OnMouseEnter(e);
        }

        public void InvokeClick()
        {
            OnClick();
        }
    }

    private sealed class TestAppBarToggleButton : AppBarToggleButton
    {
        public void InvokeMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);
        }

        public void InvokeMouseEnter(MouseEventArgs e)
        {
            OnMouseEnter(e);
        }

        public void InvokeClick()
        {
            OnClick();
        }
    }

    private sealed class RecordingCommand : ICommand
    {
        public int ExecuteCount { get; private set; }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            ExecuteCount++;
        }
    }
}
