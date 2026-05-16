using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using XamlControlsResources = ModernWpf.Controls.XamlControlsResources;

namespace ModernWpf.WinUI.Tests.TabView;

[TestClass]
public class TabViewResourceTests
{
    [TestMethod]
    public void VerifyFinalWinUI2TabViewDefaultResources()
    {
        WpfTestHost.Run(() =>
        {
            var resources = new XamlControlsResources();

            AssertResource(resources, "TabViewTopHeaderPadding", new Thickness(0, 8, 0, 0));
            AssertResource(resources, "TabViewHeaderPadding", new Thickness(0, 8, 0, 0));
            AssertResource(resources, "TabViewItemHeaderPadding", new Thickness(8, 3, 4, 3));
            AssertResource(resources, "TabViewSelectedItemHeaderPadding", new Thickness(9, 3, 5, 4));
            AssertResource(resources, "TabViewItemMinHeight", 32.0);
            AssertResource(resources, "TabViewItemMaxWidth", 240.0);
            AssertResource(resources, "TabViewItemMinWidth", 100.0);
            AssertResource(resources, "TabViewItemHeaderFontSize", 12.0);
            AssertResource(resources, "TabViewItemHeaderIconSize", 16.0);
            AssertResource(resources, "TabViewItemHeaderIconMargin", new Thickness(0, 0, 10, 0));
            AssertResource(resources, "TabViewItemHeaderCloseButtonHeight", 24.0);
            AssertResource(resources, "TabViewItemHeaderCloseButtonWidth", 32.0);
            AssertResource(resources, "TabViewItemHeaderCloseButtonSize", 16.0);
            AssertResource(resources, "TabViewItemHeaderCloseFontSize", 12.0);
            AssertResource(resources, "TabViewItemHeaderCloseMargin", new Thickness(4, 0, 0, 0));
            AssertResource(resources, "TabViewItemScrollButtonWidth", 32.0);
            AssertResource(resources, "TabViewItemScrollButtonHeight", 24.0);
            AssertResource(resources, "TabViewItemScrollButonFontSize", 8.0);
            AssertResource(resources, "TabViewItemScrollButtonPadding", new Thickness(7, 3, 7, 3));
            AssertResource(resources, "TabViewItemLeftScrollButtonContainerPadding", new Thickness(8, 0, 3, 3));
            AssertResource(resources, "TabViewItemRightScrollButtonContainerPadding", new Thickness(3, 0, 8, 3));
            AssertResource(resources, "TabViewItemAddButtonWidth", 32.0);
            AssertResource(resources, "TabViewItemAddButtonHeight", 24.0);
            AssertResource(resources, "TabViewItemAddButtonFontSize", 12.0);
            AssertResource(resources, "TabViewItemAddButtonContainerPadding", new Thickness(3, 0, 0, 3));
            AssertResource(resources, "TabViewShadowDepth", 16.0);
            AssertResource(resources, "TabViewItemSeparatorMargin", new Thickness(0, 8, 0, 8));
            AssertResource(resources, "TabViewItemBorderThickness", new Thickness(1));
            AssertResource(resources, "TabViewSelectedItemBorderThickness", new Thickness(1, 1, 1, 0));
            AssertResource(resources, "TabViewSelectedItemHeaderMargin", new Thickness(-1, 0, -1, 0));
        });
    }

