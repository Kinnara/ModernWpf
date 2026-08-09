using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.ItemsView;

[TestClass]
public class ItemsViewTests
{
    [TestMethod]
    public void MatchesCurrentWinUiApiDefaultsAndBringIntoViewOptions()
    {
        WpfTestHost.Run(() =>
        {
            var itemsView = new Controls.ItemsView();

            Assert.IsFalse(typeof(Controls.ItemsView).IsSealed);
            Assert.IsNull(itemsView.ItemsSource);
            Assert.IsNull(itemsView.ItemTemplate);
            Assert.IsNull(itemsView.Layout);
            Assert.IsNull(itemsView.ScrollView);
            Assert.IsNull(itemsView.VerticalScrollController);
            Assert.IsFalse(itemsView.IsItemInvokedEnabled);
            Assert.AreEqual(ItemsViewSelectionMode.Single, itemsView.SelectionMode);
            Assert.AreEqual(-1, itemsView.CurrentItemIndex);
            Assert.IsNull(itemsView.SelectedItem);
            Assert.AreEqual(0, itemsView.SelectedItems.Count);
            Assert.AreEqual(new CornerRadius(4), itemsView.CornerRadius);

            Assert.IsNotNull(Controls.ItemsView.ItemsSourceProperty);
            Assert.IsNotNull(Controls.ItemsView.ItemTemplateProperty);
            Assert.IsNotNull(Controls.ItemsView.LayoutProperty);
            Assert.IsNotNull(Controls.ItemsView.IsItemInvokedEnabledProperty);
            Assert.IsNotNull(Controls.ItemsView.SelectionModeProperty);
            Assert.IsNotNull(Controls.ItemsView.VerticalScrollControllerProperty);
            Assert.IsNotNull(Controls.ItemsView.CornerRadiusProperty);
            Assert.IsNotNull(Controls.ItemsView.ScrollViewProperty);
            Assert.IsNotNull(Controls.ItemsView.CurrentItemIndexProperty);
            Assert.IsNotNull(Controls.ItemsView.SelectedItemProperty);

            var options = new BringIntoViewOptions();
            Assert.IsFalse(options.AnimationDesired);
            Assert.IsNull(options.TargetRect);
            Assert.IsTrue(double.IsNaN(options.HorizontalAlignmentRatio));
            Assert.IsTrue(double.IsNaN(options.VerticalAlignmentRatio));
            Assert.AreEqual(0.0, options.HorizontalOffset);
            Assert.AreEqual(0.0, options.VerticalOffset);

            options.HorizontalAlignmentRatio = -4.0;
            options.VerticalAlignmentRatio = 7.0;
            Assert.AreEqual(0.0, options.HorizontalAlignmentRatio);
            Assert.AreEqual(1.0, options.VerticalAlignmentRatio);

            options.HorizontalAlignmentRatio = double.NaN;
            Assert.IsTrue(double.IsNaN(options.HorizontalAlignmentRatio));
            Assert.ThrowsExactly<ArgumentException>(() => options.HorizontalOffset = double.NaN);
            Assert.ThrowsExactly<ArgumentException>(() => options.VerticalOffset = double.PositiveInfinity);
        });
    }

    [TestMethod]
    public void AppliesDefaultTemplateAndWrapsDataItemsInItemContainers()
    {
        WpfTestHost.Run(() =>
        {
            var itemsView = CreateItemsView("Alpha", "Beta", "Gamma");

            using var host = new TestWindowHost(itemsView, width: 360, height: 240);
            host.UpdateLayout();

            Assert.IsInstanceOfType<StackLayout>(itemsView.Layout);
            Assert.IsNotNull(itemsView.ScrollView);
            Assert.IsNotNull(itemsView.ItemTemplate);

            var firstContainer = GetContainer(itemsView, 0);
            Assert.IsInstanceOfType<TextBlock>(firstContainer.Child);
            Assert.AreEqual("Alpha", ((TextBlock)firstContainer.Child).Text);
            Assert.AreEqual(1, System.Windows.Automation.AutomationProperties.GetPositionInSet(firstContainer));
            Assert.AreEqual(3, System.Windows.Automation.AutomationProperties.GetSizeOfSet(firstContainer));

            itemsView.ItemsSource = new[] { "One", "Two", "Three", "Four" };
            host.UpdateLayout();
            Assert.AreEqual(4, System.Windows.Automation.AutomationProperties.GetSizeOfSet(GetContainer(itemsView, 0)));
        });
    }

