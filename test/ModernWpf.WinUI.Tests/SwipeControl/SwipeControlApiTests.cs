using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
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
    public void SwipeControlAcceptsWinUIContentPresenterSurface()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new TransitionCollection();
            var swipeControl = new ModernWpf.Controls.SwipeControl
            {
                CornerRadius = new CornerRadius(4),
                ContentTransitions = transitions
            };

            Assert.AreEqual(new CornerRadius(4), swipeControl.CornerRadius);
            Assert.AreSame(transitions, swipeControl.ContentTransitions);
        });
    }

    [TestMethod]
    public void SwipeControlTemplateUsesWinUIContentPresenter()
    {
        WpfTestHost.Run(() =>
        {
            var content = new Border { Width = 80, Height = 24 };
            var transitions = new TransitionCollection();
            var background = new SolidColorBrush(Colors.Red);
            var borderBrush = new SolidColorBrush(Colors.Blue);
            var swipeControl = new ModernWpf.Controls.SwipeControl
            {
                Content = content,
                Background = background,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1, 2, 3, 4),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(6),
                ContentTransitions = transitions
            };

            using var host = new TestWindowHost(swipeControl, width: 240, height: 120);

            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(swipeControl)
                ?? throw new AssertFailedException("Expected SwipeControl template to use ContentPresenterEx.");

            Assert.AreSame(content, presenter.Content);
            Assert.AreSame(background, presenter.Background);
            Assert.AreSame(borderBrush, presenter.BorderBrush);
            Assert.AreEqual(new Thickness(1, 2, 3, 4), presenter.BorderThickness);
            Assert.AreEqual(new CornerRadius(5), presenter.CornerRadius);
            Assert.AreEqual(new Thickness(6), presenter.Padding);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreEqual(HorizontalAlignment.Stretch, presenter.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Stretch, presenter.VerticalContentAlignment);
            var rootGrid = FindDescendantByName<Grid>(swipeControl, "RootGrid");
            var swipeContentRoot = FindDescendantByName<Grid>(swipeControl, "SwipeContentRoot");
            var swipeContentStackPanel = FindDescendantByName<StackPanel>(swipeControl, "SwipeContentStackPanel");
            var contentRoot = FindDescendantByName<Grid>(swipeControl, "ContentRoot");
            var inputEater = FindDescendantByName<Grid>(swipeControl, "InputEater");

            Assert.IsNotNull(rootGrid);
            Assert.IsNotNull(swipeContentRoot);
            Assert.IsNotNull(swipeContentStackPanel);
            Assert.IsNotNull(contentRoot);
            Assert.IsNotNull(inputEater);
            Assert.IsInstanceOfType(contentRoot!.RenderTransform, typeof(TranslateTransform));
            Assert.IsNull(FindDescendantByName<Panel>(swipeControl, "PART_LeftItemsPanel"));
            Assert.IsNull(FindDescendantByName<Panel>(swipeControl, "PART_RightItemsPanel"));
            Assert.IsNull(FindDescendantByName<Panel>(swipeControl, "PART_TopItemsPanel"));
            Assert.IsNull(FindDescendantByName<Panel>(swipeControl, "PART_BottomItemsPanel"));
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
                LeftItems = new SwipeItems { swipeItem }
            };

            using var host = new TestWindowHost(swipeControl, width: 240, height: 120);

            swipeControl.DragForTesting(-120, 0, complete: true);
            host.UpdateLayout();

            var button = VisualTreeTestHelper
                .EnumerateDescendants(swipeControl)
                .OfType<AppBarButton>()
                .FirstOrDefault(candidate => candidate.Label == "Delete");

            Assert.IsNotNull(button);

            button!.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            Assert.IsTrue(invoked);
            Assert.AreSame(swipeControl, invokedControl);
            Assert.AreEqual("row", command.ExecutedParameter);
        });
    }

    [TestMethod]
    public void SwipeItemStylePressedStateUsesVisualStateSetter()
    {
        WpfTestHost.Run(() =>
        {
            var swipeItem = new SwipeItem { Text = "Delete" };
            var swipeControl = new ModernWpf.Controls.SwipeControl
            {
                Content = new TextBlock { Text = "Item" },
                LeftItems = new SwipeItems { swipeItem }
            };

            using var host = new TestWindowHost(swipeControl, width: 240, height: 120);

            swipeControl.DragForTesting(-120, 0, complete: true);
            host.UpdateLayout();

            var button = VisualTreeTestHelper
                .EnumerateDescendants(swipeControl)
                .OfType<AppBarButton>()
                .FirstOrDefault(candidate => candidate.Label == "Delete");

            Assert.IsNotNull(button);
            Assert.IsNotNull(button!.Style);
            AssertDynamicResourceSetter(button.Style!, Control.FontFamilyProperty, "ContentControlThemeFontFamily");
            AssertDynamicResourceSetter(button.Style!, Control.BackgroundProperty, "SwipeItemBackground");
            AssertDynamicResourceSetter(button.Style!, Control.ForegroundProperty, "SwipeItemForeground");

            button.ApplyTemplate();
            host.UpdateLayout();

            var root = VisualTreeTestHelper
                .EnumerateDescendants(button)
                .OfType<Grid>()
                .FirstOrDefault(candidate => candidate.Name == "Root");
            Assert.IsNotNull(root);
            var rootGrid = root!;

            var commonStates = VisualStateManager.GetVisualStateGroups(rootGrid)
                .OfType<VisualStateGroup>()
                .Single(group => group.Name == "CommonStates");
            var pressedState = commonStates.States
                .Cast<VisualState>()
                .Single(state => state.Name == "Pressed");
            Assert.IsInstanceOfType(pressedState, typeof(VisualStateEx));

            var stateEx = (VisualStateEx)pressedState;
            Assert.AreEqual(1, stateEx.Setters.Count);
            Assert.AreEqual("Root.Background", stateEx.Setters[0].Target);
            AssertResourceReferenceExpression(
                stateEx.Setters[0].ReadLocalValue(VisualStateSetter.ValueProperty),
                "SwipeItemBackgroundPressed");

            Assert.AreEqual(68.0, button.MinWidth);
            Assert.AreEqual(40.0, button.MinHeight);
            Assert.AreEqual(FontWeights.Normal, button.FontWeight);
            Assert.AreSame(button.TryFindResource("SwipeItemBackground"), button.Background);
            Assert.AreSame(button.TryFindResource("SwipeItemForeground"), button.Foreground);

            AssertResourceAlias(rootGrid, "SwipeItemBackground", "ControlFillColorTertiaryBrush");
            AssertResourceAlias(rootGrid, "SwipeItemForeground", "TextFillColorPrimaryBrush");
            AssertResourceAlias(rootGrid, "SwipeItemBackgroundPressed", "ControlAltFillColorQuarternaryBrush");
            AssertResourceAlias(rootGrid, "SwipeItemPreThresholdExecuteForeground", "ControlStrongFillColorDefaultBrush");
            AssertResourceAlias(rootGrid, "SwipeItemPreThresholdExecuteBackground", "ControlFillColorTertiaryBrush");
            AssertResourceAlias(rootGrid, "SwipeItemPostThresholdExecuteForeground", "TextOnAccentFillColorPrimaryBrush");
            AssertResourceAlias(rootGrid, "SwipeItemPostThresholdExecuteBackground", "AccentFillColorDefaultBrush");
        });
    }

    [TestMethod]
    public void DragRevealsLeftItemsAndCloseResetsOffset()
    {
        WpfTestHost.Run(() =>
        {
            var swipeControl = new ModernWpf.Controls.SwipeControl
            {
                Content = new Border { Width = 200, Height = 48, Background = Brushes.Green },
                LeftItems = new SwipeItems
                {
                    new SwipeItem { Text = "Delete" }
                }
            };

            using var host = new TestWindowHost(swipeControl, width: 260, height: 120);

            swipeControl.DragForTesting(-120, 0, complete: true);
            host.UpdateLayout();

            Assert.IsTrue(swipeControl.IsOpenForTesting);
            Assert.AreEqual(SwipeItemsPlacement.Left, swipeControl.OpenedItemsPlacementForTesting);
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
                LeftItems = executeItems
            };

            using var host = new TestWindowHost(swipeControl, width: 260, height: 120);

            swipeControl.DragForTesting(-120, 0, complete: true);
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
                LeftItems = new SwipeItems
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

            swipeControl.DragForTesting(-120, 0, complete: true);
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
                LeftItems = new SwipeItems { swipeItem }
            };

            using var host = new TestWindowHost(swipeControl, width: 260, height: 120);

            swipeControl.DragForTesting(-120, 0, complete: true);
            Assert.IsTrue(swipeControl.IsOpenForTesting);

            var button = VisualTreeTestHelper
                .EnumerateDescendants(swipeControl)
                .OfType<AppBarButton>()
                .FirstOrDefault(candidate => candidate.Label == "Delete");

            Assert.IsNotNull(button);
            button!.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            Assert.AreEqual("row", command.ExecutedParameter);
            Assert.IsFalse(swipeControl.IsOpenForTesting);
            Assert.AreEqual(0d, swipeControl.HorizontalOffsetForTesting);
        });
    }

    [TestMethod]
    public void ExecuteSwipeRemainOpenKeepsExecuteContentOpen()
    {
        WpfTestHost.Run(() =>
        {
            var swipeItem = new SwipeItem
            {
                Text = "Delete",
                BehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen
            };
            var invoked = false;
            swipeItem.Invoked += (_, _) => invoked = true;
            var executeItems = new SwipeItems { Mode = SwipeMode.Execute };
            executeItems.Add(swipeItem);
            var swipeControl = new ModernWpf.Controls.SwipeControl
            {
                Content = new Border { Width = 200, Height = 48, Background = Brushes.Green },
                LeftItems = executeItems
            };

            using var host = new TestWindowHost(swipeControl, width: 260, height: 120);

            swipeControl.DragForTesting(-120, 0, complete: true);
            host.UpdateLayout();

            Assert.IsTrue(invoked);
            Assert.IsTrue(swipeControl.IsOpenForTesting);
            Assert.AreEqual(SwipeItemsPlacement.Left, swipeControl.OpenedItemsPlacementForTesting);
            Assert.IsTrue(swipeControl.HorizontalOffsetForTesting < 0);
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

    private static T? FindDescendantByName<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        return VisualTreeTestHelper
            .EnumerateDescendants(root)
            .OfType<T>()
            .FirstOrDefault(candidate => candidate.Name == name);
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");

        var dynamicResource = setter!.Value as DynamicResourceExtension;
        Assert.IsNotNull(dynamicResource, $"Expected {property.Name} to use a dynamic resource.");
        Assert.AreEqual(expectedResourceKey, dynamicResource!.ResourceKey);
    }

    private static Setter? FindSetter(Style style, DependencyProperty property)
    {
        for (var candidate = style; candidate != null; candidate = candidate.BasedOn)
        {
            var setter = candidate.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
            if (setter != null)
            {
                return setter;
            }
        }

        return null;
    }

    private static void AssertResourceAlias(FrameworkElement element, object resourceKey, object expectedResourceKey)
    {
        Assert.AreSame(
            element.TryFindResource(expectedResourceKey),
            element.TryFindResource(resourceKey),
            $"Unexpected resource alias for {resourceKey}.");
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
}
