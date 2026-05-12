using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.SwipeControl;

[TestClass]
public class SwipeControlApiTests
{
    [TestMethod]
    public void SwipeItemTest()
    {
        WpfTestHost.Run(() =>
        {
            var swipeItem = new SwipeItem
            {
                Text = "Selfie",
                IconSource = new FontIconSource { Glyph = "&#xE114;" },
                Background = new SolidColorBrush(Colors.Red),
                Foreground = new SolidColorBrush(Colors.Blue)
            };

            Assert.AreEqual("Selfie", swipeItem.Text);
            Assert.IsInstanceOfType(swipeItem.IconSource, typeof(FontIconSource));
            Assert.AreEqual("&#xE114;", ((FontIconSource)swipeItem.IconSource).Glyph);
            Assert.AreEqual(Colors.Red, ((SolidColorBrush)swipeItem.Background).Color);
            Assert.AreEqual(Colors.Blue, ((SolidColorBrush)swipeItem.Foreground).Color);
            Assert.IsNull(swipeItem.Command);
            Assert.IsNull(swipeItem.CommandParameter);
            Assert.AreEqual(SwipeBehaviorOnInvoked.Auto, swipeItem.BehaviorOnInvoked);
        });
    }

    [TestMethod]
    public void SwipeItemsTest()
    {
        WpfTestHost.Run(() =>
        {
            var swipeItems = new SwipeItems();

            Assert.AreEqual(SwipeMode.Reveal, swipeItems.Mode);
            Assert.AreEqual(0, swipeItems.Count);

            swipeItems.Add(new SwipeItem());
            swipeItems.Add(new SwipeItem());

            Assert.AreEqual(2, swipeItems.Count);
        });
    }

    [TestMethod]
    public void SwipeItemsExecuteThrowsExceptionWhenMoreThanOneItemAreAdded()
    {
        WpfTestHost.Run(() =>
        {
            var swipeItems = new SwipeItems
            {
                Mode = SwipeMode.Execute
            };

            swipeItems.Add(new SwipeItem());

            Assert.ThrowsException<ArgumentException>(() => swipeItems.Add(new SwipeItem()));
        });
    }

    [TestMethod]
    public void SwipeControlTest()
    {
        WpfTestHost.Run(() =>
        {
            var swipeControl = new ModernWpf.Controls.SwipeControl();

            Assert.AreEqual(0d, swipeControl.ActualHeight);
            Assert.AreEqual(0d, swipeControl.ActualWidth);
            Assert.IsNull(swipeControl.LeftItems);
            Assert.IsNull(swipeControl.RightItems);
            Assert.IsNull(swipeControl.TopItems);
            Assert.IsNull(swipeControl.BottomItems);

            swipeControl.LeftItems = new SwipeItems();
            swipeControl.RightItems = new SwipeItems();

            using var host = new TestWindowHost(swipeControl, width: 240, height: 120);

            Assert.IsFalse(swipeControl.IsTabStop);
            Assert.IsNotNull(swipeControl.LeftItems);
            Assert.IsNotNull(swipeControl.RightItems);
        });
    }

    [TestMethod]
    public void SwipeControlCanOnlyBeHorizontalOrVertical()
    {
        WpfTestHost.Run(() =>
        {
            var swipeControl = new ModernWpf.Controls.SwipeControl
            {
                LeftItems = new SwipeItems()
            };
            var topItems = new SwipeItems
            {
                new SwipeItem()
            };

            swipeControl.TopItems = topItems;

            Assert.ThrowsException<ArgumentException>(() => swipeControl.LeftItems.Add(new SwipeItem()));
        });
    }

    [TestMethod]
    public void SwipeControlCanOnlyBeHorizontalOrVerticalAfterRendering()
    {
        WpfTestHost.Run(() =>
        {
            var swipeControl = new ModernWpf.Controls.SwipeControl
            {
                TopItems = new SwipeItems()
            };

            using var host = new TestWindowHost(swipeControl, width: 240, height: 120);

            swipeControl.LeftItems = new SwipeItems();
            swipeControl.LeftItems.Add(new SwipeItem());

            Assert.ThrowsException<ArgumentException>(() => swipeControl.TopItems.Add(new SwipeItem()));
        });
    }

    [TestMethod]
    public void MarkupDefinedSwipeItemDoesNotCrash()
    {
        WpfTestHost.Run(() =>
        {
            var rootGrid = (Grid)XamlReader.Parse(
                "<Grid xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
                "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' " +
                "xmlns:controls='clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls'> " +
                    "<ListView> " +
                        "<ListViewItem> " +
                            "<controls:SwipeControl> " +
                                "<controls:SwipeControl.RightItems> " +
                                    "<controls:SwipeItems> " +
                                        "<controls:SwipeItem Background='#E81123' Foreground='White' Text='Remove'/> " +
                                    "</controls:SwipeItems> " +
                                "</controls:SwipeControl.RightItems> " +
                                "<Grid Width='200' Height='200' Background='Green'/> " +
                            "</controls:SwipeControl> " +
                        "</ListViewItem> " +
                    "</ListView> " +
                "</Grid>");

            using var host = new TestWindowHost(rootGrid, width: 300, height: 260);

            var swipeControl = VisualTreeTestHelper
                .EnumerateDescendants(rootGrid)
                .OfType<ModernWpf.Controls.SwipeControl>()
                .FirstOrDefault();

            Assert.IsNotNull(swipeControl);
            Assert.AreEqual(1, swipeControl!.RightItems.Count);
            Assert.AreEqual("Remove", swipeControl.RightItems[0].Text);
        });
    }

