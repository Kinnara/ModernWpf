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
            var presenter = VisualTreeTestHelper
                .EnumerateDescendants(item)
                .OfType<ContentPresenterEx>()
                .FirstOrDefault(candidate => ReferenceEquals(candidate.Content, content))
                ?? throw new AssertFailedException("Expected BreadcrumbBarItem template to use ContentPresenterEx for the item content.");

            Assert.AreSame(transitions, ControlHelper.GetContentTransitions(button));
            Assert.AreEqual(new CornerRadius(5), ControlHelper.GetCornerRadius(button));
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreSame(foreground, presenter.Foreground);
            Assert.AreEqual(HorizontalAlignment.Right, presenter.HorizontalAlignment);
            Assert.AreEqual(HorizontalAlignment.Right, presenter.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalContentAlignment);
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
