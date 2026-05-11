using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
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
    public void AppBarSeparatorMapsOverflowState()
    {
        WpfTestHost.Run(() =>
        {
            var separator = new AppBarSeparator();

            Assert.IsFalse(separator.Focusable);
            Assert.IsFalse(separator.IsCompact);
            Assert.IsFalse(separator.IsInOverflow);

            separator.IsCompact = true;
            ToolBar.SetOverflowMode(separator, OverflowMode.Always);

            Assert.IsTrue(separator.IsCompact);
            Assert.IsTrue(separator.IsInOverflow);
        });
    }
}