    [TestMethod]
    public void SwipeItemButtonInvokesEventAndCommand()
    {
        WpfTestHost.Run(() =>
        {
            var command = new TestCommand();
            var swipeItem = new SwipeItem
            {
                Text = "Delete",
                Command = command,
                CommandParameter = "row"
            };
            var invoked = false;
            ModernWpf.Controls.SwipeControl? invokedControl = null;
            swipeItem.Invoked += (sender, args) =>
            {
                invoked = ReferenceEquals(sender, swipeItem);
                invokedControl = args.SwipeControl;
            };
            var swipeControl = new ModernWpf.Controls.SwipeControl
            {
                Content = new TextBlock { Text = "Item" },
                RightItems = new SwipeItems { swipeItem }
            };

            using var host = new TestWindowHost(swipeControl, width: 240, height: 120);

            var button = VisualTreeTestHelper
                .EnumerateDescendants(swipeControl)
                .OfType<Button>()
                .FirstOrDefault(candidate => candidate.Tag == swipeItem);

            Assert.IsNotNull(button);

            button!.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            Assert.IsTrue(invoked);
            Assert.AreSame(swipeControl, invokedControl);
            Assert.AreEqual("row", command.ExecutedParameter);
        });
    }

    [TestMethod]
    public void DragRevealsRightItemsAndCloseResetsOffset()
    {
        WpfTestHost.Run(() =>
        {
            var swipeControl = new ModernWpf.Controls.SwipeControl
            {
                Content = new Border { Width = 200, Height = 48, Background = Brushes.Green },
                RightItems = new SwipeItems
                {
                    new SwipeItem { Text = "Delete" }
                }
            };

            using var host = new TestWindowHost(swipeControl, width: 260, height: 120);

            swipeControl.DragForTesting(-96, 0, complete: true);
            host.UpdateLayout();

            Assert.IsTrue(swipeControl.IsOpenForTesting);
            Assert.AreEqual(SwipeItemsPlacement.Right, swipeControl.OpenedItemsPlacementForTesting);
            Assert.IsTrue(swipeControl.HorizontalOffsetForTesting < 0);

            swipeControl.Close();
            host.UpdateLayout();

            Assert.IsFalse(swipeControl.IsOpenForTesting);
            Assert.AreEqual(0d, swipeControl.HorizontalOffsetForTesting);
        });
    }

    [TestMethod]
    public void ExecuteSwipeInvokesSingleItemAndCloses()
    {
        WpfTestHost.Run(() =>
        {
            var command = new TestCommand();
            var swipeItem = new SwipeItem
            {
                Text = "Delete",
                Command = command,
                CommandParameter = "row"
            };
            var invoked = false;
            swipeItem.Invoked += (_, _) => invoked = true;
            var executeItems = new SwipeItems { Mode = SwipeMode.Execute };
            executeItems.Add(swipeItem);
            var swipeControl = new ModernWpf.Controls.SwipeControl
            {
                Content = new Border { Width = 200, Height = 48, Background = Brushes.Green },
                RightItems = executeItems
            };

            using var host = new TestWindowHost(swipeControl, width: 260, height: 120);

            swipeControl.DragForTesting(-96, 0, complete: true);
            host.UpdateLayout();

            Assert.IsTrue(invoked);
            Assert.AreEqual("row", command.ExecutedParameter);
            Assert.IsFalse(swipeControl.IsOpenForTesting);
            Assert.AreEqual(0d, swipeControl.HorizontalOffsetForTesting);
        });
    }

    [TestMethod]
    public void OutsideTapDismissesOpenSwipe()
    {
        WpfTestHost.Run(() =>
        {
            var outside = new Button { Content = "Outside", Width = 80, Height = 24 };
            var swipeControl = new ModernWpf.Controls.SwipeControl
            {
                Content = new Border { Width = 200, Height = 48, Background = Brushes.Green },
                RightItems = new SwipeItems
                {
                    new SwipeItem { Text = "Delete" }
                }
            };
            var root = new StackPanel
            {
                Children =
                {
                    swipeControl,
                    outside
                }
            };

            using var host = new TestWindowHost(root, width: 260, height: 160);

            swipeControl.DragForTesting(-96, 0, complete: true);
            Assert.IsTrue(swipeControl.IsOpenForTesting);

            outside.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.PreviewMouseDownEvent,
                Source = outside
            });
            host.UpdateLayout();

            Assert.IsFalse(swipeControl.IsOpenForTesting);
            Assert.AreEqual(0d, swipeControl.HorizontalOffsetForTesting);
        });
    }

    [TestMethod]
    public void RevealedButtonInvokesAndAutoCloses()
    {
        WpfTestHost.Run(() =>
        {
            var command = new TestCommand();
            var swipeItem = new SwipeItem
            {
                Text = "Delete",
                Command = command,
                CommandParameter = "row"
            };
            var swipeControl = new ModernWpf.Controls.SwipeControl
            {
                Content = new Border { Width = 200, Height = 48, Background = Brushes.Green },
                RightItems = new SwipeItems { swipeItem }
            };

            using var host = new TestWindowHost(swipeControl, width: 260, height: 120);

            swipeControl.DragForTesting(-96, 0, complete: true);
            Assert.IsTrue(swipeControl.IsOpenForTesting);

            var button = VisualTreeTestHelper
                .EnumerateDescendants(swipeControl)
                .OfType<Button>()
                .FirstOrDefault(candidate => candidate.Tag == swipeItem);

            Assert.IsNotNull(button);
            button!.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            Assert.AreEqual("row", command.ExecutedParameter);
            Assert.IsFalse(swipeControl.IsOpenForTesting);
            Assert.AreEqual(0d, swipeControl.HorizontalOffsetForTesting);
        });
    }

    private sealed class TestCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public object? ExecutedParameter { get; private set; }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            ExecutedParameter = parameter;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
