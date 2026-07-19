using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.SelectorBar;

[TestClass]
public class SelectorBarApiTests
{
    [TestMethod]
    public void VerifyDefaultSelectorBarItemPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBarItem = new SelectorBarItem();

            Assert.AreEqual(string.Empty, selectorBarItem.Text);
            Assert.IsNull(selectorBarItem.Icon);
            Assert.IsNull(selectorBarItem.Child);
            Assert.IsFalse(selectorBarItem.IsSelected);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, selectorBarItem.BackgroundSizing);
            Assert.AreEqual(new CornerRadius(), selectorBarItem.CornerRadius);
        });
    }

    [TestMethod]
    public void VerifyDefaultSelectorBarPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBar = new ModernWpf.Controls.SelectorBar();

            Assert.IsNotNull(selectorBar.Items);
            Assert.AreEqual(0, selectorBar.Items.Count);
            Assert.IsNull(selectorBar.SelectedItem);
            Assert.IsFalse(selectorBar.Focusable);
            Assert.AreEqual(KeyboardNavigationMode.Once, KeyboardNavigation.GetTabNavigation(selectorBar));
        });
    }

    [TestMethod]
    public void SelectorBarItemTemplateUsesWinUIPresenterSurface()
    {
        WpfTestHost.Run(() =>
        {
            var icon = new SymbolIcon(Symbol.Delete);
            var child = new Border { Width = 20, Height = 12 };
            var foreground = new SolidColorBrush(Colors.Blue);
            var item = new SelectorBarItem
            {
                Text = "Deleted",
                Icon = icon,
                Child = child,
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                CornerRadius = new CornerRadius(5),
                Foreground = foreground
            };

            using var host = new TestWindowHost(item, width: 240, height: 80);

            var root = GetNamedDescendant<GridEx>(item, "PART_ContainerRoot");
            var iconPresenter = GetNamedDescendant<ContentPresenterEx>(item, "PART_IconVisual");
            var textVisual = GetNamedDescendant<TextBlock>(item, "PART_TextVisual");
            var selectionVisual = GetNamedDescendant<Rectangle>(item, "PART_SelectionVisual");
            var commonVisual = GetNamedDescendant<Rectangle>(item, "PART_CommonVisual");
            var contentStack = VisualTreeTestHelper
                .EnumerateDescendants(item)
                .OfType<StackPanelEx>()
                .FirstOrDefault()
                ?? throw new AssertFailedException("Expected SelectorBarItem template to use StackPanelEx for source spacing.");

            Assert.IsFalse(
                VisualTreeTestHelper.EnumerateDescendants(item).OfType<Button>().Any(),
                "SelectorBarItem should not keep the old WPF button wrapper.");
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, root.BackgroundSizing);
            Assert.AreEqual(new CornerRadius(5), root.CornerRadius);
            Assert.AreSame(icon, iconPresenter.Content);
            Assert.AreEqual("Deleted", textVisual.Text);
            Assert.AreSame(foreground, iconPresenter.Foreground);
            Assert.AreSame(foreground, textVisual.Foreground);
            Assert.IsInstanceOfType(textVisual.RenderTransform, typeof(TranslateTransform));
            Assert.AreEqual(-1.0, ((TranslateTransform)textVisual.RenderTransform).Y);
            Assert.AreEqual(8.0, contentStack.Spacing);
            Assert.AreEqual(0.0, selectionVisual.Opacity);
            Assert.AreEqual(1.0, commonVisual.StrokeThickness);
            Assert.IsTrue(root.Children.IndexOf(commonVisual) < root.Children.IndexOf(contentStack));
            Assert.IsTrue(root.Children.IndexOf(commonVisual) < root.Children.IndexOf(selectionVisual));
        });
    }

    [TestMethod]
    public void SelectorBarStylesUseWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            var resources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/SelectorBar/SelectorBar.xaml", UriKind.Relative)
            };
            var selectorBarStyle = (Style)resources[typeof(ModernWpf.Controls.SelectorBar)];
            var itemStyle = (Style)resources[typeof(SelectorBarItem)];
            var pillStyle = (Style)resources["SelectorBarItemPill"];
            var item = new SelectorBarItem
            {
                Text = "Recent",
                Icon = new SymbolIcon(Symbol.Clock),
                Style = itemStyle
            };
            var selectorBar = new ModernWpf.Controls.SelectorBar
            {
                Style = selectorBarStyle
            };
            item.Resources.MergedDictionaries.Add(resources);
            selectorBar.Resources.MergedDictionaries.Add(resources);

            var testRoot = new StackPanel();
            testRoot.Children.Add(selectorBar);
            testRoot.Children.Add(item);

            using var host = new TestWindowHost(testRoot, width: 240, height: 120);
            host.UpdateLayout();

            Assert.AreEqual(typeof(ModernWpf.Controls.SelectorBar), selectorBarStyle.TargetType);
            AssertSetterValue(selectorBarStyle, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            AssertSetterValue(selectorBarStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Top);
            AssertSetterValue(selectorBarStyle, Control.IsTabStopProperty, false);
            AssertSetterValue(selectorBarStyle, Control.PaddingProperty, selectorBar.TryFindResource("SelectorBarPadding"));
            AssertDynamicResourceSetter(selectorBarStyle, Control.BackgroundProperty, "SelectorBarBackground");
            AssertSetterValue(selectorBarStyle, Control.BorderThicknessProperty, selectorBar.TryFindResource("SelectorBarBorderThickness"));
            AssertDynamicResourceSetter(selectorBarStyle, ModernWpf.Controls.SelectorBar.CornerRadiusProperty, "ControlCornerRadius");
            Assert.IsInstanceOfType(GetSetterValue(selectorBarStyle, Control.TemplateProperty), typeof(ControlTemplate));

            Assert.AreSame(selectorBar.TryFindResource("SelectorBarBackground"), selectorBar.Background);
            Assert.AreEqual(selectorBar.TryFindResource("SelectorBarPadding"), selectorBar.Padding);
            Assert.AreEqual(selectorBar.TryFindResource("SelectorBarBorderThickness"), selectorBar.BorderThickness);
            Assert.AreEqual(selectorBar.TryFindResource("ControlCornerRadius"), selectorBar.CornerRadius);

            Assert.AreEqual(typeof(SelectorBarItem), itemStyle.TargetType);
            AssertSetterValue(itemStyle, SelectorBarItem.BackgroundSizingProperty, BackgroundSizing.OuterBorderEdge);
            AssertDynamicResourceSetter(itemStyle, Control.ForegroundProperty, "SelectorBarItemForeground");
            AssertDynamicResourceSetter(itemStyle, Control.BorderBrushProperty, "SelectorBarItemBorderBrush");
            AssertSetterValue(itemStyle, Control.BorderThicknessProperty, item.TryFindResource("SelectorBarItemBorderThickness"));
            AssertSetterValue(itemStyle, Control.PaddingProperty, item.TryFindResource("SelectorBarItemPadding"));
            AssertDynamicResourceSetter(itemStyle, SelectorBarItem.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetterValue(itemStyle, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            AssertSetterValue(itemStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(itemStyle, SelectorBarItem.FocusVisualMarginProperty, item.TryFindResource("SelectorBarItemFocusVisualMargin"));
            AssertDynamicResourceSetter(itemStyle, Control.FontFamilyProperty, "ContentControlThemeFontFamily");
            AssertSetterValue(itemStyle, Control.FontWeightProperty, FontWeights.Normal);
            AssertDynamicResourceSetter(itemStyle, Control.FontSizeProperty, "ControlContentThemeFontSize");
            AssertDynamicResourceSetter(itemStyle, SelectorBarItem.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
            AssertSetterValue(itemStyle, UIElement.FocusableProperty, true);
            Assert.IsInstanceOfType(GetSetterValue(itemStyle, Control.TemplateProperty), typeof(ControlTemplate));
            AssertDynamicResourceSetter(itemStyle, Control.BackgroundProperty, "SelectorBarItemBackground");

            Assert.AreEqual(typeof(Rectangle), pillStyle.TargetType);
            AssertDynamicResourceSetter(pillStyle, Shape.FillProperty, "SelectorBarItemPillFill");
            AssertSetterValue(pillStyle, FrameworkElement.HeightProperty, item.TryFindResource("SelectorBarItemPillHeight"));
            AssertSetterValue(pillStyle, FrameworkElement.WidthProperty, item.TryFindResource("SelectorBarItemPillWidth"));
            AssertSetterValue(pillStyle, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetterValue(pillStyle, UIElement.RenderTransformOriginProperty, new Point(0.5, 0.5));

            AssertResourceAlias(item, "SelectorBarBackground", "SystemControlTransparentBrush");
            AssertResourceAlias(item, "SelectorBarItemPillFill", "AccentFillColorDefaultBrush");
            AssertResourceAlias(item, "SelectorBarItemDisabledPillFill", "AccentFillColorDisabledBrush");
            AssertResourceAlias(item, "SelectorBarItemBorderBrush", "SystemControlTransparentBrush");
            AssertResourceAlias(item, "SelectorBarItemBorderBrushPointerOver", "SystemControlTransparentBrush");
            AssertResourceAlias(item, "SelectorBarItemBorderBrushSelected", "SystemControlTransparentBrush");
            AssertResourceAlias(item, "SelectorBarItemBorderBrushPressed", "SystemControlTransparentBrush");
            AssertResourceAlias(item, "SelectorBarItemBorderBrushDisabled", "SystemControlTransparentBrush");
            AssertResourceAlias(item, "SelectorBarItemForeground", "TextFillColorPrimaryBrush");
            AssertResourceAlias(item, "SelectorBarItemForegroundPointerOver", "TextFillColorSecondaryBrush");
            AssertResourceAlias(item, "SelectorBarItemForegroundSelected", "TextFillColorPrimaryBrush");
            AssertResourceAlias(item, "SelectorBarItemForegroundPressed", "TextFillColorTertiaryBrush");
            AssertResourceAlias(item, "SelectorBarItemForegroundDisabled", "TextFillColorDisabledBrush");
            AssertResourceAlias(item, "SelectorBarItemBackground", "SystemControlTransparentBrush");
            AssertResourceAlias(item, "SelectorBarItemBackgroundPointerOver", "SystemControlTransparentBrush");
            AssertResourceAlias(item, "SelectorBarItemBackgroundSelected", "SystemControlTransparentBrush");
            AssertResourceAlias(item, "SelectorBarItemBackgroundPressed", "SystemControlTransparentBrush");
            AssertResourceAlias(item, "SelectorBarItemBackgroundDisabled", "SystemControlTransparentBrush");

            Assert.AreEqual(new Thickness(0, 4, 0, 4), item.TryFindResource("SelectorBarPadding"));
            Assert.AreEqual(new Thickness(1), item.TryFindResource("SelectorBarBorderThickness"));
            Assert.AreEqual(new Thickness(1), item.TryFindResource("SelectorBarItemBorderThickness"));
            Assert.AreEqual(new Thickness(1), item.TryFindResource("SelectorBarSelectedInnerThickness"));
            Assert.AreEqual(new Thickness(-2, 0, -2, 0), item.TryFindResource("SelectorBarItemIconVisualMargin"));
            Assert.AreEqual(new Thickness(0), item.TryFindResource("SelectorBarItemTextVisualMargin"));
            Assert.AreEqual(new Thickness(12, 10, 12, 7), item.TryFindResource("SelectorBarItemPadding"));
            Assert.AreEqual(new Thickness(0), item.TryFindResource("SelectorBarItemSelectionVisualMargin"));
            Assert.AreEqual(new Thickness(-2), item.TryFindResource("SelectorBarItemFocusVisualMargin"));
            Assert.AreEqual(3.0, item.TryFindResource("SelectorBarItemPillHeight"));
            Assert.AreEqual(4.0, item.TryFindResource("SelectorBarItemPillWidth"));
            Assert.AreEqual(0.8, item.TryFindResource("SelectorBarItemIconScale"));
            Assert.AreEqual(8.0, item.TryFindResource("SelectorBarItemSpacing"));

            Assert.AreSame(item.TryFindResource("SelectorBarItemForeground"), item.Foreground);
            Assert.AreSame(item.TryFindResource("SelectorBarItemBorderBrush"), item.BorderBrush);
            Assert.AreEqual(new Thickness(1), item.BorderThickness);
            Assert.AreEqual(new Thickness(12, 10, 12, 7), item.Padding);
            Assert.AreEqual(item.TryFindResource("ControlCornerRadius"), item.CornerRadius);
            Assert.AreEqual(new Thickness(-2), item.FocusVisualMargin);
            Assert.AreSame(item.TryFindResource("ContentControlThemeFontFamily"), item.FontFamily);
            Assert.AreEqual(item.TryFindResource("ControlContentThemeFontSize"), item.FontSize);
            Assert.AreEqual(item.TryFindResource("UseSystemFocusVisuals"), item.UseSystemFocusVisuals);
            Assert.AreSame(item.TryFindResource("SelectorBarItemBackground"), item.Background);

            var root = GetNamedDescendant<GridEx>(item, "PART_ContainerRoot");
            var iconPresenter = GetNamedDescendant<ContentPresenterEx>(item, "PART_IconVisual");
            var textVisual = GetNamedDescendant<TextBlock>(item, "PART_TextVisual");
            var selectionVisual = GetNamedDescendant<Rectangle>(item, "PART_SelectionVisual");
            var commonVisual = GetNamedDescendant<Rectangle>(item, "PART_CommonVisual");
            var iconScale = (ScaleTransform)iconPresenter.RenderTransform;

            Assert.AreSame(item.TryFindResource("SelectorBarItemForeground"), iconPresenter.Foreground);
            Assert.AreSame(item.TryFindResource("SelectorBarItemForeground"), textVisual.Foreground);
            Assert.AreEqual(new Thickness(-2, 0, -2, 0), iconPresenter.Margin);
            Assert.AreEqual(0.8, iconScale.ScaleX);
            Assert.AreEqual(0.8, iconScale.ScaleY);
            Assert.AreSame(pillStyle, selectionVisual.Style);
            Assert.AreSame(item.TryFindResource("SelectorBarItemPillFill"), selectionVisual.Fill);
            Assert.AreEqual(3.0, selectionVisual.Height);
            Assert.AreEqual(4.0, selectionVisual.Width);
            Assert.AreSame(item.TryFindResource("SelectorBarItemBackground"), commonVisual.Fill);
            Assert.AreSame(item.TryFindResource("SelectorBarItemBorderBrush"), commonVisual.Stroke);
            Assert.AreEqual(1.0, commonVisual.StrokeThickness);

            AssertStateSetterDynamicResource(root, "CombinedStates", "UnselectedPointerOver", "PART_ContainerRoot.Background", "SelectorBarItemBackgroundPointerOver");
            AssertStateSetterDynamicResource(root, "CombinedStates", "UnselectedPointerOver", "PART_TextVisual.Foreground", "SelectorBarItemForegroundPointerOver");
            AssertStateSetterDynamicResource(root, "CombinedStates", "UnselectedPressed", "PART_ContainerRoot.Background", "SelectorBarItemBackgroundPressed");
            AssertStateSetterDynamicResource(root, "CombinedStates", "SelectedNormal", "PART_ContainerRoot.Background", "SelectorBarItemBackgroundSelected");
            AssertStateSetterDynamicResource(root, "CombinedStates", "SelectedNormal", "PART_TextVisual.Foreground", "SelectorBarItemForegroundSelected");
            AssertStateSetterDynamicResource(root, "DisabledStates", "Disabled", "PART_ContainerRoot.Background", "SelectorBarItemBackgroundDisabled");
            AssertStateSetterDynamicResource(root, "DisabledStates", "Disabled", "PART_TextVisual.Foreground", "SelectorBarItemForegroundDisabled");
            AssertStateSetterDynamicResource(root, "DisabledStates", "Disabled", "PART_SelectionVisual.Fill", "SelectorBarItemDisabledPillFill");
        });
    }

    [TestMethod]
    public void SelectorBarTemplateHostsItemsAsSelectorBarItemContainers()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBar = new ModernWpf.Controls.SelectorBar();
            var recent = new SelectorBarItem { Text = "Recent" };
            var shared = new SelectorBarItem { Text = "Shared" };
            selectorBar.Items.Add(recent);
            selectorBar.Items.Add(shared);

            using var host = new TestWindowHost(selectorBar, width: 400, height: 120);

            var itemsView = GetNamedDescendant<SelectorBarItemsControl>(selectorBar, "PART_ItemsView");
            var itemsViewPeer = FrameworkElementAutomationPeer.CreatePeerForElement(itemsView);
            var itemPeer = itemsViewPeer.GetChildren()
                .OfType<ModernWpf.Controls.SelectorBarItemsControlItemAutomationPeer>()
                .Single(peer => peer.GetName() == "Shared");
            var selectionItemProvider = (ISelectionItemProvider)itemPeer.GetPattern(PatternInterface.SelectionItem);
            selectionItemProvider.Select();

            Assert.AreSame(shared, selectorBar.SelectedItem);
            Assert.AreEqual("ItemsView", itemsViewPeer.GetClassName());
            Assert.AreEqual(AutomationControlType.List, itemsViewPeer.GetAutomationControlType());
            Assert.IsInstanceOfType(itemsViewPeer.GetPattern(PatternInterface.Selection), typeof(ISelectionProvider));
            Assert.AreEqual(AutomationControlType.ListItem, itemPeer.GetAutomationControlType());
            Assert.AreEqual("SelectorBarItem", itemPeer.GetLocalizedControlType());
            Assert.IsNotNull(selectionItemProvider.SelectionContainer);
        });
    }

    [TestMethod]
    public void VerifySelectorBarItems()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBar = new ModernWpf.Controls.SelectorBar();
            var deleted = new SelectorBarItem
            {
                Text = "Deleted",
                Icon = new SymbolIcon(Symbol.Delete),
                IsEnabled = false
            };
            var remote = new SelectorBarItem
            {
                Text = "Remote",
                Icon = new SymbolIcon(Symbol.Remote),
                IsSelected = true
            };
            var shared = new SelectorBarItem
            {
                Text = "Shared",
                Icon = new SymbolIcon(Symbol.Share)
            };
            var favorites = new SelectorBarItem
            {
                Text = "Favorites",
                Icon = new SymbolIcon(Symbol.Favorite)
            };

            selectorBar.Items.Add(deleted);
            selectorBar.Items.Add(remote);
            selectorBar.Items.Add(shared);
            selectorBar.Items.Add(favorites);

            using var host = new TestWindowHost(selectorBar, width: 400, height: 120);

            Assert.AreEqual(4, selectorBar.Items.Count);
            Assert.AreSame(remote, selectorBar.SelectedItem);
            Assert.IsTrue(remote.IsSelected);

            selectorBar.Items.RemoveAt(1);

            Assert.AreEqual(3, selectorBar.Items.Count);
            Assert.IsNull(selectorBar.SelectedItem);

            selectorBar.Items.Clear();

            Assert.AreEqual(0, selectorBar.Items.Count);
        });
    }

    [TestMethod]
    public void ClickingItemUpdatesSelectedItem()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBar = new ModernWpf.Controls.SelectorBar();
            var first = new SelectorBarItem { Text = "First" };
            var second = new SelectorBarItem { Text = "Second" };
            selectorBar.Items.Add(first);
            selectorBar.Items.Add(second);

            var selectionChangedCount = 0;
            selectorBar.SelectionChanged += (sender, args) => selectionChangedCount++;

            using var host = new TestWindowHost(selectorBar, width: 400, height: 120);

            RaiseItemClick(second);

            Assert.AreSame(second, selectorBar.SelectedItem);
            Assert.IsFalse(first.IsSelected);
            Assert.IsTrue(second.IsSelected);
            Assert.AreEqual(1, selectionChangedCount);
        });
    }

    [TestMethod]
    public void SelectedItemMustBelongToItems()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBar = new ModernWpf.Controls.SelectorBar();

            Assert.ThrowsException<System.ArgumentException>(() => selectorBar.SelectedItem = new SelectorBarItem());
        });
    }

    [TestMethod]
    public void VerifySelectionAutomation()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBar = new ModernWpf.Controls.SelectorBar();
            var first = new SelectorBarItem { Text = "First" };
            var second = new SelectorBarItem { Text = "Second" };
            selectorBar.Items.Add(first);
            selectorBar.Items.Add(second);

            using var host = new TestWindowHost(selectorBar, width: 400, height: 120);

            var itemPeer = FrameworkElementAutomationPeer.CreatePeerForElement(second);
            var selectionItemProvider = (ISelectionItemProvider)itemPeer.GetPattern(PatternInterface.SelectionItem);
            selectionItemProvider.Select();

            Assert.AreSame(second, selectorBar.SelectedItem);
            Assert.IsTrue(selectionItemProvider.IsSelected);
            Assert.AreEqual("Second", itemPeer.GetName());
            Assert.AreEqual("SelectorBarItem", itemPeer.GetLocalizedControlType());

            var itemsView = GetNamedDescendant<SelectorBarItemsControl>(selectorBar, "PART_ItemsView");
            var itemsViewPeer = FrameworkElementAutomationPeer.CreatePeerForElement(itemsView);
            var selectionProvider = (ISelectionProvider)itemsViewPeer.GetPattern(PatternInterface.Selection);

            Assert.IsFalse(selectionProvider.CanSelectMultiple);
            Assert.IsFalse(selectionProvider.IsSelectionRequired);
            Assert.AreEqual(1, selectionProvider.GetSelection().Length);
            Assert.AreEqual("ItemsView", itemsViewPeer.GetClassName());
            Assert.AreEqual(AutomationControlType.List, itemsViewPeer.GetAutomationControlType());
            Assert.AreEqual(AutomationControlType.ListItem, itemPeer.GetAutomationControlType());
            var selectorPeer = FrameworkElementAutomationPeer.CreatePeerForElement(selectorBar);
            Assert.IsNotNull(selectorPeer);
            Assert.IsFalse(selectorPeer.IsControlElement());
            Assert.IsFalse(selectorPeer.IsContentElement());
        });
    }

    private static void RaiseItemClick(SelectorBarItem item)
    {
        item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonDownEvent
        });
        item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonUpEvent
        });
    }

    private static T GetNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        return VisualTreeTestHelper
            .EnumerateDescendants(root)
            .OfType<T>()
            .FirstOrDefault(candidate => candidate.Name == name)
            ?? throw new AssertFailedException($"Expected to find template part {name}.");
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setterValue = GetSetterValue(style, property);

        var dynamicResource = setterValue as DynamicResourceExtension;
        Assert.IsNotNull(dynamicResource, $"Expected {property.Name} to use a dynamic resource.");
        Assert.AreEqual(expectedResourceKey, dynamicResource!.ResourceKey);
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object? expectedValue)
    {
        Assert.AreEqual(expectedValue, GetSetterValue(style, property));
    }

    private static object? GetSetterValue(Style style, DependencyProperty property)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        return setter!.Value;
    }

    private static void AssertResourceAlias(FrameworkElement element, object resourceKey, object expectedResourceKey)
    {
        Assert.AreSame(
            element.TryFindResource(expectedResourceKey),
            element.TryFindResource(resourceKey),
            $"Unexpected resource alias for {resourceKey}.");
    }

    private static void AssertStateSetterDynamicResource(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string target,
        object expectedResourceKey)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(candidate => candidate.Name == groupName);
        var state = group.States
            .OfType<VisualStateEx>()
            .Single(candidate => candidate.Name == stateName);
        var setter = state.Setters.Single(candidate => candidate.Target == target);

        AssertResourceReferenceExpression(
            setter.ReadLocalValue(VisualStateSetter.ValueProperty),
            expectedResourceKey);
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