    [TestMethod]
    public void TracksSelectionAndAutomationMetadataAcrossCollectionChanges()
    {
        WpfTestHost.Run(() =>
        {
            var items = new ObservableCollection<string> { "Alpha", "Beta", "Gamma" };
            var itemsView = new Controls.ItemsView
            {
                Width = 300,
                Height = 180,
                ItemsSource = items,
                SelectionMode = ItemsViewSelectionMode.Multiple
            };

            using var host = new TestWindowHost(itemsView, width: 360, height: 240);
            host.UpdateLayout();

            itemsView.Select(1);
            items.Insert(0, "Zero");
            WpfTestHost.DoEvents();
            host.UpdateLayout();

            Assert.AreEqual("Beta", itemsView.SelectedItem);
            Assert.IsTrue(itemsView.IsSelected(2));
            Assert.AreEqual(3, System.Windows.Automation.AutomationProperties.GetPositionInSet(GetContainer(itemsView, 2)));
            Assert.AreEqual(4, System.Windows.Automation.AutomationProperties.GetSizeOfSet(GetContainer(itemsView, 2)));

            items.RemoveAt(2);
            WpfTestHost.DoEvents();
            host.UpdateLayout();

            Assert.IsNull(itemsView.SelectedItem);
            Assert.AreEqual(0, itemsView.SelectedItems.Count);
            Assert.AreEqual(3, System.Windows.Automation.AutomationProperties.GetSizeOfSet(GetContainer(itemsView, 0)));
        });
    }

    [TestMethod]
    public void UsesDirectItemContainerSourcesWithoutInventingATemplate()
    {
        WpfTestHost.Run(() =>
        {
            var first = new Controls.ItemContainer { Child = new TextBlock { Text = "Alpha" } };
            var second = new Controls.ItemContainer { Child = new TextBlock { Text = "Beta" } };
            var itemsView = new Controls.ItemsView
            {
                Width = 300,
                Height = 180,
                ItemsSource = new[] { first, second }
            };

            using var host = new TestWindowHost(itemsView, width: 360, height: 240);
            host.UpdateLayout();

            Assert.IsNull(itemsView.ItemTemplate);
            Assert.AreSame(first, GetContainer(itemsView, 0));
            Assert.AreSame(second, GetContainer(itemsView, 1));
            itemsView.Select(1);
            Assert.IsTrue(second.IsSelected);
        });
    }