    [TestMethod]
    public void VerifyFinalWinUI2TabViewThemeResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertLightDarkTabViewResources(themeName);
            }

            AssertThemeResourceValue("Light", "LayerOnMicaBaseAltFillColorDefault", Color.FromArgb(0xB3, 0xFF, 0xFF, 0xFF));
            AssertThemeResourceValue("Light", "LayerOnMicaBaseAltFillColorSecondary", Color.FromArgb(0x0A, 0x00, 0x00, 0x00));
            AssertThemeResourceValue("Light", "LayerOnMicaBaseAltFillColorTransparent", Color.FromArgb(0x00, 0x00, 0x00, 0x00));
            AssertThemeResourceValue("Dark", "LayerOnMicaBaseAltFillColorDefault", Color.FromArgb(0x73, 0x3A, 0x3A, 0x3A));
            AssertThemeResourceValue("Dark", "LayerOnMicaBaseAltFillColorSecondary", Color.FromArgb(0x0F, 0xFF, 0xFF, 0xFF));
            AssertThemeResourceValue("Dark", "LayerOnMicaBaseAltFillColorTransparent", Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
            AssertHighContrastTabViewResources();
        });
    }

    [TestMethod]
    public void VerifyWpfTabItemStyleUsesFinalWinUI2SizingResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var tabControlStyle = (Style)Application.Current.TryFindResource("DefaultTabControlStyle");
            var tabItemStyle = (Style)Application.Current.TryFindResource("DefaultTabItemStyle");
            Assert.IsNotNull(tabControlStyle);
            Assert.IsNotNull(tabItemStyle);

            AssertDynamicResourceSetter(tabControlStyle, Control.BackgroundProperty, "TabViewBackground");
            AssertDynamicResourceSetter(tabItemStyle, FrameworkElement.MinHeightProperty, "TabViewItemMinHeight");
            AssertDynamicResourceSetter(tabItemStyle, FrameworkElement.MaxWidthProperty, "TabViewItemMaxWidth");
            AssertDynamicResourceSetter(tabItemStyle, FrameworkElement.MinWidthProperty, "TabViewItemMinWidth");
            AssertSetterValue(tabItemStyle, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetterValue(tabItemStyle, Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch);

            var tabItem = new TabItem
            {
                Header = "Tab",
                Content = "Content"
            };
            var tabControl = new TabControl
            {
                Width = 320,
                Height = 160
            };
            tabControl.Items.Add(tabItem);

            using var host = new TestWindowHost(tabControl, width: 380, height: 220);

            Assert.AreEqual(32.0, tabItem.MinHeight);
            Assert.AreEqual(240.0, tabItem.MaxWidth);
            Assert.AreEqual(100.0, tabItem.MinWidth);
        });
    }

    [TestMethod]
    public void WpfTabItemStatesUseVisualStateSettersForWinUIParity()
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
            TabItemHelper.SetIcon(tabItem, new TextBlock { Text = "I" });

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
            var layoutRoot = FindNamedDescendant<Border>(tabItem, "LayoutRoot");

            AssertStateSetter(layoutRoot, "CommonStates", "PointerOver",
                "TabContainer.Background",
                "ContentPresenter.Foreground",
                "IconControl.Foreground");
            AssertStateSetter(layoutRoot, "CommonStates", "Pressed",
                "TabContainer.Background",
                "ContentPresenter.Foreground",
                "IconControl.Foreground");
            AssertStateSetter(layoutRoot, "CommonStates", "Selected",
                "Shadow.IsShadowEnabled",
                "TabContainer.Background",
                "TabContainer.Margin",
                "TabContainer.BorderThickness",
                "TabContainer.Padding",
                "ContentPresenter.Foreground",
                "IconControl.Foreground",
                "ContentPresenter.FontWeight");
            AssertStateSetter(layoutRoot, "CommonStates", "PointerOverSelected",
                "TabContainer.Background",
                "ContentPresenter.FontWeight");
            AssertStateSetter(layoutRoot, "CommonStates", "PressedSelected",
                "TabContainer.Background",
                "ContentPresenter.FontWeight");
            AssertStateSetter(layoutRoot, "DisabledStates", "Disabled",
                "TabContainer.Background",
                "IconControl.Foreground",
                "ContentPresenter.Foreground");
            AssertStateSetter(layoutRoot, "IconStates", "NoIcon", "IconBox.Visibility");
            AssertStateSetter(layoutRoot, "ForegroundStates", "ForegroundSet",
                "IconControl.Foreground",
                "ContentPresenter.Foreground");

            AssertCurrentState(layoutRoot, "CommonStates", "Selected");
            AssertCurrentState(layoutRoot, "DisabledStates", "Enabled");
            AssertCurrentState(layoutRoot, "IconStates", "Icon");
            AssertCurrentState(layoutRoot, "ForegroundStates", "ForegroundNotSet");

            var contentPresenter = FindNamedDescendant<ContentPresenterEx>(tabItem, "ContentPresenter");
            var tabContainer = FindNamedDescendant<Border>(tabItem, "TabContainer");
            Assert.AreEqual(FontWeights.SemiBold, contentPresenter.FontWeight);
            Assert.AreSame(tabItem.TryFindResource("TabViewItemHeaderBackgroundSelected"), tabContainer.Background);
            Assert.AreEqual(new Thickness(-1, 0, -1, 0), tabContainer.Margin);
            Assert.AreEqual(new Thickness(1, 1, 1, 0), tabContainer.BorderThickness);
            Assert.AreEqual(new Thickness(9, 3, 5, 4), tabContainer.Padding);
            Assert.AreEqual(1, Panel.GetZIndex(tabItem));

            var foreground = Brushes.Red;
            tabItem.Foreground = foreground;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            AssertCurrentState(layoutRoot, "ForegroundStates", "ForegroundSet");
            Assert.AreSame(foreground, contentPresenter.Foreground);

            TabItemHelper.SetIcon(tabItem, null);
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            AssertCurrentState(layoutRoot, "IconStates", "NoIcon");
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<Viewbox>(tabItem, "IconBox").Visibility);

            tabItem.ClearValue(Control.ForegroundProperty);
            tabItem.IsEnabled = false;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            AssertCurrentState(layoutRoot, "ForegroundStates", "ForegroundNotSet");
            AssertCurrentState(layoutRoot, "DisabledStates", "Disabled");
            Assert.AreSame(tabItem.TryFindResource("TabViewItemHeaderForegroundDisabled"), contentPresenter.Foreground);
        });
    }

    private static void AssertLightDarkTabViewResources(string themeName)
    {
        AssertThemeResourceReference(themeName, "TabViewBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderBackground", "LayerOnMicaBaseAltFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderBackgroundPointerOver", "LayerOnMicaBaseAltFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderBackgroundPressed", "LayerOnMicaBaseAltFillColorDefaultBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderBackgroundSelected", "SolidBackgroundFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderBackgroundDisabled", "LayerOnMicaBaseAltFillColorTransparentBrush");
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
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBorderBrush", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBorderBrushPointerOver", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBorderBrushPressed", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBorderBrushSelected", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "TabViewItemHeaderCloseButtonBorderBrushDisabled", "SubtleFillColorTransparentBrush");
    }

    private static void AssertHighContrastTabViewResources()
    {
        AssertThemeResourceReference("HighContrast", "TabViewBackground", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderBackground", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderBackgroundSelected", "SystemColorWindowColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderBackgroundPointerOver", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderBackgroundPressed", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderBackgroundDisabled", "SystemColorWindowColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderForeground", "SystemColorWindowTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderForegroundSelected", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderForegroundPointerOver", "SystemColorHighlightTextColorBrush");
        AssertThemeResourceReference("HighContrast", "TabViewItemHeaderForegroundDisabled", "SystemColorGrayTextColorBrush");
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

    private static VisualStateEx AssertStateSetter(FrameworkElement stateGroupsRoot, string groupName, string stateName, params string[] expectedTargets)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        var state = group.States.OfType<VisualState>().SingleOrDefault(state => state.Name == stateName);
        Assert.IsNotNull(state, $"Could not find visual state '{groupName}.{stateName}'.");
        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        foreach (var expectedTarget in expectedTargets)
        {
            Assert.IsTrue(
                stateEx.Setters.Any(setter => setter.Target == expectedTarget),
                $"{groupName}.{stateName} is missing setter target '{expectedTarget ?? "<self>"}'.");
        }

        return stateEx;
    }

    private static void AssertCurrentState(FrameworkElement stateGroupsRoot, string groupName, string expectedStateName)
    {
        Assert.AreEqual(expectedStateName, FindVisualStateGroup(stateGroupsRoot, groupName).CurrentState?.Name);
    }

    private static VisualStateGroup FindVisualStateGroup(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .SingleOrDefault(group => group.Name == groupName);
        Assert.IsNotNull(group, $"Could not find visual state group '{groupName}'.");
        return group!;
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        var element = VisualTreeTestHelper.EnumerateDescendants(root)
            .OfType<T>()
            .SingleOrDefault(element => element.Name == name);
        Assert.IsNotNull(element, $"Could not find descendant named '{name}'.");
        return element!;
    }

    private static void AssertResource(ResourceDictionary resources, string key, object expected)
    {
        Assert.IsTrue(resources.Contains(key), $"Expected resource '{key}' to exist.");
        Assert.AreEqual(expected, resources[key], $"Unexpected value for '{key}'.");
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
}
