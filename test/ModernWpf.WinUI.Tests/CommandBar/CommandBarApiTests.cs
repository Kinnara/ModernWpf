using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
                IsDynamicOverflowEnabled = false,
                DefaultLabelPosition = CommandBarDefaultLabelPosition.Collapsed,
                OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Visible
            };

            Assert.AreEqual("Title", commandBar.Content);
            Assert.IsNotNull(commandBar.ContentTemplate);
            Assert.AreSame(overflowStyle, commandBar.CommandBarOverflowPresenterStyle);
            Assert.IsTrue(commandBar.IsOpen);
            Assert.IsFalse(commandBar.IsDynamicOverflowEnabled);
            Assert.AreEqual(CommandBarDefaultLabelPosition.Collapsed, commandBar.DefaultLabelPosition);
            Assert.AreEqual(CommandBarOverflowButtonVisibility.Visible, commandBar.OverflowButtonVisibility);
        });
    }

    [TestMethod]
    public void CommandCollectionsApplyOverflowModes()
    {
        WpfTestHost.Run(() =>
        {
            var commandBar = new ModernWpf.Controls.CommandBar();
            var primary = new AppBarButton();
            var secondary = new AppBarButton();

            commandBar.PrimaryCommands.Add(primary);
            commandBar.SecondaryCommands.Add(secondary);

            Assert.AreEqual(OverflowMode.AsNeeded, ToolBar.GetOverflowMode(primary));
            Assert.AreEqual(OverflowMode.Always, ToolBar.GetOverflowMode(secondary));

            commandBar.IsDynamicOverflowEnabled = false;
            Assert.AreEqual(OverflowMode.Never, ToolBar.GetOverflowMode(primary));

            var secondPrimary = new AppBarButton();
            commandBar.PrimaryCommands.Add(secondPrimary);
            Assert.AreEqual(OverflowMode.Never, ToolBar.GetOverflowMode(secondPrimary));

            commandBar.IsDynamicOverflowEnabled = true;
            Assert.AreEqual(OverflowMode.AsNeeded, ToolBar.GetOverflowMode(primary));
            Assert.AreEqual(OverflowMode.AsNeeded, ToolBar.GetOverflowMode(secondPrimary));
            Assert.AreEqual(OverflowMode.Always, ToolBar.GetOverflowMode(secondary));
        });
    }

    [TestMethod]
    public void CommandBarToolBarDynamicOverflowUsesVisualStateSetters()
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

            var toolBar = FindTemplateChild<CommandBarToolBar>(commandBar, "PART_ToolBar");
            var contentColumn = FindTemplateChild<ColumnDefinition>(toolBar, "ContentControlColumnDefinition");
            var primaryColumn = FindTemplateChild<ColumnDefinition>(toolBar, "PrimaryItemsControlColumnDefinition");

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
            Assert.AreEqual(new Rect(button.ActualWidth, 0, 0, 0), flyout.InternalPopup.PlacementRectangle);

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
            Assert.AreEqual(new Rect(button.ActualWidth, 0, 0, 0), flyout.InternalPopup.PlacementRectangle);

            flyout.Hide();
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

            var toolBar = FindTemplateChild<CommandBarToolBar>(commandBar, "PART_ToolBar");
            var layoutRoot = FindTemplateChild<Grid>(toolBar, "LayoutRoot");

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

            var toolBar = FindTemplateChild<CommandBarToolBar>(commandBar, "PART_ToolBar");
            var moreButton = FindTemplateChild<ToggleButton>(toolBar, "MoreButton");

            Assert.AreEqual(Visibility.Visible, toolBar.EffectiveOverflowButtonVisibility);
            Assert.IsTrue(toolBar.EffectiveOverflowButtonEnabled);
            Assert.AreEqual(Visibility.Visible, moreButton.Visibility);
            Assert.IsTrue(moreButton.IsEnabled);

            button.LabelPosition = CommandBarLabelPosition.Collapsed;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, toolBar.EffectiveOverflowButtonVisibility);
            Assert.IsFalse(toolBar.EffectiveOverflowButtonEnabled);
            Assert.AreEqual(Visibility.Collapsed, moreButton.Visibility);
            Assert.IsFalse(moreButton.IsEnabled);
        });
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
            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(primaryButton)
                ?? throw new AssertFailedException("Expected SplitButtonCommandBarStyle primary button to use ContentPresenterEx.");

            Assert.AreEqual("Accept", presenter.Content);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreEqual(splitButton.Foreground, presenter.Foreground);
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

        var toolBar = FindTemplateChild<CommandBarToolBar>(commandBar, "PART_ToolBar");
        var moreButton = FindTemplateChild<ToggleButton>(toolBar, "MoreButton");

        Assert.AreEqual(Visibility.Visible, toolBar.EffectiveOverflowButtonVisibility);
        Assert.IsTrue(toolBar.EffectiveOverflowButtonEnabled);
        Assert.AreEqual(Visibility.Visible, moreButton.Visibility);
        Assert.IsTrue(moreButton.IsEnabled);

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

        Assert.AreEqual(Visibility.Collapsed, toolBar.EffectiveOverflowButtonVisibility);
        Assert.IsFalse(toolBar.EffectiveOverflowButtonEnabled);
        Assert.AreEqual(Visibility.Collapsed, moreButton.Visibility);
        Assert.IsFalse(moreButton.IsEnabled);
    }

    private static MouseButtonEventArgs CreateMouseLeftButtonDownArgs(UIElement source)
    {
        return new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonDownEvent,
            Source = source
        };
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

    private static bool ValuesAreEqual(object actual, object expected)
    {
        return (actual is double actualDouble &&
            expected is double expectedDouble &&
            double.IsNaN(actualDouble) &&
            double.IsNaN(expectedDouble)) ||
            Equals(actual, expected);
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

    private sealed class TestAppBarButton : AppBarButton
    {
        public void InvokeMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);
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
