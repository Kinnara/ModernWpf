using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestInfra;
using CommandBarFlyout = ModernWpf.Controls.CommandBarFlyout;

namespace ModernWpf.WinUI.Tests.CommandBarFlyouts;

[TestClass]
public class CommandBarFlyoutApiTests
{
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

    private static CommandBarFlyoutCommandBar GetCommandBar(CommandBarFlyout commandBarFlyout)
    {
        var presenter = commandBarFlyout.GetPresenter();
        Assert.IsNotNull(presenter);

        var commandBar = presenter.Content as CommandBarFlyoutCommandBar;
        Assert.IsNotNull(commandBar);
        return commandBar!;
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
}
