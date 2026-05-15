using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using WpfTreeView = System.Windows.Controls.TreeView;
using WpfTreeViewItem = System.Windows.Controls.TreeViewItem;

namespace ModernWpf.WinUI.Tests.TreeView;

[TestClass]
public class TreeViewResourceTests
{
    [TestMethod]
    public void VerifyFinalWinUI2TreeViewThemeResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "TreeViewItemBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemBackgroundPressed", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemBackgroundDisabled", "SubtleFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemBackgroundSelected", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemBackgroundSelectedPointerOver", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemBackgroundSelectedPressed", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemForegroundPressed", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemForegroundSelectedPressed", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemMultiSelectBorderBrushSelected", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemSelectionIndicatorForeground", "AccentFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemSelectionIndicatorForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceValue(themeName, "TreeViewItemBorderThemeThickness", new Thickness(0));
                AssertThemeResourceValue(themeName, "TreeViewItemPresenterMargin", new Thickness(4, 2, 4, 2));
                AssertThemeResourceValue(themeName, "TreeViewItemPresenterPadding", new Thickness(0, 3, 0, 5));
                AssertThemeResourceValue(themeName, "TreeViewItemMultiSelectSelectedItemBorderMargin", new Thickness(0));
                AssertThemeResourceValue(themeName, "TreeViewItemMinHeight", 28.0);
                AssertThemeResourceValue(themeName, "TreeViewItemMultiSelectCheckBoxMinHeight", 28.0);
                AssertThemeResourceValue(themeName, "TreeViewItemContentHeight", 20.0);
            }

            AssertThemeResourceReference("HighContrast", "TreeViewItemBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemBackgroundSelectedPointerOver", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemForeground", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemForegroundPointerOver", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemForegroundDisabled", "SystemColorGrayTextColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemBorderBrushSelected", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemBorderBrushSelectedPointerOver", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemCheckGlyphSelected", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemSelectionIndicatorForeground", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemSelectionIndicatorForegroundDisabled", "SystemColorGrayTextColorBrush");
            AssertThemeResourceValue("HighContrast", "TreeViewItemBorderThemeThickness", new Thickness(1));
            AssertThemeResourceValue("HighContrast", "TreeViewItemMultiSelectSelectedItemBorderMargin", new Thickness(1));
            AssertThemeResourceValue("HighContrast", "TreeViewItemMinHeight", 28.0);
        });
    }

    [TestMethod]
    public void VerifyFinalWinUI2TreeViewResourceDefaults()
    {
        WpfTestHost.Run(() =>
        {
            var resources = new XamlControlsResources();

            AssertResource(resources, "TreeViewItemMinHeight", 28.0);
            AssertResource(resources, "TreeViewItemPresenterMargin", new Thickness(4, 2, 4, 2));
            AssertResource(resources, "TreeViewItemPresenterPadding", new Thickness(0, 3, 0, 5));
            AssertResource(resources, "TreeViewItemMultiSelectSelectedItemBorderMargin", new Thickness(0));
            AssertResource(resources, "TreeViewItemMultiSelectCheckBoxMinHeight", 28.0);
            AssertResource(resources, "TreeViewItemContentHeight", 20.0);
        });
    }

    [TestMethod]
    public void VerifyTreeViewItemStyleUsesFinalResourceKeys()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var style = (Style)Application.Current.TryFindResource("DefaultTreeViewItemStyle");
            Assert.IsNotNull(style);

            AssertDynamicResourceSetter(style, Control.BackgroundProperty, "TreeViewItemBackground");
            AssertDynamicResourceSetter(style, Control.ForegroundProperty, "TreeViewItemForeground");
            AssertDynamicResourceSetter(style, Control.BorderBrushProperty, "TreeViewItemBorderBrush");
            AssertDynamicResourceSetter(style, Control.BorderThicknessProperty, "TreeViewItemBorderThemeThickness");
            AssertDynamicResourceSetter(style, FrameworkElement.MinHeightProperty, "TreeViewItemMinHeight");
            AssertDynamicResourceSetter(style, TreeViewItemHelper.GlyphBrushProperty, "TreeViewItemForeground");
            Assert.AreEqual(12.0, TreeViewItemHelper.GetGlyphSize(new WpfTreeViewItem()));
            Assert.IsNotNull(GetLocalSetter(style, TreeViewItemHelper.CollapsedPathProperty).Value);
            Assert.IsNotNull(GetLocalSetter(style, TreeViewItemHelper.ExpandedPathProperty).Value);
        });
    }

    [TestMethod]
    public void VerifyWpfTreeViewExpansionAndIndentationMapping()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var child = new WpfTreeViewItem { Header = "Child" };
            var root = new WpfTreeViewItem
            {
                Header = "Root",
                IsExpanded = true
            };
            root.Items.Add(child);

            var treeView = new WpfTreeView();
            treeView.Items.Add(root);

            using var host = new TestWindowHost(treeView);

            Assert.AreEqual(new Thickness(0), TreeViewItemHelper.GetIndentation(root));
            Assert.AreEqual(new Thickness(16, 0, 0, 0), TreeViewItemHelper.GetIndentation(child));

            var rootChevron = FindNamedDescendant<ToggleButton>(root, "ExpandCollapseChevron");
            var childChevron = FindNamedDescendant<ToggleButton>(child, "ExpandCollapseChevron");

            Assert.AreEqual(Visibility.Visible, rootChevron.Visibility);
            Assert.AreEqual(Visibility.Hidden, childChevron.Visibility);

            root.IsExpanded = false;
            host.UpdateLayout();
            Assert.IsFalse(child.IsVisible);

            root.IsExpanded = true;
            host.UpdateLayout();
            Assert.IsTrue(child.IsVisible);
        });
    }

    [TestMethod]
    public void TreeViewItemTemplateUsesWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var root = new WpfTreeViewItem
            {
                Header = "Root",
                IsExpanded = true,
                IsSelected = true
            };
            root.Items.Add(new WpfTreeViewItem { Header = "Child" });

            var treeView = new WpfTreeView();
            treeView.Items.Add(root);

            using var host = new TestWindowHost(treeView);

            var headerPresenter = FindNamedDescendant<ContentPresenterEx>(root, "PART_Header");
            Assert.AreEqual(root.Header, headerPresenter.Content);
            Assert.AreSame(headerPresenter.TryFindResource("TreeViewItemForegroundSelected"), headerPresenter.Foreground);

            var chevron = FindNamedDescendant<ToggleButton>(root, "ExpandCollapseChevron");
            Assert.IsTrue(
                VisualTreeTestHelper.EnumerateDescendants(chevron).OfType<ContentPresenterEx>().Any(),
                "Expected the expand/collapse toggle template to use ContentPresenterEx.");
        });
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue(string themeName, string resourceKey, object expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertResource(ResourceDictionary resources, string key, object expected)
    {
        Assert.IsTrue(resources.Contains(key), $"Expected resource '{key}' to exist.");
        Assert.AreEqual(expected, resources[key], $"Unexpected resource value for '{key}'.");
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = GetLocalSetter(style, property);
        var dynamicResource = setter.Value as DynamicResourceExtension;
        Assert.IsNotNull(dynamicResource, $"Expected {property.Name} to use a dynamic resource.");
        Assert.AreEqual(expectedResourceKey, dynamicResource!.ResourceKey);
    }

    private static void AssertSetterValue<T>(Style style, DependencyProperty property, T expectedValue)
    {
        var setter = GetLocalSetter(style, property);
        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static Setter GetLocalSetter(Style style, DependencyProperty property)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        return setter!;
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is T match && match.Name == name)
            {
                return match;
            }
        }

        Assert.Fail($"Could not find descendant named '{name}'.");
        throw new InvalidOperationException();
    }
}
