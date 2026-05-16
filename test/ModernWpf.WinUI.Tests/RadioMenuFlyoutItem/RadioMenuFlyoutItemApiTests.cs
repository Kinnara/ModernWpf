using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.RadioMenuFlyoutItem;

[TestClass]
public class RadioMenuFlyoutItemApiTests
{
    [TestMethod]
    public void VerifyDefaultStyleAndWinUI2Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = new XamlControlsResources();
            AssertResourceValue(resources, "MenuFlyoutItemChevronMargin", new Thickness(24, 0, 0, -1));
            AssertResourceValue(resources, "MenuFlyoutItemPlaceholderThemeThickness", new Thickness(28, 0, 0, 0));
            AssertResourceValue(resources, "MenuFlyoutItemThemePadding", new Thickness(11, 8, 11, 9));
            AssertResourceValue(resources, "MenuFlyoutItemThemePaddingNarrow", new Thickness(11, 4, 11, 5));

            AssertLightDarkChevronResources("Light");
            AssertLightDarkChevronResources("Dark");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevron", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevronPointerOver", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevronPressed", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevronSubMenuOpened", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevronDisabled", "SystemColorGrayTextColorBrush");

            var item = new RadioMenuItem
            {
                Header = "Yellow",
                Icon = new SymbolIcon { Symbol = Symbol.Accept }
            };

            using var host = new TestWindowHost(item, width: 240, height: 80);
            host.UpdateLayout();

            Assert.AreEqual(resources["MenuFlyoutItemThemePadding"], item.Padding);
            Assert.AreEqual(HorizontalAlignment.Stretch, item.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Center, item.VerticalContentAlignment);

            item.ApplyTemplate();
            var checkGlyph = GetTemplateChild<FontIconFallback>(item, "CheckGlyph");
            Assert.AreEqual(12.0, checkGlyph.FontSize);
            Assert.AreEqual(new Thickness(0, 0, 16, 0), checkGlyph.Margin);
            Assert.AreEqual(0.0, checkGlyph.Opacity);

            var iconContent = GetTemplateChild<ContentPresenterEx>(item, "IconContent");
            Assert.AreSame(item.Icon, iconContent.Content);
            var iconRoot = GetTemplateChild<FrameworkElement>(item, "IconRoot");
            Assert.AreEqual(Visibility.Visible, iconRoot.Visibility);

            item.SetCurrentValue(MenuItem.IsCheckedProperty, true);
            host.UpdateLayout();

            Assert.AreEqual(1.0, checkGlyph.Opacity);
        });
    }

    [TestMethod]
    public void DefaultTemplateUsesVisualStateSettersForWinUIStateParity()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var item = new RadioMenuItem
            {
                Header = "Yellow",
                Icon = new SymbolIcon { Symbol = Symbol.Accept },
                InputGestureText = "Ctrl+Y"
            };

            using var host = new TestWindowHost(item, width: 240, height: 80);
            host.UpdateLayout();

            var root = GetTemplateChild<FrameworkElement>(item, "LayoutRoot");
            var keyboardAcceleratorTextBlock = GetTemplateChild<FrameworkElement>(item, "KeyboardAcceleratorTextBlock");

            AssertStateSetter(root, "CommonStates", "PointerOver", "LayoutRoot.Background");
            AssertStateSetter(root, "CommonStates", "PointerOver", "TextBlock.Foreground");
            AssertStateSetter(root, "CommonStates", "PointerOver", "CheckGlyph.Foreground");
            AssertStateSetter(root, "CommonStates", "PointerOver", "IconContent.Foreground");
            AssertStateSetter(root, "CommonStates", "PointerOver", "KeyboardAcceleratorTextBlock.Foreground");
            AssertStateSetter(root, "CommonStates", "Pressed", "LayoutRoot.Background");
            AssertStateSetter(root, "CommonStates", "Pressed", "TextBlock.Foreground");
            AssertStateSetter(root, "CommonStates", "Pressed", "CheckGlyph.Foreground");
            AssertStateSetter(root, "CommonStates", "Pressed", "IconContent.Foreground");
            AssertStateSetter(root, "CommonStates", "Pressed", "KeyboardAcceleratorTextBlock.Foreground");
            AssertStateSetter(root, "CommonStates", "Disabled", "TextBlock.Foreground");
            AssertStateSetter(root, "CommonStates", "Disabled", "CheckGlyph.Foreground");
            AssertStateSetter(root, "CommonStates", "Disabled", "IconContent.Foreground");
            AssertStateSetter(root, "CommonStates", "Disabled", "KeyboardAcceleratorTextBlock.Foreground");
            AssertStateSetter(root, "CheckStates", "Checked", "CheckGlyph.Opacity");
            AssertStateSetter(root, "CheckStates", "UncheckedWithIcon", "IconRoot.Visibility");
            AssertStateSetter(root, "CheckStates", "CheckedWithIcon", "CheckGlyph.Opacity");
            AssertStateSetter(root, "CheckStates", "CheckedWithIcon", "IconRoot.Visibility");
            AssertStateSetter(root, "KeyboardAcceleratorTextVisibility", "KeyboardAcceleratorTextVisible", "KeyboardAcceleratorTextBlock.Visibility");
            Assert.AreEqual(Visibility.Visible, keyboardAcceleratorTextBlock.Visibility);
        });
    }

    private static void AssertLightDarkChevronResources(string themeName)
    {
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevron", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevronPointerOver", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevronPressed", "TextFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevronSubMenuOpened", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevronDisabled", "TextFillColorDisabledBrush");
    }

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        var child = control.Template.FindName(name, control) as T;
        Assert.IsNotNull(child, $"Missing template part '{name}'.");
        return child!;
    }

    private static void AssertResourceValue<T>(ResourceDictionary resources, string resourceKey, T expectedValue)
    {
        Assert.IsTrue(resources.Contains(resourceKey), $"Missing root resource '{resourceKey}'.");
        Assert.AreEqual(expectedValue, resources[resourceKey], resourceKey);
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string setterTarget)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        Assert.IsTrue(
            stateEx.Setters.Any(setter => setter.Target == setterTarget),
            $"{groupName}.{stateName} should set {setterTarget}.");
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }
}