    [TestMethod]
    public void ProgrammaticSelectionMaintainsItemsContainersAndEvents()
    {
        WpfTestHost.Run(() =>
        {
            var itemsView = CreateItemsView("Alpha", "Beta", "Gamma", "Delta");
            itemsView.SelectionMode = ItemsViewSelectionMode.Multiple;
            int selectionChangedCount = 0;
            itemsView.SelectionChanged += (_, _) => selectionChangedCount++;

            using var host = new TestWindowHost(itemsView, width: 360, height: 260);
            host.UpdateLayout();

            itemsView.Select(1);
            itemsView.Select(3);
            Assert.IsTrue(itemsView.IsSelected(1));
            Assert.IsTrue(itemsView.IsSelected(3));
            Assert.AreEqual("Beta", itemsView.SelectedItem);
            CollectionAssert.AreEquivalent(
                new object[] { "Beta", "Delta" },
                itemsView.SelectedItems.ToArray());
            Assert.IsTrue(GetContainer(itemsView, 1).IsSelected);
            Assert.IsTrue(GetContainer(itemsView, 3).IsSelected);

            itemsView.Deselect(1);
            Assert.IsFalse(GetContainer(itemsView, 1).IsSelected);
            CollectionAssert.AreEqual(new object[] { "Delta" }, itemsView.SelectedItems.ToArray());

            itemsView.SelectAll();
            Assert.AreEqual(4, itemsView.SelectedItems.Count);
            itemsView.InvertSelection();
            Assert.AreEqual(0, itemsView.SelectedItems.Count);

            itemsView.Select(2);
            itemsView.SelectionMode = ItemsViewSelectionMode.None;
            Assert.AreEqual(0, itemsView.SelectedItems.Count);
            Assert.IsFalse(GetContainer(itemsView, 2).IsSelected);
            Assert.IsGreaterThanOrEqualTo(6, selectionChangedCount);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => itemsView.Select(4));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => itemsView.Deselect(-1));
        });
    }

    [TestMethod]
    public void ItemInteractionsFollowSelectionAndInvocationModes()
    {
        WpfTestHost.Run(() =>
        {
            var itemsView = CreateItemsView("Alpha", "Beta", "Gamma");
            itemsView.SelectionMode = ItemsViewSelectionMode.Multiple;
            itemsView.IsItemInvokedEnabled = true;
            object? invokedItem = null;
            int invocationCount = 0;
            itemsView.ItemInvoked += (_, args) =>
            {
                invokedItem = args.InvokedItem;
                invocationCount++;
            };

            using var host = new TestWindowHost(itemsView, width: 360, height: 240);
            host.UpdateLayout();

            var first = GetContainer(itemsView, 0);
            first.RaiseItemInvoked(ItemContainerInteractionTrigger.MouseReleased, first);
            Assert.IsTrue(itemsView.IsSelected(0));
            Assert.AreEqual(0, itemsView.CurrentItemIndex);
            Assert.AreEqual(0, invocationCount);

            first.RaiseItemInvoked(ItemContainerInteractionTrigger.MouseReleased, first);
            Assert.IsFalse(itemsView.IsSelected(0));

            first.RaiseItemInvoked(ItemContainerInteractionTrigger.DoubleClick, first);
            Assert.AreEqual("Alpha", invokedItem);
            Assert.AreEqual(1, invocationCount);

            var second = GetContainer(itemsView, 1);
            second.RaiseItemInvoked(ItemContainerInteractionTrigger.EnterKey, second);
            Assert.IsTrue(itemsView.IsSelected(1));
            Assert.AreEqual("Beta", invokedItem);
            Assert.AreEqual(2, invocationCount);

            second.RaiseItemInvoked(ItemContainerInteractionTrigger.SpaceKey, second);
            Assert.IsFalse(itemsView.IsSelected(1));
            Assert.AreEqual(2, invocationCount);

            itemsView.SelectionMode = ItemsViewSelectionMode.None;
            first.RaiseItemInvoked(ItemContainerInteractionTrigger.MouseReleased, first);
            Assert.AreEqual("Alpha", invokedItem);
            Assert.AreEqual(3, invocationCount);
            Assert.AreEqual(0, itemsView.SelectedItems.Count);
        });
    }

    [TestMethod]
    public void AutomationPeerExposesRealizedSelectionAndOptInItemInvocation()
    {
        WpfTestHost.Run(() =>
        {
            var itemsView = CreateItemsView("Alpha", "Beta", "Gamma");
            itemsView.SelectionMode = ItemsViewSelectionMode.Multiple;
            itemsView.IsItemInvokedEnabled = true;

            using var host = new TestWindowHost(itemsView, width: 360, height: 240);
            host.UpdateLayout();

            itemsView.Select(1);
            var peer = new ItemsViewAutomationPeer(itemsView);
            Assert.AreEqual(AutomationControlType.List, peer.GetAutomationControlType());
            Assert.AreEqual(nameof(Controls.ItemsView), peer.GetClassName());
            Assert.AreSame(peer, peer.GetPattern(PatternInterface.Selection));

            var selectionProvider = (ISelectionProvider)peer.GetPattern(PatternInterface.Selection)!;
            Assert.IsTrue(selectionProvider.CanSelectMultiple);
            Assert.IsFalse(selectionProvider.IsSelectionRequired);
            Assert.AreEqual(1, selectionProvider.GetSelection().Length);

            object? invokedItem = null;
            itemsView.ItemInvoked += (_, args) => invokedItem = args.InvokedItem;
            var itemPeer = new ItemContainerAutomationPeer(GetContainer(itemsView, 2));
            ((IInvokeProvider)itemPeer.GetPattern(PatternInterface.Invoke)!).Invoke();
            Assert.AreEqual("Gamma", invokedItem);

            itemsView.SelectionMode = ItemsViewSelectionMode.Single;
            Assert.IsFalse(selectionProvider.CanSelectMultiple);
            itemsView.SelectionMode = ItemsViewSelectionMode.None;
            Assert.IsNull(peer.GetPattern(PatternInterface.Selection));
        });
    }

    [TestMethod]
    public void SupportsItemContainerTemplatesAndRejectsOtherRoots()
    {
        WpfTestHost.Run(() =>
        {
            var valid = CreateItemsView("Alpha", "Beta");
            valid.ItemTemplate = (DataTemplate)XamlReader.Parse(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
                "xmlns:ui='clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls'>" +
                "<ui:ItemContainer><TextBlock Text='{Binding}' /></ui:ItemContainer>" +
                "</DataTemplate>");

            using (var host = new TestWindowHost(valid, width: 320, height: 200))
            {
                host.UpdateLayout();
                Assert.AreEqual("Alpha", ((TextBlock)GetContainer(valid, 0).Child).Text);
            }

            var invalid = CreateItemsView("Alpha");
            invalid.ItemTemplate = (DataTemplate)XamlReader.Parse(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<TextBlock Text='{Binding}' />" +
                "</DataTemplate>");

            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                using var host = new TestWindowHost(invalid, width: 320, height: 200);
                host.UpdateLayout();
            });
        });
    }

    [TestMethod]
    public void BringsItemsIntoViewAndFindsTheViewportItem()
    {
        WpfTestHost.Run(() =>
        {
            var itemsView = new Controls.ItemsView
            {
                Width = 320,
                Height = 180,
                ItemsSource = Enumerable.Range(0, 200).Select(index => $"Item {index}").ToArray()
            };

            using var host = new TestWindowHost(itemsView, width: 380, height: 240);
            host.UpdateLayout();

            itemsView.StartBringItemIntoView(1, new BringIntoViewOptions());
            host.UpdateLayout();
            Assert.AreEqual(0.0, itemsView.ScrollView!.VerticalOffset, 0.5);

            itemsView.StartBringItemIntoView(
                150,
                new BringIntoViewOptions
                {
                    VerticalAlignmentRatio = 0.5,
                    TargetRect = new Rect(0, 0, 20, 10)
                });
            host.UpdateLayout();

            Assert.IsGreaterThan(0.0, itemsView.ScrollView.VerticalOffset);
            Assert.IsNotNull(GetContainer(itemsView, 150));
            Assert.IsTrue(itemsView.TryGetItemIndex(0.5, 0.5, out int viewportIndex));
            Assert.IsGreaterThanOrEqualTo(0, viewportIndex);
            Assert.IsLessThan(200, viewportIndex);
        });
    }

    private static Controls.ItemsView CreateItemsView(params string[] items)
    {
        return new Controls.ItemsView
        {
            Width = 300,
            Height = 180,
            ItemsSource = items
        };
    }

    private static Controls.ItemContainer GetContainer(Controls.ItemsView itemsView, int index)
    {
        var repeater = (ItemsRepeater)itemsView.Template.FindName(
            Controls.ItemsView.ItemsRepeaterPartName,
            itemsView);
        return (Controls.ItemContainer)repeater.GetOrCreateElement(index);
    }
}
