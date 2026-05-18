using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public void VerifyOfficialWpfFluentTreeViewThemeResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "TreeViewItemBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemBackgroundSelected", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "TreeViewItemForeground", "TextFillColorPrimaryBrush");
            }

            AssertThemeResourceReference("Light", "TreeViewItemSelectionIndicatorForeground", "SystemAccentColorDark1Brush");
            AssertThemeResourceReference("Dark", "TreeViewItemSelectionIndicatorForeground", "SystemAccentColorLight2Brush");

            AssertThemeResourceReference("HighContrast", "TreeViewItemBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemBackgroundPointerOver", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemBackgroundSelected", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemForeground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "TreeViewItemSelectionIndicatorForeground", "SystemColorHighlightColorBrush");
        });
    }

    [TestMethod]
    public void VerifyOfficialWpfFluentTreeViewResourceDefaults()
    {
        WpfTestHost.Run(() =>
        {
            var resources = new XamlControlsResources();

            AssertResource(resources, "TreeViewItemChevronSize", 10.0);
            AssertResource(resources, "TreeViewItemFontSize", 14.0);
            AssertResource(resources, "TreeViewChevronRightGlyph", "\uE76C");
            AssertResource(resources, "TreeViewChevronLeftGlyph", "\uE76B");

            AssertResource(resources, "TreeViewItemMinHeight", 28.0);
            AssertResource(resources, "TreeViewItemPresenterMargin", new Thickness(4, 2, 4, 2));
            AssertResource(resources, "TreeViewItemPresenterPadding", new Thickness(0, 3, 0, 5));
            AssertResource(resources, "TreeViewItemMultiSelectSelectedItemBorderMargin", new Thickness(0));
            AssertResource(resources, "TreeViewItemMultiSelectCheckBoxMinHeight", 28.0);
            AssertResource(resources, "TreeViewItemContentHeight", 20.0);
        });
    }

    [TestMethod]
    public void VerifyTreeViewItemStyleUsesOfficialWpfFluentSurface()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var style = (Style)Application.Current.TryFindResource("DefaultTreeViewItemStyle");
            Assert.IsNotNull(style);

            AssertDynamicResourceSetter(style, Control.FocusVisualStyleProperty, "DefaultCollectionFocusVisualStyle");
            AssertDynamicResourceSetter(style, Control.ForegroundProperty, "TreeViewItemForeground");
            AssertDynamicResourceSetter(style, Control.BackgroundProperty, "TreeViewItemBackground");
            AssertSetterValue(style, FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 2));
            AssertSetterValue(style, Control.PaddingProperty, new Thickness(4));
            AssertDynamicResourceSetter(style, ControlHelper.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetterValue(style, Control.IsTabStopProperty, true);
            AssertSetterValue(style, FrameworkElement.OverridesDefaultStyleProperty, true);
            AssertSetterValue(style, UIElement.SnapsToDevicePixelsProperty, true);

            var oldHelperSetters = style.Setters
                .OfType<Setter>()
                .Where(setter => setter.Property.OwnerType.Name == "TreeViewItemHelper")
                .ToArray();
            Assert.AreEqual(0, oldHelperSetters.Length, "Official WPF Fluent TreeViewItem should not use the deleted TreeViewItemHelper state path.");
        });
    }

    [TestMethod]
    public void VerifyOfficialWpfFluentTreeViewExpansion()
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

            var rootExpander = FindNamedDescendant<ToggleButton>(root, "Expander");
            var childExpander = FindNamedDescendant<ToggleButton>(child, "Expander");
            var itemsHost = FindNamedDescendant<ItemsPresenter>(root, "ItemsHost");

            Assert.AreEqual(Visibility.Visible, rootExpander.Visibility);
            Assert.AreEqual(Visibility.Hidden, childExpander.Visibility);
            Assert.AreEqual(Visibility.Visible, itemsHost.Visibility);

            root.IsExpanded = false;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, itemsHost.Visibility);

            root.IsExpanded = true;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, itemsHost.Visibility);
        });
    }

    [TestMethod]
    public void TreeViewItemTemplateUsesOfficialWpfPresenterAndTriggers()
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
            host.UpdateLayout();

            var headerPresenter = FindNamedDescendant<ContentPresenter>(root, "PART_Header");
            Assert.AreEqual(root.Header, headerPresenter.Content);
            Assert.AreEqual(14.0, TextElement.GetFontSize(headerPresenter));

            var activeRectangle = FindNamedDescendant<Rectangle>(root, "ActiveRectangle");
            Assert.AreEqual(Visibility.Visible, activeRectangle.Visibility);
            Assert.AreSame(root.TryFindResource("TreeViewItemSelectionIndicatorForeground"), activeRectangle.Fill);

            var itemBorder = FindNamedDescendant<Border>(root, "Border");
            Assert.AreSame(root.TryFindResource("TreeViewItemBackgroundSelected"), itemBorder.Background);

            var chevronIcon = FindNamedDescendant<TextBlock>(root, "ChevronIcon");
            Assert.AreEqual(root.TryFindResource("TreeViewChevronRightGlyph"), chevronIcon.Text);
            Assert.AreEqual(root.TryFindResource("TreeViewItemChevronSize"), chevronIcon.FontSize);

            Assert.IsFalse(
                VisualTreeTestHelper.EnumerateDescendants(root).OfType<ContentPresenterEx>().Any(),
                "Official WPF Fluent TreeViewItem should use plain WPF ContentPresenter slots.");
        });
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
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
