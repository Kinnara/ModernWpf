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
                AssertLightDarkTabViewTheme(themeName);
            }

            AssertHighContrastTabViewTheme();
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

    private static void AssertLightDarkTabViewTheme(string themeName)
    {
        AssertThemeResourceReference(themeName, "TabViewBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewForeground", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemForegroundSelected", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "TabViewBorderBrush", "CardStrokeColorDefaultBrush");
        AssertThemeResourceReference(themeName, "TabViewSelectedItemBorderBrush", "CardStrokeColorDefaultBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderBackground", "LayerOnMicaBaseAltFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderBackgroundSelected", "SolidBackgroundFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderBackgroundPointerOver", "LayerOnMicaBaseAltFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderBackgroundPressed", "LayerOnMicaBaseAltFillColorDefaultBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderBackgroundDisabled", "LayerOnMicaBaseAltFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderForeground", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderForegroundPressed", "TextFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderForegroundSelected", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderForegroundPointerOver", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderForegroundDisabled", "TextFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "TabViewItemIconForeground", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemIconForegroundPressed", "TextFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemIconForegroundSelected", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemIconForegroundPointerOver", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemIconForegroundDisabled", "TextFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "TabViewButtonBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewButtonBackgroundPressed", "SubtleFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "TabViewButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewButtonBackgroundDisabled", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewButtonForeground", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "TabViewButtonForegroundPressed", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewButtonForegroundPointerOver", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "TabViewButtonForegroundDisabled", "TextFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "TabViewButtonBorderBrush", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewButtonBorderBrushPressed", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewButtonBorderBrushPointerOver", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewButtonBorderBrushDisabled", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewScrollButtonBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewScrollButtonBackgroundPressed", "SubtleFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "TabViewScrollButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewScrollButtonBackgroundDisabled", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewScrollButtonForeground", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewScrollButtonForegroundPressed", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewScrollButtonForegroundPointerOver", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewScrollButtonForegroundDisabled", "TextFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "TabViewScrollButtonBorderBrush", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewScrollButtonBorderBrushPressed", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewScrollButtonBorderBrushPointerOver", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewScrollButtonBorderBrushDisabled", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemSeparator", "DividerStrokeColorDefaultBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBackgroundPressed", "SubtleFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderPressedCloseButtonBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderPointerOverCloseButtonBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderSelectedCloseButtonBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderDisabledCloseButtonBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonForeground", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonForegroundPressed", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonForegroundPointerOver", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderPressedCloseButtonForeground", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderPointerOverCloseButtonForeground", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderSelectedCloseButtonForeground", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderDisabledCloseButtonForeground", "TextFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBorderBrush", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBorderBrushPointerOver", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBorderBrushPressed", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBorderBrushSelected", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBorderBrushDisabled", "SubtleFillColorTransparentBrush");
    }

    private static void AssertHighContrastTabViewTheme()
    {
        AssertThemeResourceReference("HighContrast", "TabViewBackground", "SystemColorWindowColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewForeground", "SystemColorWindowTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemForegroundSelected", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewBorderBrush", "SystemColorWindowTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewSelectedItemBorderBrush", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderBackground", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderBackgroundSelected", "SystemColorWindowColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderBackgroundPointerOver", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderBackgroundPressed", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderBackgroundDisabled", "SystemColorWindowColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderForeground", "SystemColorWindowTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderForegroundPressed", "SystemColorHighlightTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderForegroundSelected", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderForegroundPointerOver", "SystemColorHighlightTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderForegroundDisabled", "SystemColorGrayTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemIconForeground", "SystemColorWindowTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemIconForegroundPressed", "SystemColorHighlightTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemIconForegroundSelected", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemIconForegroundPointerOver", "SystemColorHighlightTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemIconForegroundDisabled", "SystemColorGrayTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewButtonBackground", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewButtonBackgroundPressed", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewButtonBackgroundPointerOver", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewButtonBackgroundDisabled", "SystemColorWindowColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewButtonForeground", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewButtonForegroundPressed", "SystemColorHighlightTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewButtonForegroundPointerOver", "SystemColorHighlightTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewButtonForegroundDisabled", "SystemColorGrayTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewButtonBorderBrush", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewButtonBorderBrushPressed", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewButtonBorderBrushPointerOver", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewButtonBorderBrushDisabled", "SystemColorGrayTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewScrollButtonBackground", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewScrollButtonBackgroundPressed", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewScrollButtonBackgroundPointerOver", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewScrollButtonBackgroundDisabled", "SystemColorWindowColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewScrollButtonForeground", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewScrollButtonForegroundPressed", "SystemColorHighlightTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewScrollButtonForegroundPointerOver", "SystemColorHighlightTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewScrollButtonForegroundDisabled", "SystemColorGrayTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewScrollButtonBorderBrush", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewScrollButtonBorderBrushPressed", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewScrollButtonBorderBrushPointerOver", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewScrollButtonBorderBrushDisabled", "SystemColorGrayTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemSeparator", "SystemColorWindowTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderCloseButtonBackground", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderCloseButtonBackgroundPressed", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderCloseButtonBackgroundPointerOver", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderPressedCloseButtonBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderPointerOverCloseButtonBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderSelectedCloseButtonBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderDisabledCloseButtonBackground", "SystemColorWindowColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderCloseButtonForeground", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderCloseButtonForegroundPressed", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderCloseButtonForegroundPointerOver", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderPressedCloseButtonForeground", "SystemColorHighlightTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderPointerOverCloseButtonForeground", "SystemColorHighlightTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderSelectedCloseButtonForeground", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderDisabledCloseButtonForeground", "SystemColorGrayTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderCloseButtonBorderBrush", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderCloseButtonBorderBrushPointerOver", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderCloseButtonBorderBrushPressed", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderCloseButtonBorderBrushSelected", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderCloseButtonBorderBrushDisabled", "SubtleFillColorTransparentBrush");
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
