using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.TabView;

[TestClass]
public class TabViewResourceTests
{
    [TestMethod]
    public void OfficialWpfFluentTabControlThemeResourcesAreAvailable()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "TabViewBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "TabViewForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "TabViewItemForegroundSelected", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "TabViewBorderBrush", "CardStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "TabViewSelectedItemBorderBrush", "CardStrokeColorDefaultBrush");
            }

            AssertThemeResourceReference("HighContrast", "TabViewBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "TabViewForeground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "TabViewItemForegroundSelected", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "TabViewBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "TabViewSelectedItemBorderBrush", "SystemColorHighlightColorBrush");
        });
    }

    [TestMethod]
    public void OfficialWpfFluentTabControlStylesUseOfficialResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            Assert.IsInstanceOfType(Application.Current.TryFindResource("DefaultTopTabControlStyle"), typeof(ControlTemplate));
            Assert.IsInstanceOfType(Application.Current.TryFindResource("DefaultBottomTabControlStyle"), typeof(ControlTemplate));
            Assert.IsInstanceOfType(Application.Current.TryFindResource("DefaultLeftTabControlStyle"), typeof(ControlTemplate));
            Assert.IsInstanceOfType(Application.Current.TryFindResource("DefaultRightTabControlStyle"), typeof(ControlTemplate));

            var tabControlStyle = (Style)Application.Current.TryFindResource("DefaultTabControlStyle");
            var tabItemStyle = (Style)Application.Current.TryFindResource("DefaultTabItemStyle");
            Assert.IsNotNull(tabControlStyle);
            Assert.IsNotNull(tabItemStyle);

            AssertDynamicResourceSetter(tabControlStyle, Control.ForegroundProperty, "TabViewForeground");
            AssertDynamicResourceSetter(tabControlStyle, Control.BackgroundProperty, "TabViewBackground");
            AssertDynamicResourceSetter(tabControlStyle, Control.BorderBrushProperty, "TabViewBorderBrush");
            AssertSetterValue(tabControlStyle, Control.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
            AssertDynamicResourceSetter(tabItemStyle, Control.FocusVisualStyleProperty, "DefaultControlFocusVisualStyle");
            AssertNoSetter(tabItemStyle, FrameworkElement.MinHeightProperty);
            AssertNoSetter(tabItemStyle, FrameworkElement.MaxWidthProperty);
            AssertNoSetter(tabItemStyle, FrameworkElement.MinWidthProperty);
        });
    }

    [TestMethod]
    public void OfficialWpfFluentTabControlTemplatesUseWpfPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var tabItem = new TabItem
            {
                Header = "First",
                Content = "First content",
                IsSelected = true
            };
            var secondItem = new TabItem
            {
                Header = "Second",
                Content = "Second content"
            };
            var tabControl = new TabControl
            {
                Width = 360,
                Height = 180,
                Items =
                {
                    tabItem,
                    secondItem
                }
            };

            using var host = new TestWindowHost(tabControl, width: 420, height: 240);
            host.UpdateLayout();

            var headerPresenter = FindTemplateChild<ContentPresenter>(tabItem, "ContentSite");
            Assert.AreEqual(tabItem.Header, headerPresenter.Content);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(tabItem));

            var headerBorder = FindTemplateChild<Border>(tabItem, "Border");
            Assert.AreSame(tabItem.TryFindResource("TabViewItemHeaderBackgroundSelected"), headerBorder.Background);
            Assert.AreSame(tabItem.TryFindResource("TabViewSelectedItemBorderBrush"), headerBorder.BorderBrush);
            Assert.AreSame(tabItem.TryFindResource("TabViewItemForegroundSelected"), tabItem.Foreground);
            Assert.AreEqual(100, Panel.GetZIndex(tabItem));

            var selectedContentHost = FindTemplateChild<ContentPresenter>(tabControl, "PART_SelectedContentHost");
            Assert.AreEqual(tabItem.Content, selectedContentHost.Content);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(tabControl));
        });
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");

        var dynamicResource = setter!.Value as DynamicResourceExtension;
        Assert.IsNotNull(dynamicResource, $"Expected {property.Name} to use a dynamic resource.");
        Assert.AreEqual(expectedResourceKey, dynamicResource!.ResourceKey);
    }

    private static void AssertSetterValue<T>(Style style, DependencyProperty property, T expectedValue)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static void AssertNoSetter(Style style, DependencyProperty property)
    {
        Assert.IsFalse(
            style.Setters.OfType<Setter>().Any(setter => setter.Property == property),
            $"Official WPF Fluent TabItem style should not set {property.Name}.");
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template child '{name}' on {control.GetType().Name}.");
    }

    private static T? FindVisualChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T typedChild)
            {
                return typedChild;
            }

            var descendant = FindVisualChild<T>(child);
            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }
}
