using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
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
            Assert.AreSame(icon, button.Icon);
            Assert.IsTrue(button.IsCompact);
            Assert.IsFalse(button.IsInOverflow);
            Assert.AreEqual(CommandBarLabelPosition.Collapsed, button.LabelPosition);
            Assert.IsNull(button.Flyout);
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

            var button = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "Accept",
                InputGestureText = "Ctrl+A",
                Flyout = new MenuFlyout()
            };

            using var host = new TestWindowHost(button, width: 180, height: 120);
            host.UpdateLayout();

            var root = FindTemplateChild<Border>(button, "Root");

            AssertStateSetter(root, "ApplicationViewStates", "Compact", "AppBarButtonInnerBorder.Margin");
            AssertStateSetter(root, "ApplicationViewStates", "LabelOnRight", "TextLabel.(Grid.Row)");
            AssertStateSetter(root, "ApplicationViewStates", "LabelOnRight", null, "Width");
            AssertStateSetter(root, "ApplicationViewStates", "Overflow", "OverflowTextLabel.Visibility");
            AssertStateSetter(root, "ApplicationViewStates", "OverflowWithMenuIcons", "ContentViewbox.Width");
            AssertStateSetter(root, "ApplicationViewStates", "OverflowWithToggleButtonsAndMenuIcons", "OverflowTextLabel.Margin");

            AssertStateSetter(root, "CommonStates", "PointerOver", "AppBarButtonInnerBorder.Background");
            AssertStateSetter(root, "CommonStates", "Pressed", "Content.Foreground");
            AssertStateSetter(root, "CommonStates", "Disabled", "KeyboardAcceleratorTextLabel.Foreground");
            AssertStateSetter(root, "CommonStates", "OverflowPointerOver", "SubItemChevron.Foreground");
            AssertStateSetter(root, "CommonStates", "OverflowPressed", "SubItemChevron.Foreground");

            AssertStateSetter(root, "KeyboardAcceleratorTextVisibility", "KeyboardAcceleratorTextVisible", "KeyboardAcceleratorTextLabel.Visibility");
            AssertStateSetter(root, "FlyoutStates", "HasFlyout", "SubItemChevron.Visibility");
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

            Assert.AreEqual("Custom content", presenter.Content);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreEqual(container.Padding, presenter.Margin);
            Assert.AreEqual(HorizontalAlignment.Center, presenter.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalAlignment);
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

            ToolBar.SetOverflowMode(separator, OverflowMode.Always);
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

    private static T FindTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        control.ApplyTemplate();

        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template child '{name}' to be {typeof(T).Name}.");
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
}
