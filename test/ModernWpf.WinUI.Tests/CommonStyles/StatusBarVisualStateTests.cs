using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class StatusBarVisualStateTests
{
    [TestMethod]
    public void DefaultStatusBarStyleUsesOfficialWpfFluentStyleSurface()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultStatusBarStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(StatusBar));
            Assert.AreEqual(typeof(StatusBar), defaultStyle.TargetType);
            Assert.AreEqual(typeof(StatusBar), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            Assert.AreEqual(6, setters.Length);
            AssertBrushSetter(setters, Control.ForegroundProperty);
            AssertBrushSetter(setters, Control.BackgroundProperty);
            AssertDynamicResourceSetter(setters, Control.BorderBrushProperty, "ControlElevationBorderBrush");
            AssertSetter(setters, Control.BorderThicknessProperty, new Thickness(1));
            AssertSetter(setters, Control.PaddingProperty, new Thickness(12));
            AssertSetter(setters, FrameworkElement.MarginProperty, new Thickness(0));
            Assert.IsFalse(setters.Any(item => item.Property == Control.TemplateProperty));

            var statusBar = new StatusBar();
            using var host = new TestWindowHost(statusBar, width: 260, height: 80);
            host.UpdateLayout();

            AssertRuntimeStatusBarValues(statusBar);
        });
    }

    [TestMethod]
    public void StatusBarItemUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultStatusBarItemStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(StatusBarItem));
            Assert.AreEqual(typeof(StatusBarItem), defaultStyle.TargetType);
            Assert.AreEqual(typeof(StatusBarItem), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            Assert.AreEqual(5, setters.Length);
            AssertDynamicResourceSetter(setters, Control.PaddingProperty, "StatusBarItemPadding");
            AssertDynamicResourceSetter(setters, Control.BackgroundProperty, "StatusBarItemBackground");
            AssertSetter(setters, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
            AssertSetter(setters, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            Assert.IsTrue(setters.Any(item => item.Property == Control.TemplateProperty));

            var statusBarItem = new StatusBarItem
            {
                Content = "Status content"
            };

            using var host = new TestWindowHost(statusBarItem, width: 220, height: 80);
            host.UpdateLayout();

            AssertRuntimeStatusBarItemValues(statusBarItem);
            AssertStatusBarItemTemplateShape(statusBarItem);

            statusBarItem.IsEnabled = false;
            host.UpdateLayout();

            Assert.AreSame(statusBarItem.TryFindResource("StatusBarItemBackgroundDisabled"), statusBarItem.Background);
            Assert.AreSame(statusBarItem.TryFindResource("StatusBarItemForegroundDisabled"), statusBarItem.Foreground);
        });
    }

    [TestMethod]
    public void SeparatorStyleUsesOfficialWpfFluentShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var separatorStyle = (Style)Application.Current.FindResource(StatusBar.SeparatorStyleKey);
            Assert.AreEqual(typeof(Separator), separatorStyle.TargetType);

            var setters = separatorStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(setters, Control.BorderBrushProperty, "ControlElevationBorderBrush");
            AssertBrushSetter(setters, Control.BackgroundProperty, Colors.Transparent);
            AssertSetter(setters, FrameworkElement.MarginProperty, new Thickness(6, 0, 6, 0));
            AssertSetter(setters, Control.BorderThicknessProperty, new Thickness(1, 1, 0, 0));
            AssertSetter(setters, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);
            Assert.IsTrue(setters.Any(item => item.Property == Control.TemplateProperty));
        });
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialStatusBarItemAliases()
    {
        WpfTestHost.Run(() =>
        {
            AssertThemeResourceReference("Light", "StatusBarItemBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("Light", "StatusBarItemBackgroundDisabled", "ControlFillColorDisabledBrush");
            AssertThemeResourceReference("Light", "StatusBarItemForegroundDisabled", "TextFillColorDisabledBrush");

            AssertThemeResourceReference("Dark", "StatusBarItemBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("Dark", "StatusBarItemBackgroundDisabled", "ControlFillColorDisabledBrush");
            AssertThemeResourceReference("Dark", "StatusBarItemForegroundDisabled", "TextFillColorDisabledBrush");

            AssertThemeResourceReference("HighContrast", "StatusBarItemBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "StatusBarItemBackgroundDisabled", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "StatusBarItemForegroundDisabled", "SystemColorGrayTextColorBrush");
        });
    }

    private static void AssertRuntimeStatusBarValues(StatusBar statusBar)
    {
        Assert.AreEqual(new Thickness(1), statusBar.BorderThickness);
        Assert.AreEqual(new Thickness(12), statusBar.Padding);
        Assert.AreEqual(new Thickness(0), statusBar.Margin);
        Assert.AreSame(statusBar.TryFindResource("ControlElevationBorderBrush"), statusBar.BorderBrush);
        Assert.AreEqual((Color)statusBar.TryFindResource("TextFillColorPrimary"), ((SolidColorBrush)statusBar.Foreground).Color);
        Assert.AreEqual((Color)statusBar.TryFindResource("ControlFillColorDefault"), ((SolidColorBrush)statusBar.Background).Color);
    }

    private static void AssertRuntimeStatusBarItemValues(StatusBarItem statusBarItem)
    {
        Assert.AreEqual((Thickness)statusBarItem.TryFindResource("StatusBarItemPadding"), statusBarItem.Padding);
        Assert.AreSame(statusBarItem.TryFindResource("StatusBarItemBackground"), statusBarItem.Background);
        Assert.AreEqual(HorizontalAlignment.Left, statusBarItem.HorizontalContentAlignment);
        Assert.AreEqual(VerticalAlignment.Center, statusBarItem.VerticalContentAlignment);
    }

    private static void AssertStatusBarItemTemplateShape(StatusBarItem statusBarItem)
    {
        var border = VisualTreeTestHelper.FindDescendant<Border>(statusBarItem)
            ?? throw new AssertFailedException("Expected official WPF Fluent StatusBarItem border chrome.");
        var presenter = VisualTreeTestHelper.EnumerateDescendants(statusBarItem)
            .OfType<ContentPresenter>()
            .Single(item => Equals(item.Content, statusBarItem.Content));

        Assert.AreSame(statusBarItem.Background, border.Background);
        Assert.AreEqual(statusBarItem.Padding, border.Padding);
        Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
        Assert.AreEqual(statusBarItem.HorizontalContentAlignment, presenter.HorizontalAlignment);
        Assert.AreEqual(statusBarItem.VerticalContentAlignment, presenter.VerticalAlignment);
        Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(statusBarItem));
    }

    private static void AssertSetter(Setter[] setters, DependencyProperty property, object value)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.AreEqual(value, setter.Value);
    }

    private static void AssertBrushSetter(Setter[] setters, DependencyProperty property)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(SolidColorBrush));
    }

    private static void AssertBrushSetter(Setter[] setters, DependencyProperty property, Color color)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(SolidColorBrush));
        Assert.AreEqual(color, ((SolidColorBrush)setter.Value).Color);
    }

    private static void AssertDynamicResourceSetter(Setter[] setters, DependencyProperty property, object resourceKey)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertThemeResourceReference(string themeName, string key, object expectedResourceKey)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(theme.Contains(expectedResourceKey), $"Theme is missing {expectedResourceKey}.");
        Assert.AreSame(theme[expectedResourceKey], theme[key], key);
    }
}
