using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.Media.Animation;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.BreadcrumbBar;

[TestClass]
public class BreadcrumbBarApiTests
{
    [TestMethod]
    public void VerifyBreadcrumbDefaultAPIValues()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar();

            Assert.IsNull(breadcrumb.ItemsSource);
            Assert.IsNull(breadcrumb.ItemTemplate);
        });
    }

    [TestMethod]
    public void VerifyDefaultBreadcrumb()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar();

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            Assert.AreEqual(0, breadcrumb.Containers.Count);
        });
    }

    [TestMethod]
    public void BreadcrumbBarItemAcceptsWinUIContentPresenterSurface()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new TransitionCollection();
            var item = new ModernWpf.Controls.BreadcrumbBarItem
            {
                ContentTransitions = transitions,
                CornerRadius = new CornerRadius(4)
            };

            Assert.AreSame(transitions, item.ContentTransitions);
            Assert.AreEqual(new CornerRadius(4), item.CornerRadius);
        });
    }

    [TestMethod]
    public void BreadcrumbBarItemTemplateUsesWinUIContentPresenter()
    {
        WpfTestHost.Run(() =>
        {
            var content = new Border { Width = 80, Height = 24 };
            var transitions = new TransitionCollection();
            var foreground = new SolidColorBrush(Colors.Blue);
            var item = new ModernWpf.Controls.BreadcrumbBarItem
            {
                Content = content,
                ContentTransitions = transitions,
                CornerRadius = new CornerRadius(5),
                Foreground = foreground,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Bottom
            };

            using var host = new TestWindowHost(item, width: 240, height: 80);

            var button = VisualTreeTestHelper
                .EnumerateDescendants(item)
                .OfType<Button>()
                .FirstOrDefault()
                ?? throw new AssertFailedException("Expected BreadcrumbBarItem template to contain an item button.");
            var presenter = FindTemplatePart<ContentPresenterEx>(item, "PART_ItemContentPresenter");

            Assert.AreSame(transitions, ControlHelper.GetContentTransitions(button));
            Assert.AreEqual(new CornerRadius(5), ControlHelper.GetCornerRadius(button));
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreEqual(HorizontalAlignment.Right, presenter.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalContentAlignment);
        });
    }

    [TestMethod]
    public void BreadcrumbBarItemTemplateUsesVisualStateSettersForWinUIStateParity()
    {
        WpfTestHost.Run(() =>
        {
            var item = new ModernWpf.Controls.BreadcrumbBarItem
            {
                Content = "Node"
            };

            using var host = new TestWindowHost(item, width: 240, height: 80);
            host.UpdateLayout();

            var root = FindTemplatePart<FrameworkElement>(item, "PART_LayoutRoot");
            var itemButton = FindTemplatePart<Button>(item, "PART_ItemButton");
            var itemButtonRoot = VisualTreeTestHelper
                .EnumerateDescendants(itemButton)
                .OfType<FrameworkElement>()
                .FirstOrDefault(element => FindVisualStateGroup(element, "CommonStates") != null)
                ?? throw new AssertFailedException("Expected BreadcrumbBarItem button template to contain CommonStates.");
            AssertStateSetter(root, "ItemTypeStates", "EllipsisDropDown", "PART_ItemButton.Visibility");
            AssertStateSetter(root, "ItemTypeStates", "EllipsisDropDown", "PART_EllipsisDropDownItemContentPresenter.Visibility");
            AssertStateSetter(root, "InlineItemTypeStates", "Default", "PART_ChevronTextBlock.Text");
            AssertStateSetter(root, "InlineItemTypeStates", "DefaultRTL", "PART_ChevronTextBlock.Text");
            AssertStateSetter(root, "InlineItemTypeStates", "LastItem", "PART_ItemButton.Visibility");
            AssertStateSetter(root, "InlineItemTypeStates", "LastItem", "PART_ChevronTextBlock.Visibility");
            AssertStateSetter(root, "InlineItemTypeStates", "LastItem", "PART_LastItemContentPresenter.Visibility");
            AssertStateSetter(root, "InlineItemTypeStates", "Ellipsis", "PART_EllipsisTextBlock.Visibility");
            AssertStateSetter(root, "InlineItemTypeStates", "EllipsisRTL", "PART_ChevronTextBlock.Text");

            AssertStateSetter(itemButtonRoot, "CommonStates", "CurrentNormal", "PART_ContentPresenter.Foreground");
            AssertStateSetter(itemButtonRoot, "CommonStates", "PointerOver", "PART_ContentPresenter.Foreground");
            AssertStateSetter(itemButtonRoot, "CommonStates", "PointerOver", "PART_ContentPresenter.Background");
            AssertStateSetter(itemButtonRoot, "CommonStates", "PointerOver", "PART_ContentPresenter.BorderBrush");
            AssertStateSetter(itemButtonRoot, "CommonStates", "Pressed", "PART_ContentPresenter.Foreground");
            AssertStateSetter(itemButtonRoot, "CommonStates", "Pressed", "PART_ContentPresenter.Background");
            AssertStateSetter(itemButtonRoot, "CommonStates", "Pressed", "PART_ContentPresenter.BorderBrush");
            AssertStateSetter(itemButtonRoot, "CommonStates", "Disabled", "PART_ContentPresenter.Foreground");
            AssertStateSetter(itemButtonRoot, "CommonStates", "Focus", "PART_ContentPresenter.Foreground");

            Assert.IsTrue(VisualStateManager.GoToState(itemButton, "Pressed", false));

            item.IsCurrentItem = true;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, FindTemplatePart<Button>(item, "PART_ItemButton").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindTemplatePart<TextBlock>(item, "PART_ChevronTextBlock").Visibility);
            var lastItemPresenter = FindTemplatePart<ContentPresenterEx>(item, "PART_LastItemContentPresenter");
            Assert.AreEqual(Visibility.Visible, lastItemPresenter.Visibility);
            Assert.AreEqual(FontWeights.Normal, lastItemPresenter.FontWeight);
        });
    }

    [TestMethod]
    public void BreadcrumbBarTemplateUsesWinUIItemsRepeater()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[] { "Root", "Node A", "Node B" }
            };

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            var repeater = FindTemplatePart<ItemsRepeater>(breadcrumb, "PART_ItemsRepeater");

            Assert.IsNotNull(repeater);
            Assert.IsInstanceOfType(repeater.Layout, typeof(NonVirtualizingLayout));
            Assert.IsNull(breadcrumb.Template?.FindName("PART_RootPanel", breadcrumb));
        });
    }

    [TestMethod]
    public void VerifyItemsSourceCreatesBreadcrumbBarItems()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[] { "Root", "Node A", "Node B" }
            };

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            Assert.AreEqual(3, breadcrumb.Containers.Count);
            Assert.AreEqual("Root", breadcrumb.ContainerFromIndex(0).Content);
            Assert.IsFalse(breadcrumb.ContainerFromIndex(0).IsCurrentItem);
            Assert.IsTrue(breadcrumb.ContainerFromIndex(2).IsCurrentItem);
            Assert.AreEqual(1, breadcrumb.ContainerFromIndex(0).GetValue(AutomationProperties.PositionInSetProperty));
            Assert.AreEqual(3, breadcrumb.ContainerFromIndex(0).GetValue(AutomationProperties.SizeOfSetProperty));
        });
    }

    [TestMethod]
    public void VerifyConstrainedWidthUsesWinUIEllipsisElement()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[]
                {
                    "Very long root node",
                    "Very long child node",
                    "Current node"
                }
            };

            using var host = new TestWindowHost(breadcrumb, width: 110, height: 80);
            host.UpdateLayout();

            var repeater = FindTemplatePart<ItemsRepeater>(breadcrumb, "PART_ItemsRepeater");
            var ellipsis = repeater.TryGetElement(0) as BreadcrumbBarItem;
            var hiddenElements = breadcrumb.HiddenElements();

            Assert.IsNotNull(ellipsis);
            Assert.IsTrue(hiddenElements.Count > 0);
            Assert.AreEqual("Very long root node", hiddenElements[0]);
            Assert.AreEqual(3, breadcrumb.Containers.Count);
        });
    }

    [TestMethod]
    public void VerifyCustomItemTemplate()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[]
                {
                    new MockNode { Name = "Root" },
                    new MockNode { Name = "Node A" }
                },
                ItemTemplate = CreateNameTemplate()
            };

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            var textBlocks = VisualTreeTestHelper
                .EnumerateDescendants(breadcrumb)
                .OfType<TextBlock>()
                .Where(textBlock => textBlock.Text == "Root" || textBlock.Text == "Node A")
                .ToList();

            Assert.AreEqual(2, textBlocks.Count);
        });
    }

    [TestMethod]
    public void VerifyItemClickedEventArgs()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[] { "Root", "Node A", "Node B" }
            };

            object? clickedItem = null;
            var clickedIndex = -1;
            breadcrumb.ItemClicked += (sender, args) =>
            {
                clickedItem = args.Item;
                clickedIndex = args.Index;
            };

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            RaiseItemButtonClick(breadcrumb.ContainerFromIndex(1));

            Assert.AreEqual("Node A", clickedItem);
            Assert.AreEqual(1, clickedIndex);

            clickedItem = null;
            clickedIndex = -1;

            RaiseItemButtonClick(breadcrumb.ContainerFromIndex(2));

            Assert.IsNull(clickedItem);
            Assert.AreEqual(-1, clickedIndex);
        });
    }

    [TestMethod]
    public void VerifyAutomationInvokePattern()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[] { "Root", "Node A" }
            };

            object? clickedItem = null;
            breadcrumb.ItemClicked += (sender, args) => clickedItem = args.Item;

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(breadcrumb.ContainerFromIndex(0));
            var invokeProvider = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);
            invokeProvider.Invoke();

            Assert.AreEqual("Root", clickedItem);
        });
    }

    [TestMethod]
    public void VerifyCollectionChangeGetsRespected()
    {
        WpfTestHost.Run(() =>
        {
            var items = new ObservableCollection<string> { "Root", "Node A" };
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = items
            };

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            Assert.AreEqual(2, breadcrumb.Containers.Count);

            items.Add("Node B");
            host.UpdateLayout();

            Assert.AreEqual(3, breadcrumb.Containers.Count);
            Assert.AreEqual("Node B", breadcrumb.ContainerFromIndex(2).Content);
        });
    }

    private static void RaiseItemButtonClick(BreadcrumbBarItem item)
    {
        var button = VisualTreeTestHelper
            .EnumerateDescendants(item)
            .OfType<Button>()
            .FirstOrDefault();

        if (button == null)
        {
            Assert.Fail("Could not find breadcrumb item button.");
            throw new AssertFailedException();
        }

        button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : class
    {
        control.ApplyTemplate();

        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Could not find template part '{name}'.");
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string expectedTarget)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        Assert.IsNotNull(group, $"Expected visual state group '{groupName}'.");

        var state = FindVisualState(group!, stateName);
        Assert.IsNotNull(state, $"Expected visual state '{groupName}.{stateName}'.");
        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        foreach (VisualStateSetter setter in stateEx.Setters)
        {
            if (setter.Target == expectedTarget)
            {
                return;
            }
        }

        Assert.Fail($"Expected visual state '{groupName}.{stateName}' to contain setter '{expectedTarget}'.");
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

    private static DataTemplate CreateNameTemplate()
    {
        var template = new DataTemplate(typeof(MockNode));
        var textBlock = new FrameworkElementFactory(typeof(TextBlock));
        textBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(MockNode.Name)));
        template.VisualTree = textBlock;
        return template;
    }

    private sealed class MockNode
    {
        public string Name { get; set; } = string.Empty;
    }
}
